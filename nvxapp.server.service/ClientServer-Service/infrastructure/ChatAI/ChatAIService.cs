using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Serilog;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.RabbitMQ;
using nvxapp.server.service.RabbitMQ.Listener;
using nvxapp.server.service.ServerModels;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    public class ChatAIService : ServiceBase, IChatAIService
    {
        private readonly iRabbitMqConnection _rabbitMqConnection;
        private readonly IWebApiService _webApiService;
        private readonly ICommandRegistry _commandRegistry;
        private readonly IIntentCatalog _intentCatalog;

        // Sessioni in memoria — ConcurrentDictionary garantisce thread safety
        // su accessi concorrenti da richieste HTTP parallele.
        private static readonly ConcurrentDictionary<string, ChatSession> _sessions = new();

        private readonly string _ollamaUrl;
        private readonly string _ollamaModel;

        public ChatAIService(IMapper mapper,
                             UserManager<ApplicationUser> userManager,
                             IAspNetUsersRepository aspNetUsersRepository,
                             IOptions<JwtParameter> jwtParameter,
                             IHttpContextAccessor httpContextAccessor,
                             IConfiguration configuration,
                             iRabbitMqConnection rabbitMqConnection,
                             IWebApiService webApiService,
                             ICommandRegistry commandRegistry,
                             IIntentCatalog intentCatalog
                             ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _rabbitMqConnection = rabbitMqConnection;
            _webApiService = webApiService;
            _commandRegistry = commandRegistry;
            _intentCatalog = intentCatalog;

            _ollamaUrl = _configuration["AI:Url"] ?? "";
            _ollamaModel = _configuration["AI:model"] ?? "" ;

        }

        // ---------------------------------------------------------------------------
        // Entry point principale
        // ---------------------------------------------------------------------------

        public virtual async Task<GenericResult<ChatAIOutModel>> SendMessage(
            GenericRequest<ChatAIInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var userMessage = model.Data.Request;
                var sessionId = model.Data.SessionId;

                // 1. Carica o crea la sessione
                var session = GetOrCreateSession(sessionId);

                // 2. Comando di reset — l'utente vuole interrompere e ricominciare.
                // Riconosco il comando PRIMA di qualsiasi altro controllo, in modo che
                // funzioni sia durante la raccolta slot sia durante la conferma.
                if (IsResetCommand(userMessage))
                {
                    DeleteSession(session.SessionId);
                    var resetSession = new ChatSession();
                    _sessions[resetSession.SessionId] = resetSession;
                    return new ChatAIOutModel
                    {
                        SessionId    = resetSession.SessionId,
                        Responce     = "Operazione annullata. Puoi iniziare con un nuovo comando.",
                        ResponseType = "result",
                        Suggestions  = new List<string> { "Timbratura", "Ferie", "Malattia" }
                    };
                }

                // 3. Gestione conferma esplicita ("sì" / "no")
                if (session.State == SessionState.ReadyToExecute)
                    return await HandleConfirmation(session, userMessage);

                // 4. Registra il messaggio utente nella history PRIMA delle chiamate Ollama.
                // BuildChatMessages legge la history aggiornata — il messaggio corrente
                // è già incluso e non va passato separatamente.
                session.AddToHistory("user", userMessage);

                // 5. Primo turno vs turni successivi
                bool isFirstTurn = string.IsNullOrEmpty(session.Intent);

                if (isFirstTurn)
                {
                    // PRE-FILTRO: verifica che il testo contenga almeno una keyword
                    // di almeno uno degli intent noti. Evita di chiamare Ollama per
                    // testo palesemente non pertinente (es. "sooka", "vaffa", ecc.).
                    bool anyKeywordMatch = _intentCatalog.Intents
                        .Where(i => i.Keywords.Count > 0)
                        .Any(i => i.Keywords.Any(k =>
                            userMessage.Contains(k, StringComparison.OrdinalIgnoreCase)));

                    bool allIntentsHaveKeywords = _intentCatalog.Intents.All(i => i.Keywords.Count > 0);

                    if (allIntentsHaveKeywords && !anyKeywordMatch)
                    {
                        DeleteSession(session.SessionId);
                        return BuildErrorResponse(session, "Non ho capito la richiesta. Puoi ripetere?");
                    }

                    // PRIMO TURNO: chiedi a Ollama intent + tutti gli slot presenti nel messaggio
                    var extracted = await CallOllamaExtractIntentAsync(session);
                    if (extracted == null || string.IsNullOrEmpty(extracted.Intent))
                    {
                        // Sessione senza intent confermato — non ha senso mantenerla
                        DeleteSession(session.SessionId);
                        return BuildErrorResponse(session, "Non ho capito la richiesta. Puoi ripetere?");
                    }

                    // Rifiuta intent "unknown" o confidence troppo bassa
                    if (extracted.Intent.Equals("unknown", StringComparison.OrdinalIgnoreCase) || extracted.Confidence < 0.5)
                    {
                        DeleteSession(session.SessionId);
                        return BuildErrorResponse(session, "Non ho capito la richiesta. Puoi ripetere?");
                    }

                    // Verifica che l'intent restituito da Ollama esista nel catalogo.
                    // Se non esiste il modello sta allucinando — lo blocchiamo qui.
                    var knownIntent = _intentCatalog.Intents
                        .FirstOrDefault(i => i.Name.Equals(extracted.Intent, StringComparison.OrdinalIgnoreCase));
                    if (knownIntent == null)
                    {
                        DeleteSession(session.SessionId);
                        return BuildErrorResponse(session, $"Non conosco il comando '{extracted.Intent}'. Puoi ripetere con un'operazione valida?");
                    }

                    session.Intent = knownIntent.Name; // usa il nome canonico dal catalogo
                    SafeMergeSlots(session, extracted.Slots);
                }
                else
                {
                    // TURNI SUCCESSIVI: estrai SOLO il valore dello slot mancante atteso.
                    // NON richiamare l'estrazione generica — sovrascrive slot già raccolti.
                    var nextMissing = GetMissingRequiredSlots(session).FirstOrDefault();
                    if (nextMissing != null)
                    {
                        var slotValue = await CallOllamaExtractSingleSlotAsync(
                            nextMissing, session.Intent, session);

                        // Se Ollama non riesce, usa il testo grezzo come fallback
                        if (string.IsNullOrEmpty(slotValue))
                            slotValue = userMessage.Trim();

                        // Passa per SafeMergeSlots per applicare Validator e controllo PromptDescription
                        // anche nei turni successivi, non solo nel primo turno.
                        SafeMergeSlots(session, new Dictionary<string, string> { [nextMissing] = slotValue });
                    }
                }

                // 5. Valida i valori degli slot presenti (formato, range, ecc.)
                var formatValidation = ValidateSlotFormats(session);
                if (!formatValidation.IsValid)
                {
                    session.ResetSlot(formatValidation.InvalidSlotName);
                    session.AddToHistory("assistant", formatValidation.MessageToUser);
                    SaveSession(session);
                    return BuildQuestionResponse(session, formatValidation.MessageToUser, formatValidation.Suggestions);
                }

                // 6. Controlla se mancano slot obbligatori
                var missingSlots = GetMissingRequiredSlots(session);
                if (missingSlots.Any())
                {
                    var question = BuildMissingSlotQuestion(missingSlots.First(), session.Intent);
                    session.AddToHistory("assistant", question);
                    SaveSession(session);
                    return BuildQuestionResponse(session, question);
                }

                // 7. Tutti gli slot presenti e validi → chiedi conferma
                session.State = SessionState.ReadyToExecute;
                var summary = BuildConfirmationSummary(session);
                session.AddToHistory("assistant", summary);
                SaveSession(session);
                return BuildConfirmationResponse(session, summary);

            }, isSubProcess);
        }

        // ---------------------------------------------------------------------------
        // Gestione conferma utente
        // ---------------------------------------------------------------------------

        private async Task<ChatAIOutModel> HandleConfirmation(ChatSession session, string userMessage)
        {
            var lower = userMessage.Trim().ToLower();
            bool confirmed = lower is "sì" or "si" or "confermo" or "ok" or "yes";
            bool cancelled = lower is "no" or "annulla" or "cancel";

            session.AddToHistory("user", userMessage);

            if (confirmed)
            {
                session.State = SessionState.Confirmed;
                var result = await ExecuteCommandAsync(session);
                session.AddToHistory("assistant", result.Message);
                DeleteSession(session.SessionId);

                return new ChatAIOutModel
                {
                    SessionId    = session.SessionId,
                    Responce     = result.Message,
                    ResponseType = result.Success ? "result" : "error"
                };
            }

            if (cancelled)
            {
                session.AddToHistory("assistant", "Operazione annullata.");
                DeleteSession(session.SessionId);
                return new ChatAIOutModel
                {
                    SessionId    = session.SessionId,
                    Responce     = "Operazione annullata.",
                    ResponseType = "result"
                };
            }

            // Non è né sì né no — chiedi chiarimento
            var clarification = "Rispondere con 'sì' per confermare o 'no' per annullare.";
            session.State = SessionState.Collecting;
            session.AddToHistory("assistant", clarification);
            SaveSession(session);
            return BuildQuestionResponse(session, clarification, new List<string> { "Sì", "No" });
        }

        // ---------------------------------------------------------------------------
        // Reset — riconosce comandi di interruzione esplicita
        // ---------------------------------------------------------------------------

        private static bool IsResetCommand(string text)
        {
            var t = text.Trim().ToLower();
            return t is "annulla" or "reset" or "ricomincia" or "riparti" or "nuovo" or "nuova operazione"
                       or "stop" or "esci" or "basta" or "annulla tutto" or "restart";
        }

        // ---------------------------------------------------------------------------
        // Chiamata Ollama — primo turno: estrae intent + tutti gli slot presenti
        // Passa la ConversationHistory come array messages a /api/chat.
        // ---------------------------------------------------------------------------

        private async Task<ExtractedIntent?> CallOllamaExtractIntentAsync(ChatSession session)
        {
            try
            {

                //string ollamaUrl = _configuration["AI:Url"] ?? "" ;
                //string ollamaModel = _configuration["AI:model"] ?? "" ;

                var messages = BuildChatMessages(
                    _intentCatalog.BuildSystemPrompt(),
                    session.History);

                var requestBody = new OllamaChatRequest
                {
                    Model    = _ollamaModel,
                    Messages = messages,
                    Stream   = false,
                    Format   = "json"
                };

                var raw = await PostToOllamaChatAsync(_ollamaUrl, requestBody);
                return SafeDeserializeExtractedIntent(raw);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ChatAI] CallOllamaExtractIntentAsync fallita. Session={SessionId} Intent={Intent}",
                    session.SessionId, session.Intent);
                return null;
            }
        }

        // ---------------------------------------------------------------------------
        // Chiamata Ollama — turni successivi: estrae UN singolo slot dal testo.
        // Passa la ConversationHistory per permettere al modello di disambiguare
        // risposte contestuali (es. "quello di prima", "stessa data").
        // ---------------------------------------------------------------------------

        private async Task<string?> CallOllamaExtractSingleSlotAsync(
            string slotName, string intentName, ChatSession session)
        {
            try
            {
                //string ollamaUrl = _configuration["AI:Url"] ?? "";
                //string ollamaModel = _configuration["AI:model"] ?? "" ;

                var messages = BuildChatMessages(
                    BuildSingleSlotSystemPrompt(slotName, intentName),
                    session.History);

                var requestBody = new OllamaChatRequest
                {
                    Model    = _ollamaModel,
                    Messages = messages,
                    Stream   = false,
                    Format   = "json"
                };

                var raw = await PostToOllamaChatAsync(_ollamaUrl, requestBody);
                if (string.IsNullOrEmpty(raw)) return null;

                var start = raw.IndexOf('{');
                var end   = raw.LastIndexOf('}');
                if (start == -1 || end == -1) return null;

                var cleanJson = raw.Substring(start, end - start + 1);
                using var doc = JsonDocument.Parse(cleanJson);

                if (doc.RootElement.TryGetProperty("value", out var val))
                    return val.GetString();

                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ChatAI] CallOllamaExtractSingleSlotAsync fallita. Session={SessionId} Slot={SlotName}",
                    session.SessionId, slotName);
                return null;
            }
        }

        private string BuildSingleSlotSystemPrompt(string slotName, string intentName)
        {
            var slotDef  = FindSlotDefinition(intentName, slotName);
            var slotDesc = !string.IsNullOrEmpty(slotDef?.PromptDescription)
                ? slotDef.PromptDescription
                : "valore testuale";

            return
                "Estrai dal testo il valore di: " + slotDesc + "\n" +
                "Rispondi SOLO con questo JSON, nessun testo aggiuntivo:\n" +
                "{ \"value\": \"valore estratto\" }\n" +
                "Se il valore non è presente nel testo rispondi:\n" +
                "{ \"value\": null }";
        }

        // Cerca la SlotDefinition per nome nell'intent corrente della sessione.
        // Unico punto di accesso alla struttura dello slot — nessun switch sui nomi.
        private SlotDefinition? FindSlotDefinition(string intentName, string slotName) =>
            _intentCatalog.Intents
                .FirstOrDefault(i => i.Name == intentName)
                ?.Slots
                .FirstOrDefault(s => s.Name == slotName);

        // ---------------------------------------------------------------------------
        // HTTP helper — /api/chat (supporta messages array con history)
        // ---------------------------------------------------------------------------

        // Costruisce la lista messages: [system] + [history completa].
        // Il messaggio corrente dell'utente è già stato aggiunto alla history
        // in SendMessage prima di questa chiamata — non va passato separatamente.
        private static List<OllamaChatMessage> BuildChatMessages(
            string systemPrompt,
            List<ConversationTurn> history)
        {
            var messages = new List<OllamaChatMessage>
            {
                new() { Role = "system", Content = systemPrompt }
            };

            foreach (var turn in history)
                messages.Add(new OllamaChatMessage
                {
                    Role    = turn.Role,
                    Content = turn.Content
                });

            return messages;
        }

        private async Task<string> PostToOllamaChatAsync(string baseUrl, OllamaChatRequest requestBody)
        {
            var result = await _webApiService.Post<OllamaChatRequest, OllamaChatResponse>(
                serverUrl  : baseUrl,
                authentication: null,
                action     : "chat",
                body       : requestBody,
                bodyType   : WebApiBodyType.raw,
                headers    : new Dictionary<string, string>());

            if (result?.Data == null)
                throw new InvalidOperationException(
                    $"Ollama ha risposto con status {result?.StatusCode}: {result?.ReasonPhrase}");

            return result.Data.Message?.Content ?? string.Empty;
        }

        // ---------------------------------------------------------------------------
        // Deserializzazione sicura
        // ---------------------------------------------------------------------------

        private ExtractedIntent? SafeDeserializeExtractedIntent(string raw)
        {
            try
            {
                if (string.IsNullOrEmpty(raw)) return null;

                var start = raw.IndexOf('{');
                var end = raw.LastIndexOf('}');
                if (start == -1 || end == -1) return null;

                var cleanJson = raw.Substring(start, end - start + 1);
                return JsonSerializer.Deserialize<ExtractedIntent>(cleanJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch
            {
                return null;
            }
        }

        // ---------------------------------------------------------------------------
        // Merge sicuro degli slot — FIX del bug principale
        // ---------------------------------------------------------------------------

        // Aggiunge alla sessione SOLO i valori realmente presenti.
        // Non tocca MAI gli slot già valorizzati in sessione.
        // Se lo slot ha un Validator e il valore non supera la validazione, viene scartato:
        // questo impedisce che hallucination del LLM (es. "timbratura" come nome dipendente)
        // vengano accettate e saltino la raccolta del dato reale.
        private void SafeMergeSlots(ChatSession session, Dictionary<string, string> newSlots)
        {
            if (newSlots == null) return;

            var intentDef = _intentCatalog.Intents
                .FirstOrDefault(i => i.Name == session.Intent);

            foreach (var kv in newSlots)
            {
                if (IsNullString(kv.Value)) continue; // ignora null/vuoti
                if (session.Slots.ContainsKey(kv.Key)) continue; // non sovrascrivere esistenti

                var slotDef = intentDef?.Slots.FirstOrDefault(s => s.Name == kv.Key);

                // Scarta il valore se Ollama ha restituito la PromptDescription verbatim
                // (hallucination classica: il modello copia la descrizione come valore)
                if (slotDef != null &&
                    !string.IsNullOrEmpty(slotDef.PromptDescription) &&
                    kv.Value.Equals(slotDef.PromptDescription, StringComparison.OrdinalIgnoreCase))
                    continue;

                // Se lo slot ha un validator di formato, rigetta valori che non lo superano
                if (slotDef?.Validator != null && slotDef.Validator(kv.Value) != null)
                    continue; // valore non valido — verrà chiesto all'utente

                session.Slots[kv.Key] = kv.Value;
            }
        }

        private bool IsNullString(string value) =>
            string.IsNullOrWhiteSpace(value) ||
            value.Equals("null", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("unknown", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("n/a", StringComparison.OrdinalIgnoreCase);

        // ---------------------------------------------------------------------------
        // Validazione slot — delega ai Validator definiti nel handler.
        // ChatAIService non conosce i nomi degli slot: itera sui metadati dell'intent.
        // ---------------------------------------------------------------------------

        private SlotValidationResult ValidateSlotFormats(ChatSession session)
        {
            var intentDef = _intentCatalog.Intents
                .FirstOrDefault(i => i.Name == session.Intent);

            if (intentDef == null) return SlotValidationResult.Ok();

            // Validazione singolo slot — Validator è definito nel handler
            foreach (var slotDef in intentDef.Slots)
            {
                if (slotDef.Validator == null) continue;
                if (!session.Slots.TryGetValue(slotDef.Name, out var value)) continue;
                var result = slotDef.Validator(value);
                if (result != null) return result;
            }

            // Validazione cross-slot — CrossValidator è definito nell'IntentDefinition
            if (intentDef.CrossValidator != null)
            {
                var crossResult = intentDef.CrossValidator(session.Slots);
                if (crossResult != null) return crossResult;
            }

            return SlotValidationResult.Ok();
        }

        // ---------------------------------------------------------------------------
        // Slot mancanti
        // ---------------------------------------------------------------------------

        private List<string> GetMissingRequiredSlots(ChatSession session)
        {
            var intentDef = _intentCatalog.Intents
                .FirstOrDefault(i => i.Name == session.Intent);

            if (intentDef == null) return new();

            return intentDef.Slots
                .Where(s => s.Required && !session.Slots.ContainsKey(s.Name))
                .Select(s => s.Name)
                .ToList();
        }

        private string BuildMissingSlotQuestion(string slotName, string intentName)
        {
            var slotDef = FindSlotDefinition(intentName, slotName);
            return !string.IsNullOrEmpty(slotDef?.Question)
                ? slotDef.Question
                : $"Puoi specificare '{slotName}'?";
        }

        // ---------------------------------------------------------------------------
        // Esecuzione comando — delega al CommandRegistry
        // ---------------------------------------------------------------------------

        private async Task<CommandResult> ExecuteCommandAsync(ChatSession session)
        {
            try
            {
                var handler = _commandRegistry.Resolve(session.Intent);
                return await handler.ExecuteAsync(session.Slots);
            }
            catch (InvalidOperationException ex)
            {
                // Handler non trovato nel registry — errore di configurazione atteso
                Log.Warning(ex, "[ChatAI] Handler non trovato. Session={SessionId} Intent={Intent}",
                    session.SessionId, session.Intent);
                return CommandResult.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                // Errore imprevisto nell'esecuzione del comando (DB, timeout, ecc.)
                Log.Error(ex, "[ChatAI] ExecuteCommandAsync fallita. Session={SessionId} Intent={Intent} Slots={Slots}",
                    session.SessionId, session.Intent, System.Text.Json.JsonSerializer.Serialize(session.Slots));
                return CommandResult.Fail("Errore durante l'esecuzione del comando. Riprova.");
            }
        }

        // ---------------------------------------------------------------------------
        // Riepilogo conferma
        // ---------------------------------------------------------------------------

        private string BuildConfirmationSummary(ChatSession session)
        {
            var intentDef = _intentCatalog.Intents.FirstOrDefault(i => i.Name == session.Intent);
            var sb = new StringBuilder();

            sb.AppendLine("Riepilogo:");

            if (intentDef != null)
            {
                foreach (var slotDef in intentDef.Slots)
                {
                    var value = session.Slots.GetValueOrDefault(slotDef.Name,
                        !string.IsNullOrEmpty(slotDef.Default) ? slotDef.Default : "-");
                    var label = !string.IsNullOrEmpty(slotDef.Label) ? slotDef.Label : slotDef.Name;
                    sb.AppendLine($"  {label}: {value}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("Confermi? (sì / no)");
            return sb.ToString();
        }

        // ---------------------------------------------------------------------------
        // Helper risposta
        // ---------------------------------------------------------------------------

        private ChatAIOutModel BuildQuestionResponse(ChatSession session, string message,
            List<string>?  suggestions = null) =>
            new()
            {
                SessionId = session.SessionId,
                Responce = message,
                ResponseType = "question",
                Suggestions = suggestions ?? new()
            };

        private ChatAIOutModel BuildConfirmationResponse(ChatSession session, string message) =>
            new()
            {
                SessionId = session.SessionId,
                Responce = message,
                ResponseType = "confirmation",
                Suggestions = new List<string> { "Sì", "No" }
            };

        private ChatAIOutModel BuildErrorResponse(ChatSession session, string message) =>
            new()
            {
                SessionId = session.SessionId,
                Responce = message,
                ResponseType = "error"
            };

        // ---------------------------------------------------------------------------
        // Session store in memoria
        // ---------------------------------------------------------------------------

        private ChatSession GetOrCreateSession(string sessionId)
        {
            if (!string.IsNullOrEmpty(sessionId) &&
                _sessions.TryGetValue(sessionId, out var existing))
            {
                if ((DateTime.UtcNow - existing.LastActivity).TotalMinutes < 10)
                    return existing;

                _sessions.TryRemove(sessionId, out _);
            }

            var newSession = new ChatSession();
            _sessions[newSession.SessionId] = newSession;
            return newSession;
        }

        // Con ConcurrentDictionary la sessione è già aggiornata per riferimento —
        // SaveSession resta per chiarezza semantica ma non fa una copia.
        private void SaveSession(ChatSession session) =>
            _sessions[session.SessionId] = session;

        private void DeleteSession(string sessionId) =>
            _sessions.TryRemove(sessionId, out _);


        #region "RabbitMq NON ELIMINARE"

        // ---------------------------------------------------------------------------
        // RabbitMQ
        // ---------------------------------------------------------------------------

        //private async Task PublishToRabbitMqAsync(string message)
        //{
        //    if (_rabbitMqConnection == null) return;

        //    await _rabbitMqConnection.Start();

        //    if (_rabbitMqConnection._channel != null)
        //    {
        //        await _rabbitMqConnection._channel.QueueDeclareAsync(
        //            queue: RabbitMqParameter.QueueName_Demo,
        //            durable: false, exclusive: false, autoDelete: false);

        //        var body = Encoding.UTF8.GetBytes(message);

        //        await _rabbitMqConnection._channel.BasicPublishAsync(
        //            exchange: RabbitMqParameter.Default_Exchange,
        //            routingKey: RabbitMqParameter.RoutingKey_Demo,
        //            body: body);

        //        await _rabbitMqConnection.Stop(10);
        //    }
        //}

         #endregion

    }

    // ---------------------------------------------------------------------------
    // Interfaccia
    // ---------------------------------------------------------------------------

    public interface IChatAIService : IServiceBase
    {
        Task<GenericResult<ChatAIOutModel>> SendMessage(
            GenericRequest<ChatAIInModel> model, bool isSubProcess);
    }

    // ---------------------------------------------------------------------------
    // FakeAI_Regex (invariato)
    // ---------------------------------------------------------------------------

    //public class FakeAI_Regex
    //{
    //    private List<Regex> Clockign_ENT;

    //    public FakeAI_Regex()
    //    {
    //        Clockign_ENT = new List<Regex>();
    //        Clockign_ENT.Add(new Regex(
    //            @"([a-zA-Z\s]+)\s(entrata|entra|ent|uscita|esce|usc)(?:\salle)?\s(\d{1,2}(?:([:\.]\d{2})|(?:\se\s\d{1,2})))"));
    //    }

    //    public List<ClockignCommand> GetClockignCommand(string text)
    //    {
    //        var clockignCommands = new List<ClockignCommand>();
    //        foreach (var item in Clockign_ENT)
    //        {
    //            if (item.IsMatch(text))
    //            {
    //                var match = item.Match(text);
    //                clockignCommands.Add(new ClockignCommand
    //                {
    //                    action = match.Groups[2].Value,
    //                    dipendente = match.Groups[1].Value,
    //                    orario = match.Groups[3].Value,
    //                    type = "timbratura"
    //                });
    //            }
    //        }
    //        return clockignCommands;
    //    }
    //}

    //public class ClockignCommand
    //{
    //    public string action { get; set; } = string.Empty;
    //    public string type { get; set; } = string.Empty;
    //    public string orario { get; set; } = string.Empty;
    //    public string dipendente { get; set; } = string.Empty;
    //}
}
