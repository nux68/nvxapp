using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.RabbitMQ;
using nvxapp.server.service.ServerModels;
using Serilog;
using System.Text;
using System.Text.Json;


namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    public class ChatAIService : ServiceBase, IChatAIService
    {
        private readonly iRabbitMqConnection _rabbitMqConnection;
        private readonly IWebApiService _webApiService;
        private readonly ICommandRegistry _commandRegistry;
        private readonly IIntentCatalog _intentCatalog;
        private readonly IChatSessionStore _sessionStore;

        private readonly string _ollamaUrl;
        private readonly string _ollamaModel;
        private readonly string _ollamaMethod;

        private readonly string _openrouterUrl;
        private readonly string _openrouterModel;
        private readonly string _openrouterMethod;
        private readonly string _openrouterApiKey;

        private readonly string _groqUrl;
        private readonly string _groqModel;
        private readonly string _groqMethod;
        private readonly string _groqApiKey;

        private readonly int _maxHistoryTurns;
        private readonly string _useLLM;

        public ChatAIService(IMapper mapper,
                             UserManager<ApplicationUser> userManager,
                             IAspNetUsersRepository aspNetUsersRepository,
                             IOptions<JwtParameter> jwtParameter,
                             IHttpContextAccessor httpContextAccessor,
                             IConfiguration configuration,
                             IHostEnvironment env,
                             iRabbitMqConnection rabbitMqConnection,
                             IWebApiService webApiService,
                             ICommandRegistry commandRegistry,
                             IIntentCatalog intentCatalog,
                             IChatSessionStore sessionStore
                             ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _rabbitMqConnection = rabbitMqConnection;
            _webApiService = webApiService;
            _commandRegistry = commandRegistry;
            _intentCatalog = intentCatalog;
            _sessionStore = sessionStore;

            _maxHistoryTurns = int.TryParse(configuration["AI:MaxHistoryTurns"], out var n) && n > 0 ? n : 20;

            _useLLM = configuration["AI:UseLLM"] ?? throw new InvalidOperationException("UseLLM non configurato");

            _ollamaUrl = configuration["AI:AI_local:Url"] ?? throw new InvalidOperationException("AI_local:Url non configurato");
            _ollamaModel = configuration["AI:AI_local:model"] ?? throw new InvalidOperationException("AI_local:model non configurato");
            _ollamaMethod = configuration["AI:AI_local:method"] ?? throw new InvalidOperationException("AI_local:method non configurato");

            _openrouterUrl = configuration["AI:AI_openrouter:Url"] ?? throw new InvalidOperationException("AI_openrouter:Url non configurato");
            _openrouterModel = configuration["AI:AI_openrouter:model"] ?? throw new InvalidOperationException("AI_openrouter:model non configurato");
            _openrouterMethod = configuration["AI:AI_openrouter:method"] ?? throw new InvalidOperationException("AI_openrouter:method non configurato");
            _openrouterApiKey = ReadApiKeyFromFile(env.ContentRootPath, "openrouter.key");

            _groqUrl = configuration["AI:AI_groq:Url"] ?? throw new InvalidOperationException("AI_groq:Url non configurato");
            _groqModel = configuration["AI:AI_groq:model"] ?? throw new InvalidOperationException("AI_groq:model non configurato");
            _groqMethod = configuration["AI:AI_groq:method"] ?? throw new InvalidOperationException("AI_groq:method non configurato");
            _groqApiKey = ReadApiKeyFromFile(env.ContentRootPath, "groq.key");



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

                // 1. Cerca la sessione esistente
                var session = TryGetSession(sessionId);

                // Se il client aveva una sessione (sessionId non vuoto) ma è scaduta,
                // lo informiamo esplicitamente invece di ripartire in silenzio.
                if (session == null && !string.IsNullOrEmpty(sessionId))
                {
                    var freshSession = CreateSession();
                    return new ChatAIOutModel
                    {
                        SessionId = freshSession.SessionId,
                        Responce = "La sessione precedente è scaduta. Puoi iniziare con un nuovo comando.",
                        ResponseType = "result",
                        Suggestions = BuildIntentSuggestions()
                    };
                }

                // Prima richiesta senza sessionId — crea la sessione
                session ??= CreateSession();

                // 2. Comando di reset — l'utente vuole interrompere e ricominciare.
                // Riconosco il comando PRIMA di qualsiasi altro controllo, in modo che
                // funzioni sia durante la raccolta slot sia durante la conferma.
                if (IsResetCommand(userMessage))
                {
                    _sessionStore.Delete(session.SessionId);
                    var resetSession = _sessionStore.Create();
                    return new ChatAIOutModel
                    {
                        SessionId = resetSession.SessionId,
                        Responce = "Operazione annullata. Puoi iniziare con un nuovo comando.",
                        ResponseType = "result",
                        Suggestions = BuildIntentSuggestions()
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
                        return BuildErrorResponse(session, "Non ho capito la richiesta. Puoi ripetere?", BuildIntentSuggestions());
                    }

                    // PRIMO TURNO: chiedi a Ollama intent + tutti gli slot presenti nel messaggio
                    var extracted = await Call_LLM_ExtractIntentAsync(session);
                    if (extracted == null || string.IsNullOrEmpty(extracted.Intent))
                    {
                        // Sessione senza intent confermato — non ha senso mantenerla
                        DeleteSession(session.SessionId);
                        return BuildErrorResponse(session, "Non ho capito la richiesta. Puoi ripetere?", BuildIntentSuggestions());
                    }

                    // Rifiuta intent "unknown" o confidence troppo bassa
                    if (extracted.Intent.Equals("unknown", StringComparison.OrdinalIgnoreCase) || extracted.Confidence < 0.5)
                    {
                        DeleteSession(session.SessionId);
                        return BuildErrorResponse(session, "Non ho capito la richiesta. Puoi ripetere?", BuildIntentSuggestions());
                    }

                    // Verifica che l'intent restituito da Ollama esista nel catalogo.
                    // Se non esiste il modello sta allucinando — lo blocchiamo qui.
                    var knownIntent = _intentCatalog.Intents
                        .FirstOrDefault(i => i.Name.Equals(extracted.Intent, StringComparison.OrdinalIgnoreCase));
                    if (knownIntent == null)
                    {
                        DeleteSession(session.SessionId);
                        return BuildErrorResponse(session, $"Non conosco il comando '{extracted.Intent}'. Puoi ripetere con un'operazione valida?", BuildIntentSuggestions());
                    }

                    session.Intent = knownIntent.Name; // usa il nome canonico dal catalogo
                    SafeMergeSlots(session, extracted.Slots, userMessage);
                }
                else
                {
                    // TURNI SUCCESSIVI: estrai SOLO il valore dello slot mancante atteso.
                    // NON richiamare l'estrazione generica — sovrascrive slot già raccolti.
                    var nextMissing = GetMissingRequiredSlots(session).FirstOrDefault();
                    if (nextMissing != null)
                    {
                        var slotValue = await Call_LLM_ExtractSingleSlotAsync(
                            nextMissing, session.Intent, session);

                        // Se Ollama non riesce, usa il testo grezzo come fallback
                        if (string.IsNullOrEmpty(slotValue))
                            slotValue = userMessage.Trim();

                        // Passa per SafeMergeSlots con userMessage per applicare tutti i guard
                        // anche nei turni successivi, non solo nel primo turno.
                        SafeMergeSlots(session, new Dictionary<string, string> { [nextMissing] = slotValue }, userMessage);
                    }
                }

                // 5. Valida i valori degli slot presenti (formato, range, ecc.)
                var formatValidation = ValidateSlotFormats(session);
                if (!formatValidation.IsValid)
                {
                    session.ResetSlot(formatValidation.InvalidSlotName);
                    session.AddToHistory("assistant", formatValidation.MessageToUser);
                    return BuildQuestionResponse(session, formatValidation.MessageToUser, formatValidation.Suggestions);
                }

                // 6. Controlla se mancano slot obbligatori
                var missingSlots = GetMissingRequiredSlots(session);
                if (missingSlots.Any())
                {
                    var question = BuildMissingSlotQuestion(missingSlots.First(), session.Intent);
                    session.AddToHistory("assistant", question);
                    return BuildQuestionResponse(session, question);
                }

                // 7. Tutti gli slot presenti e validi → chiedi conferma
                session.State = SessionState.ReadyToExecute;
                var summary = BuildConfirmationSummary(session);
                session.AddToHistory("assistant", summary);
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
                DeleteSession(session.SessionId);
                var doneSession = _sessionStore.Create();

                return new ChatAIOutModel
                {
                    SessionId = doneSession.SessionId,
                    Responce = result.Message,
                    ResponseType = result.Success ? "result" : "error",
                    Suggestions = result.Success ? BuildIntentSuggestions() : new()
                };
            }

            if (cancelled)
            {
                DeleteSession(session.SessionId);
                var freshSession = _sessionStore.Create();
                return new ChatAIOutModel
                {
                    SessionId = freshSession.SessionId,
                    Responce = "Operazione annullata.",
                    ResponseType = "result",
                    Suggestions = BuildIntentSuggestions()
                };
            }

            // Non è né sì né no — chiedi chiarimento
            var clarification = "Rispondere con 'sì' per confermare o 'no' per annullare.";
            session.State = SessionState.Collecting;
            session.AddToHistory("assistant", clarification);
            return BuildQuestionResponse(session, clarification, new List<string> { "Sì", "No" });
        }

        // ---------------------------------------------------------------------------
        // Reset — riconosce comandi di interruzione esplicita
        // ---------------------------------------------------------------------------

        private static readonly string[] _resetKeywords =
        [
            "annulla", "reset", "ricomincia", "riparti", "nuovo", "stop",
            "esci", "basta", "restart", "cancella", "abbandona", "interrompi"
        ];

        private static bool IsResetCommand(string text)
        {
            var t = text.Trim().ToLower();
            return _resetKeywords.Any(k => t.Contains(k));
        }

        // ---------------------------------------------------------------------------
        // Chiamata Ollama — primo turno: estrae intent + tutti gli slot presenti
        // Passa la ConversationHistory come array messages a /api/chat.
        // ---------------------------------------------------------------------------

        private async Task<ExtractedIntent?> Call_LLM_ExtractIntentAsync(ChatSession session)
        {
            try
            {
                var messages = BuildChatMessages(_intentCatalog.BuildSystemPrompt(null), session.History);

                var requestBody = new OllamaChatRequest
                {
                    Model = _ollamaModel,
                    Messages = messages,
                    Stream = false,
                    Format = "json"
                };

                string raw;
                if (_useLLM == "AI_local")
                    raw = await PostToOllamaChatAsync(_ollamaUrl, requestBody);
                else if (_useLLM == "AI_openrouter")
                    raw = await PostToOpenRouterChatAsync(_ollamaUrl, requestBody);
                else if (_useLLM == "AI_groq")
                    raw = await PostToGroqChatAsync(_groqUrl, requestBody);
                else
                    throw new InvalidOperationException($"LLM non supportato: {_useLLM}");

                // Memorizza la risposta del modello nella history affinché
                // i turni successivi possano contestualizzarla.
                if (!string.IsNullOrEmpty(raw))
                    session.AddToHistory("assistant", raw);

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

        private async Task<string?> Call_LLM_ExtractSingleSlotAsync(string slotName, string intentName, ChatSession session)
        {
            try
            {

                var messages = BuildChatMessages(_intentCatalog.BuildSystemPrompt(intentName), session.History);

                var requestBody = new OllamaChatRequest
                {
                    Model = _ollamaModel,
                    Messages = messages,
                    Stream = false,
                    Format = "json"
                };





                string raw = string.Empty;
                if (_useLLM == "AI_local")
                    raw = await PostToOllamaChatAsync(_ollamaUrl, requestBody);
                else if (_useLLM == "AI_openrouter")
                    raw = await PostToOpenRouterChatAsync(_ollamaUrl, requestBody);
                else if (_useLLM == "AI_groq")
                    raw = await PostToGroqChatAsync(_groqUrl, requestBody);

                else
                    throw new InvalidOperationException($"LLM non supportato: {_useLLM}");

                // Memorizza la risposta del modello nella history affinché
                // i turni successivi possano contestualizzarla.
                if (!string.IsNullOrEmpty(raw))
                    session.AddToHistory("assistant", raw);

                if (string.IsNullOrEmpty(raw)) return null;

                // Il LLM restituisce sempre un ExtractedIntent completo (stesso formato del primo turno).
                // Deserializza e legge il valore dello slot richiesto.
                var extracted = SafeDeserializeExtractedIntent(raw);
                if (extracted?.Slots != null &&
                    extracted.Slots.TryGetValue(slotName, out var slotValue) &&
                    !IsNullString(slotValue))
                    return slotValue;

                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ChatAI] CallOllamaExtractSingleSlotAsync fallita. Session={SessionId} Slot={SlotName}",
                    session.SessionId, slotName);
                return null;
            }
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
        private List<OllamaChatMessage> BuildChatMessages(
            string systemPrompt,
            List<ConversationTurn> history)
        {
            var messages = new List<OllamaChatMessage>
            {
                new() { Role = "system", Content = systemPrompt }
            };

            // Limita la history agli ultimi _maxHistoryTurns turni per evitare
            // payload eccessivi e finestre di contesto superate dal modello.
            var trimmed = history.Count > _maxHistoryTurns
                ? history.Skip(history.Count - _maxHistoryTurns)
                : history;

            foreach (var turn in trimmed)
                messages.Add(new OllamaChatMessage
                {
                    Role = turn.Role,
                    Content = turn.Content
                });

            return messages;
        }

        private async Task<string> PostToOllamaChatAsync(string baseUrl, OllamaChatRequest requestBody)
        {
            var result = await _webApiService.Post<OllamaChatRequest, OllamaChatResponse>(
                serverUrl: baseUrl,
                authentication: null,
                action: _ollamaMethod,
                body: requestBody,
                bodyType: WebApiBodyType.raw,
                headers: new Dictionary<string, string>());

            if (result?.Data == null)
                throw new InvalidOperationException(
                    $"Ollama ha risposto con status {result?.StatusCode}: {result?.ReasonPhrase}");

            return result.Data.Message?.Content ?? string.Empty;
        }

        private async Task<string> PostToOpenRouterChatAsync(string baseUrl, OllamaChatRequest requestBody)
        {



            // Converte formato Ollama in formato OpenAI/OpenRouter
            var openRouterBody = new
            {
                model = _openrouterModel, // es: "microsoft/phi-4"
                messages = requestBody.Messages.Select(m => new
                {
                    role = m.Role,
                    content = m.Content
                }).ToList(),
                stream = false
            };

            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {_openrouterApiKey}",
                ["HTTP-Referer"] = "http://localhost",   // richiesto da OpenRouter
                ["X-Title"] = "NvxApp-Dev"               // nome della app
            };

            var result = await _webApiService.Post<object, OpenRouterChatResponse>(
                serverUrl: _openrouterUrl,
                authentication: null,
                action: _openrouterMethod,
                body: openRouterBody,
                bodyType: WebApiBodyType.raw,
                headers: headers
            );

            if (result?.Data == null)
                throw new InvalidOperationException(
                    $"OpenRouter ha risposto con status {result?.StatusCode}: {result?.ReasonPhrase}");

            return result.Data.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        }


        private async Task<string> PostToGroqChatAsync(string baseUrl, OllamaChatRequest requestBody)
        {
            // Converte formato Ollama in formato OpenAI/Groq
            var groqBody = new
            {
                model = _groqModel, // es: "llama3-8b-8192" oppure "llama3-groq-70b"
                messages = requestBody.Messages.Select(m => new
                {
                    role = m.Role,
                    content = m.Content
                }).ToList(),
                stream = false
            };

            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {_groqApiKey}"
                // Groq NON richiede HTTP-Referer o X-Title
            };

            var result = await _webApiService.Post<object, GroqChatResponse>(
                serverUrl: _groqUrl,          // es: "https://api.groq.com"
                authentication: null,
                action: _groqMethod,          // es: "/openai/v1/chat/completions"
                body: groqBody,
                bodyType: WebApiBodyType.raw,
                headers: headers
            );

            if (result?.Data == null)
                throw new InvalidOperationException(
                    $"Groq ha risposto con status {result?.StatusCode}: {result?.ReasonPhrase}");

            return result.Data.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
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
            catch (Exception ex)
            {
                Log.Warning(ex, "[ChatAI] SafeDeserializeExtractedIntent: JSON non deserializzabile. Raw={Raw}",
                    raw?.Length > 500 ? raw[..500] + "…" : raw);
                return null;
            }
        }

        // ---------------------------------------------------------------------------
        // Merge sicuro degli slot — FIX del bug principale
        // ---------------------------------------------------------------------------

        // Aggiunge alla sessione SOLO i valori realmente presenti.
        // Non tocca MAI gli slot già valorizzati in sessione.
        // userMessage (opzionale): se fornito, scarta i valori che non appaiono nel testo originale
        // come sottostringa — blocca le hallucination di slot inventati quando il messaggio è breve.
        private void SafeMergeSlots(ChatSession session, Dictionary<string, string> newSlots,
            string? userMessage = null)
        {
            if (newSlots == null) return;

            var intentDef = _intentCatalog.Intents
                .FirstOrDefault(i => i.Name == session.Intent);

            foreach (var kv in newSlots)
            {
                //gate 1
                if (IsNullString(kv.Value)) continue; // ignora null/vuoti
                //gate 2
                if (session.Slots.ContainsKey(kv.Key)) continue; // non sovrascrivere esistenti

                var slotDef = intentDef?.Slots.FirstOrDefault(s => s.Name == kv.Key);

                //gate 3
                // Scarta il valore se Ollama ha restituito la PromptDescription verbatim
                if (slotDef != null &&
                    !string.IsNullOrEmpty(slotDef.PromptDescription) &&
                    kv.Value.Equals(slotDef.PromptDescription, StringComparison.OrdinalIgnoreCase))
                    continue;

                //gate 4
                // Scarta il valore se Ollama ha restituito il Type dello slot verbatim
                if (slotDef != null &&
                    !string.IsNullOrEmpty(slotDef.Type) &&
                    kv.Value.Equals(slotDef.Type, StringComparison.OrdinalIgnoreCase))
                    continue;

                //gate 5
                // Scarta il valore se coincide con una keyword dell'intent corrente.
                // Le keyword sono parole trigger (es. "timbratura"), non dati utente.
                if (intentDef != null &&
                    intentDef.Keywords.Any(k => k.Equals(kv.Value, StringComparison.OrdinalIgnoreCase)))
                    continue;

                // Scarta il valore se non è rintracciabile nel testo originale dell'utente.
                // Blocca hallucination dove Ollama inventa nomi, date e orari non digitati.
                // Eccezioni legittime — il valore non deve essere nel testo verbatim se:
                //   1. È un valore di default dello slot opzionale (es. "IN" per direction)
                //   2. Ha un validator che lo approva E il testo contiene materiale grezzo
                //      pertinente (es. almeno una cifra per slot numerici come HH:mm o yyyy-MM-dd).
                //      Questo permette normalizzazioni tipo "5" → "05:00" o "oggi" → "2025-05-20"
                //      senza accettare orari/date inventati quando il testo è solo testo (es. "mimmo zuzzu").

                //gate 6
                if (userMessage != null && slotDef != null)
                {
                    bool valueInText = userMessage.Contains(kv.Value, StringComparison.OrdinalIgnoreCase);
                    bool isDefaultValue = !string.IsNullOrEmpty(slotDef.Default) &&
                                         kv.Value.Equals(slotDef.Default, StringComparison.OrdinalIgnoreCase);

                    // Normalizzazione valida: il validator approva il valore E il testo contiene
                    // almeno una cifra (prerequisito minimo per qualsiasi slot numerico/temporale).
                    //bool isValidNormalization = slotDef.Validator != null && slotDef.Validator(kv.Value) == null;

                    bool validazioneOk = true;
                    if (slotDef.Validator != null)
                    {
                        if (slotDef.Validator(kv.Value) != null)
                            validazioneOk = false;
                    }

                    if (!validazioneOk)
                    {
                        if (slotDef.HasRelevantContent != null)
                        {
                            if (slotDef.HasRelevantContent(userMessage))
                                validazioneOk = true;
                        }
                    }

                    if (!validazioneOk)
                    {
                        Log.Warning("[ChatAI] Slot scartato: valore non presente nel testo. Slot={Slot} Valore={Value} Testo={Text}",
                        kv.Key, kv.Value, userMessage);
                        continue;
                    }

                    //bool isValidContent = slotDef.HasRelevantContent != null &&
                    //                            slotDef.HasRelevantContent(userMessage);



                    //if (!valueInText && !isDefaultValue && (!isValidNormalization /*|| isValidContent*/ ))
                    //{
                    //    Log.Warning("[ChatAI] Slot scartato: valore non presente nel testo. Slot={Slot} Valore={Value} Testo={Text}",
                    //        kv.Key, kv.Value, userMessage);
                    //    continue;
                    //}

                }

                ////Gate 7 
                //// Se lo slot ha un validator di formato, rigetta valori che non lo superano
                //if (slotDef?.Validator != null && slotDef.Validator(kv.Value) != null)
                //    continue;

                session.Slots[kv.Key] = kv.Value;
            }
        }

        private bool IsNullString(string value) =>
            string.IsNullOrWhiteSpace(value) ||
            value.Equals("null", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("unknown", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("n/a", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("<valore estratto>", StringComparison.OrdinalIgnoreCase) ||
            value.Equals("valore estratto", StringComparison.OrdinalIgnoreCase) ||
            // Scarta valori in snake_case (es. "nome_e_cognome_della_persona"):
            // sono sempre placeholder generati dal LLM, mai dati reali dell'utente.
            (value.Contains('_') && !value.Contains(' '));

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
            List<string>? suggestions = null) =>
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

        private ChatAIOutModel BuildErrorResponse(ChatSession session, string message,
            List<string>? suggestions = null) =>
            new()
            {
                SessionId = session.SessionId,
                Responce = message,
                ResponseType = "error",
                Suggestions = suggestions ?? new()
            };

        // Costruisce la lista di suggerimenti dagli intent disponibili nel catalogo.
        // Usa la Description dell'intent come testo del chip.
        private List<string> BuildIntentSuggestions() =>
            _intentCatalog.Intents
                .Select(i => !string.IsNullOrEmpty(i.DisplayName) ? i.DisplayName : i.Name)
                .ToList();

        // ---------------------------------------------------------------------------
        // Session store
        // ---------------------------------------------------------------------------

        // Cerca una sessione esistente valida. Ritorna null se:
        // - sessionId è vuoto (primo messaggio del client)
        // - sessione non trovata (mai esistita)
        // - sessione scaduta (> 10 minuti di inattività) → rimossa
        private ChatSession? TryGetSession(string sessionId) =>
            _sessionStore.TryGet(sessionId);

        private ChatSession CreateSession() =>
            _sessionStore.Create();

        private void DeleteSession(string sessionId) =>
            _sessionStore.Delete(sessionId);

        // ---------------------------------------------------------------------------
        // Lettura API key da file esterno nella root del sito
        // ---------------------------------------------------------------------------

        // I file devono trovarsi nella cartella root del sito (ContentRootPath).
        // Es.: <root>/openrouter.key  e  <root>/groq.key
        // Il file deve contenere solo la chiave API, senza spazi o ritorni a capo.
        private static string ReadApiKeyFromFile(string contentRootPath, string fileName)
        {
            var path = Path.Combine(contentRootPath, fileName);
            if (!File.Exists(path))
                throw new InvalidOperationException(
                    $"File API key non trovato: {path}. " +
                    $"Creare il file '{fileName}' nella root del sito con la chiave API.");

            var key = File.ReadAllText(path).Trim();
            if (string.IsNullOrEmpty(key))
                throw new InvalidOperationException(
                    $"Il file API key '{fileName}' è vuoto.");

            return key;
        }


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


}
