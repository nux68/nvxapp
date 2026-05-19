using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.RabbitMQ;
using nvxapp.server.service.RabbitMQ.Listener;
using nvxapp.server.service.ServerModels;
using RabbitMQ.Client;
using System.Collections.Concurrent;
using System.Text;
using System.Text.Json;
using System.Text.RegularExpressions;


namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    public class ChatAIService : ServiceBase, IChatAIService
    {
        private readonly iRabbitMqConnection _rabbitMqConnection;
        private readonly IHttpClientFactory _httpClientFactory;
        private readonly ICommandRegistry _commandRegistry;
        private readonly IIntentCatalog _intentCatalog;

        // Sessioni in memoria — ConcurrentDictionary garantisce thread safety
        // su accessi concorrenti da richieste HTTP parallele.
        private static readonly ConcurrentDictionary<string, ChatSession> _sessions = new();

        private readonly string _ollamaModel = "qwen2.5:3b";

        public ChatAIService(IMapper mapper,
                             UserManager<ApplicationUser> userManager,
                             IAspNetUsersRepository aspNetUsersRepository,
                             IOptions<JwtParameter> jwtParameter,
                             IHttpContextAccessor httpContextAccessor,
                             IConfiguration configuration,
                             iRabbitMqConnection rabbitMqConnection,
                             IHttpClientFactory httpClientFactory,
                             ICommandRegistry commandRegistry,
                             IIntentCatalog intentCatalog
                             ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _rabbitMqConnection = rabbitMqConnection;
            _httpClientFactory = httpClientFactory;
            _commandRegistry = commandRegistry;
            _intentCatalog = intentCatalog;
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

                // 2. Gestione conferma esplicita ("sì" / "no")
                if (session.State == SessionState.ReadyToExecute)
                    return await HandleConfirmation(session, userMessage);

                // 3. Primo turno vs turni successivi — questo è il FIX del loop
                bool isFirstTurn = string.IsNullOrEmpty(session.Intent);

                if (isFirstTurn)
                {
                    // PRIMO TURNO: chiedi a Ollama intent + tutti gli slot presenti nel messaggio
                    var extracted = await CallOllamaExtractIntentAsync(userMessage, session);
                    if (extracted == null || string.IsNullOrEmpty(extracted.Intent))
                        return BuildErrorResponse(session, "Non ho capito la richiesta. Puoi ripetere?");

                    session.Intent = extracted.Intent;
                    SafeMergeSlots(session, extracted.Slots);
                }
                else
                {
                    // TURNI SUCCESSIVI: l'utente sta rispondendo a una domanda specifica.
                    // NON richiamare Ollama per l'estrazione generica —
                    // quella era la causa del reset degli slot già raccolti.
                    // Ollama riceveva solo la risposta parziale ("mario rossi") e
                    // restituiva tutti gli altri slot come null, sovrascrivendoli.
                    var nextMissing = GetMissingRequiredSlots(session).FirstOrDefault();
                    if (nextMissing != null)
                    {
                        // Chiamata Ollama focalizzata: estrai SOLO il valore di questo slot
                        var slotValue = await CallOllamaExtractSingleSlotAsync(
                            userMessage, nextMissing, session.Intent, session);

                        // Se Ollama non riesce, usa il testo grezzo come fallback
                        if ( string.IsNullOrEmpty(slotValue))
                             slotValue = userMessage.Trim();

                        session.Slots[nextMissing] = slotValue;
                    }
                }

                session.AddToHistory("user", userMessage);

                // 4. Valida i valori degli slot presenti (formato, range, ecc.)
                var formatValidation = ValidateSlotFormats(session.Slots);
                if (!formatValidation.IsValid)
                {
                    session.ResetSlot(formatValidation.InvalidSlotName);
                    SaveSession(session);
                    return BuildQuestionResponse(session, formatValidation.MessageToUser, formatValidation.Suggestions);
                }

                // 5. Controlla se mancano slot obbligatori
                var missingSlots = GetMissingRequiredSlots(session);
                if (missingSlots.Any())
                {
                    var question = BuildMissingSlotQuestion(missingSlots.First());
                    session.AddToHistory("assistant", question);
                    SaveSession(session);
                    return BuildQuestionResponse(session, question);
                }

                // 6. Tutti gli slot presenti e validi → chiedi conferma
                session.State = SessionState.ReadyToExecute;
                SaveSession(session);
                var summary = BuildConfirmationSummary(session);
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

            if (confirmed)
            {
                session.State = SessionState.Confirmed;
                var result = await ExecuteCommandAsync(session);
                DeleteSession(session.SessionId);

                return new ChatAIOutModel
                {
                    SessionId = session.SessionId,
                    Responce = result.Message,
                    ResponseType = result.Success ? "result" : "error"
                };
            }

            if (cancelled)
            {
                DeleteSession(session.SessionId);
                return new ChatAIOutModel
                {
                    SessionId = session.SessionId,
                    Responce = "Operazione annullata.",
                    ResponseType = "result"
                };
            }

            // Non è né sì né no
            session.State = SessionState.Collecting;
            SaveSession(session);
            return BuildQuestionResponse(session,
                "Rispondere con 'sì' per confermare o 'no' per annullare.",
                new List<string> { "Sì", "No" });
        }

        // ---------------------------------------------------------------------------
        // Chiamata Ollama — primo turno: estrae intent + tutti gli slot presenti
        // Passa la ConversationHistory come array messages a /api/chat.
        // ---------------------------------------------------------------------------

        private async Task<ExtractedIntent?> CallOllamaExtractIntentAsync(
            string userMessage, ChatSession session)
        {
            try
            {
                string ollamaUrl = _configuration["AI:Url"] ?? "http://localhost:11434/api/";

                var messages = BuildChatMessages(
                    _intentCatalog.BuildSystemPrompt(),
                    session.History,
                    userMessage);

                var requestBody = new OllamaChatRequest
                {
                    Model    = _ollamaModel,
                    Messages = messages,
                    Stream   = false,
                    Format   = "json"
                };

                var raw = await PostToOllamaChatAsync(ollamaUrl, requestBody);
                return SafeDeserializeExtractedIntent(raw);
            }
            catch
            {
                return null;
            }
        }

        // ---------------------------------------------------------------------------
        // Chiamata Ollama — turni successivi: estrae UN singolo slot dal testo.
        // Passa la ConversationHistory per permettere al modello di disambiguare
        // risposte contestuali (es. "quello di prima", "stessa data").
        // ---------------------------------------------------------------------------

        private async Task<string?> CallOllamaExtractSingleSlotAsync(
            string userMessage, string slotName, string intentName, ChatSession session)
        {
            try
            {
                string ollamaUrl = _configuration["AI:Url"] ?? "http://localhost:11434/api/";

                var messages = BuildChatMessages(
                    BuildSingleSlotSystemPrompt(slotName),
                    session.History,
                    userMessage);

                var requestBody = new OllamaChatRequest
                {
                    Model    = _ollamaModel,
                    Messages = messages,
                    Stream   = false,
                    Format   = "json"
                };

                var raw = await PostToOllamaChatAsync(ollamaUrl, requestBody);
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
            catch
            {
                return null;
            }
        }

        private string BuildSingleSlotSystemPrompt(string slotName)
        {
            var slotDesc = slotName switch
            {
                "employeeName" => "nome e cognome di una persona",
                "time" => $"orario nel formato HH:mm. Se l'utente dice 'alle 9' restituisci '09:00'.",
                "date" => $"data nel formato yyyy-MM-dd. Oggi è {DateTime.Today:yyyy-MM-dd}. Se dice 'oggi' restituisci '{DateTime.Today:yyyy-MM-dd}'.",
                "startDate" => $"data di inizio nel formato yyyy-MM-dd. Oggi è {DateTime.Today:yyyy-MM-dd}.",
                "endDate" => $"data di fine nel formato yyyy-MM-dd. Oggi è {DateTime.Today:yyyy-MM-dd}.",
                "direction" => "valore IN oppure OUT. Se dice 'entrata' restituisci IN, se dice 'uscita' restituisci OUT.",
                "certificateNumber" => "codice o numero del certificato medico",
                _ => "valore testuale"
            };

            return
        "Estrai dal testo il valore di: " + slotDesc + "\n" +
        "Rispondi SOLO con questo JSON, nessun testo aggiuntivo:\n" +
        "{ \"value\": \"valore estratto\" }\n" +
        "Se il valore non è presente nel testo rispondi:\n" +
        "{ \"value\": null }";

        }

        // ---------------------------------------------------------------------------
        // HTTP helper — /api/chat (supporta messages array con history)
        // ---------------------------------------------------------------------------

        // Costruisce la lista messages: [system] + [history] + [messaggio corrente].
        // Il system prompt va sempre come primo messaggio con role="system".
        // La history accumula i turni precedenti dando contesto al modello.
        private static List<OllamaChatMessage> BuildChatMessages(
            string systemPrompt,
            List<ConversationTurn> history,
            string currentUserMessage)
        {
            var messages = new List<OllamaChatMessage>
            {
                new() { Role = "system", Content = systemPrompt }
            };

            foreach (var turn in history)
                messages.Add(new OllamaChatMessage
                {
                    Role    = turn.Role,    // "user" | "assistant"
                    Content = turn.Content
                });

            messages.Add(new OllamaChatMessage
            {
                Role    = "user",
                Content = currentUserMessage
            });

            return messages;
        }

        private async Task<string> PostToOllamaChatAsync(string baseUrl, OllamaChatRequest requestBody)
        {
            var json    = JsonSerializer.Serialize(requestBody);
            var content = new StringContent(json, Encoding.UTF8, "application/json");
            var httpClient = _httpClientFactory.CreateClient();

            var response = await httpClient.PostAsync($"{baseUrl}chat", content);
            response.EnsureSuccessStatusCode();

            var responseBody  = await response.Content.ReadAsStringAsync();
            var ollamaResponse = JsonSerializer.Deserialize<OllamaChatResponse>(responseBody);

            return ollamaResponse?.Message?.Content ?? string.Empty;
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
        private void SafeMergeSlots(ChatSession session, Dictionary<string, string> newSlots)
        {
            if (newSlots == null) return;

            foreach (var kv in newSlots)
            {
                if (IsNullString(kv.Value)) continue; // ignora null/vuoti
                if (session.Slots.ContainsKey(kv.Key)) continue; // non sovrascrivere esistenti

                session.Slots[kv.Key] = kv.Value;
            }
        }

        private bool IsNullString(string value) =>
            string.IsNullOrWhiteSpace(value) ||
            value.Equals("null", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("unknown", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("n/a", StringComparison.OrdinalIgnoreCase);

        // ---------------------------------------------------------------------------
        // Validazione formato slot
        // ---------------------------------------------------------------------------

        private SlotValidationResult ValidateSlotFormats(Dictionary<string, string> slots)
        {
            if (slots.TryGetValue("time", out var time))
            {
                if (!TimeOnly.TryParse(time, out _))
                    return SlotValidationResult.Failed(
                        SlotValidationError.InvalidFormat,
                        $"'{time}' non è un orario valido. Usa il formato HH:mm (es. 09:00).",
                        "time");
            }

            if (slots.TryGetValue("date", out var date))
            {
                if (!DateOnly.TryParse(date, out _))
                    return SlotValidationResult.Failed(
                        SlotValidationError.InvalidFormat,
                        $"'{date}' non è una data valida. Usa il formato gg/mm/aaaa.",
                        "date");
            }

            if (slots.TryGetValue("startDate", out var startD) &&
                slots.TryGetValue("endDate", out var endD))
            {
                if (DateOnly.TryParse(startD, out var s) &&
                    DateOnly.TryParse(endD, out var e) && s > e)
                    return SlotValidationResult.Failed(
                        SlotValidationError.BusinessRuleViolation,
                        "La data di inizio non può essere successiva alla data di fine.",
                        "startDate");
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

        private string BuildMissingSlotQuestion(string slotName) => slotName switch
        {
            "employeeName" => "Per quale dipendente?",
            "time" => "A che orario? (es. 09:00)",
            "date" => "Per quale data?",
            "direction" => "Entrata o uscita?",
            "startDate" => "Da quale data?",
            "endDate" => "Fino a quale data?",
            "certificateNumber" => "Hai il numero del certificato medico? (premi invio per saltare)",
            _ => $"Puoi specificare '{slotName}'?"
        };

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
                return CommandResult.Fail(ex.Message);
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
                    sb.AppendLine($"  {SlotLabel(slotDef.Name)}: {value}");
                }
            }

            sb.AppendLine();
            sb.AppendLine("Confermi? (sì / no)");
            return sb.ToString();
        }

        private string SlotLabel(string slotName) => slotName switch
        {
            "employeeName" => "Dipendente",
            "time" => "Orario",
            "date" => "Data",
            "direction" => "Tipo",
            "startDate" => "Dal",
            "endDate" => "Al",
            "certificateNumber" => "Certificato",
            _ => slotName
        };

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

        // ---------------------------------------------------------------------------
        // RabbitMQ
        // ---------------------------------------------------------------------------

        private async Task PublishToRabbitMqAsync(string message)
        {
            if (_rabbitMqConnection == null) return;

            await _rabbitMqConnection.Start();

            if (_rabbitMqConnection._channel != null)
            {
                await _rabbitMqConnection._channel.QueueDeclareAsync(
                    queue: RabbitMqParameter.QueueName_Demo,
                    durable: false, exclusive: false, autoDelete: false);

                var body = Encoding.UTF8.GetBytes(message);

                await _rabbitMqConnection._channel.BasicPublishAsync(
                    exchange: RabbitMqParameter.Default_Exchange,
                    routingKey: RabbitMqParameter.RoutingKey_Demo,
                    body: body);

                await _rabbitMqConnection.Stop(10);
            }
        }
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

    public class FakeAI_Regex
    {
        private List<Regex> Clockign_ENT;

        public FakeAI_Regex()
        {
            Clockign_ENT = new List<Regex>();
            Clockign_ENT.Add(new Regex(
                @"([a-zA-Z\s]+)\s(entrata|entra|ent|uscita|esce|usc)(?:\salle)?\s(\d{1,2}(?:([:\.]\d{2})|(?:\se\s\d{1,2})))"));
        }

        public List<ClockignCommand> GetClockignCommand(string text)
        {
            var clockignCommands = new List<ClockignCommand>();
            foreach (var item in Clockign_ENT)
            {
                if (item.IsMatch(text))
                {
                    var match = item.Match(text);
                    clockignCommands.Add(new ClockignCommand
                    {
                        action = match.Groups[2].Value,
                        dipendente = match.Groups[1].Value,
                        orario = match.Groups[3].Value,
                        type = "timbratura"
                    });
                }
            }
            return clockignCommands;
        }
    }

    public class ClockignCommand
    {
        public string action { get; set; } = string.Empty;
        public string type { get; set; } = string.Empty;
        public string orario { get; set; } = string.Empty;
        public string dipendente { get; set; } = string.Empty;
    }
}
