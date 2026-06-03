using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
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
using System.Data;
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

                var applicationUser = await _userManager.FindByIdAsync(this.CurrentUserId);
                if(applicationUser!=null)
                {
                    var roles = await _userManager.GetRolesAsync(applicationUser);
                    if(roles!=null)
                        _intentCatalog.Fuilter4UserRoles(roles.ToList());
                    else
                        _intentCatalog.Fuilter4UserRoles(new List<string> { "*" });
                }
                else
                {
                    _intentCatalog.Fuilter4UserRoles(new List<string> { "*" });
                }

                

                var userMessage = model.Data.Request;
                var sessionId = model.Data.SessionId;

                // 1. Cerca la sessione esistente
                var session = TryGetSession(sessionId);

                // Se il client aveva una sessione ma è scaduta, lo informiamo esplicitamente.
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

                // 2. Comando di reset
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

                // 2b. Comando di help
                if (IsHelpCommand(userMessage))
                {
                    _sessionStore.Delete(session.SessionId);
                    var helpSession = _sessionStore.Create();
                    return new ChatAIOutModel
                    {
                        SessionId = helpSession.SessionId,
                        Responce = string.Empty,
                        ResponseType = "result",
                        Suggestions = BuildIntentSuggestions()
                    };
                }

                // 3. Gestione conferma esplicita ("sì" / "no")
                if (session.State == SessionState.ReadyToExecute)
                    return await HandleConfirmation(session, userMessage);

                // 4. Registra il messaggio utente nella history
                session.AddToHistory("user", userMessage);

                // 5. PRIMO TURNO — piano non ancora costruito
                bool isFirstTurn = session.ActionQueue.Count == 0;

                if (isFirstTurn)
                {
                    // PRE-FILTRO keyword
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

                    var compatibleIntent = GetMatchingIntentNames(userMessage, _intentCatalog.Intents);

                    ExtractedPlan? plan = null;
                    try
                    {
                        plan = await Call_LLM_FirstTurnAsync(session, compatibleIntent);
                    }
                    catch (Exception ex)
                    {
                        DeleteSession(session.SessionId);
                        return BuildErrorResponse(session, ex.Message, BuildIntentSuggestions());
                    }

                    // Chiedi al LLM il piano multi-azione

                    if (plan == null || plan.Actions.Count == 0)
                    {
                        DeleteSession(session.SessionId);
                        return BuildErrorResponse(session, "LLM: non ho capito la richiesta. Puoi ripetere?", BuildIntentSuggestions());
                    }


                    // Valida e normalizza ogni azione del piano
                    foreach (var extractedAction in plan.Actions)
                    {
                        if (extractedAction.Intent.Equals("unknown", StringComparison.OrdinalIgnoreCase)
                            || extractedAction.Confidence < 0.5)
                            continue;

                        var knownIntent = _intentCatalog.Intents
                            .FirstOrDefault(i => i.Name.Equals(extractedAction.Intent, StringComparison.OrdinalIgnoreCase));
                        if (knownIntent == null) continue;

                        var entry = new ActionEntry
                        {
                            Intent = knownIntent.Name,
                            State = ActionState.Collecting
                        };
                        session.ActionQueue.Add(entry);

                        // Merge degli slot estratti per questa azione
                        SafeMergeSlotsForAction(entry, extractedAction.Slots, userMessage);
                    }

                    if (session.ActionQueue.Count == 0)
                    {
                        DeleteSession(session.SessionId);
                        return BuildErrorResponse(session, "Non ho capito la richiesta. Puoi ripetere?", BuildIntentSuggestions());
                    }

                    session.CurrentActionIndex = 0;
                }
                else
                {
                    // TURNI SUCCESSIVI — estrai il valore dello slot mancante per l'azione corrente
                    var currentAction = session.CurrentAction;
                    if (currentAction != null)
                    {
                        string? nextMissing = GetMissingRequiredSlotsForAction(currentAction).FirstOrDefault();



                        if (nextMissing != null)
                        {
                            string? slotValue  = null;

                            try
                            {
                                slotValue = await Call_LLM_OtherTurnAsync(nextMissing, currentAction.Intent, session);
                            }
                            catch (Exception ex)
                            {
                                //DeleteSession(session.SessionId);
                                return BuildErrorResponse(session, ex.Message, BuildIntentSuggestions());
                            }
                            



                            if (string.IsNullOrEmpty(slotValue))
                                slotValue = userMessage.Trim();

                            SafeMergeSlotsForAction(currentAction, new Dictionary<string, string> { [nextMissing] = slotValue }, userMessage);
                        }
                    }
                }

                // 6. Raccolta slot per l'azione corrente
                var current = session.CurrentAction;
                if (current == null)
                {
                    DeleteSession(session.SessionId);
                    return BuildErrorResponse(session, "Errore interno nel piano di esecuzione.", BuildIntentSuggestions());
                }

                // Valida i valori degli slot dell'azione corrente
                var formatValidation = ValidateSlotFormatsForAction(current);
                if (!formatValidation.IsValid)
                {
                    current.Slots.Remove(formatValidation.InvalidSlotName);
                    session.AddToHistory("assistant", formatValidation.MessageToUser);
                    return BuildQuestionResponse(session, formatValidation.MessageToUser, formatValidation.Suggestions);
                }

                // Salva l'InfoMessage sull'azione corrente (es. "Dipendente agganciato: Rossi Mario")
                if (formatValidation.InfoMessage != null)
                    current.InfoMessage = formatValidation.InfoMessage;

                // Controlla se mancano slot obbligatori per l'azione corrente
                var missingSlots = GetMissingRequiredSlotsForAction(current);
                if (missingSlots.Any())
                {
                    var actionTitle = GetDisplayName(current.Intent);
                    var question = BuildMissingSlotQuestion(missingSlots.First(), current.Intent, current, actionTitle);
                    session.AddToHistory("assistant", question);
                    return BuildQuestionResponse(session, question);
                }

                // Azione corrente completamente raccolta — segna come Ready
                current.State = ActionState.Ready;

                // 7. Intent Help — esegui subito senza conferma
                if (current.Intent.Equals("Help", StringComparison.OrdinalIgnoreCase))
                {
                    await ExecuteCommandAsync(current);
                    DeleteSession(session.SessionId);
                    var helpSession = _sessionStore.Create();
                    return new ChatAIOutModel
                    {
                        SessionId = helpSession.SessionId,
                        Responce = string.Empty,
                        ResponseType = "result",
                        Suggestions = BuildIntentSuggestions()
                    };
                }

                // 8. Avanza all'azione successiva se ci sono ancora slot da raccogliere
                //    (azioni con slot ancora da raccogliere nel piano)
                bool hasMoreCollecting = session.ActionQueue
                    .Skip(session.CurrentActionIndex + 1)
                    .Any(a => a.State == ActionState.Collecting &&
                              GetMissingRequiredSlotsForAction(a).Any());

                if (hasMoreCollecting)
                {
                    // Valida/normalizza la prossima azione prima di costruire il prompt
                    var nextAction = session.ActionQueue.ElementAtOrDefault(session.CurrentActionIndex + 1);
                    if (nextAction != null)
                    {
                        var nextValidation = ValidateSlotFormatsForAction(nextAction);
                        if (nextValidation.InfoMessage != null)
                            nextAction.InfoMessage = nextValidation.InfoMessage;
                    }

                    // Avanza all'azione successiva mostrando la domanda con titolo e slot già noti
                    var nextQuestion = BuildNextActionQuestion(session);
                    session.CurrentActionIndex++;
                    session.AddToHistory("assistant", nextQuestion);
                    return BuildQuestionResponse(session, nextQuestion);
                }

                // 9. Tutte le azioni pronte → valida le azioni non ancora validate,
                //    salva l'InfoMessage su ogni ActionEntry e mostra il riepilogo piano completo.

                // Valida le azioni successive (dalla corrente+1 in poi) che non sono ancora Ready.
                foreach (var pendingAction in session.ActionQueue.Skip(session.CurrentActionIndex + 1)
                             .Where(a => a.State == ActionState.Collecting))
                {
                    var pendingValidation = ValidateSlotFormatsForAction(pendingAction);
                    if (!pendingValidation.IsValid)
                    {
                        pendingAction.Slots.Remove(pendingValidation.InvalidSlotName);
                        session.AddToHistory("assistant", pendingValidation.MessageToUser);
                        session.CurrentActionIndex = session.ActionQueue.IndexOf(pendingAction);
                        return BuildQuestionResponse(session, pendingValidation.MessageToUser, pendingValidation.Suggestions);
                    }

                    if (pendingValidation.InfoMessage != null)
                        pendingAction.InfoMessage = pendingValidation.InfoMessage;

                    pendingAction.State = ActionState.Ready;
                }

                session.State = SessionState.ReadyToExecute;
                var summary = BuildPlanConfirmationSummary(session);
                session.AddToHistory("assistant", summary);
                return BuildConfirmationResponse(session, summary);

            }, isSubProcess);
        }

        
        public List<string> GetMatchingIntentNames(string userMessage, IEnumerable<IntentDefinition> intents)
        {
            // Se il messaggio dell'utente è vuoto o non ci sono intenti, ritorna una lista vuota
            if (string.IsNullOrWhiteSpace(userMessage) || intents == null)
            {
                return new List<string>();
            }

            return intents
                .Where(i => i.Keywords != null && i.Keywords.Count > 0)
                .Where(i => i.Keywords.Any(k => userMessage.Contains(k, StringComparison.OrdinalIgnoreCase)))
                .Select(i => i.Name)
                .ToList();
        }

       
        
        
        // ---------------------------------------------------------------------------
        // Gestione conferma utente — esegue l'intero piano
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
                var planResult = await ExecutePlanAsync(session);
                DeleteSession(session.SessionId);
                var doneSession = _sessionStore.Create();

                bool anySuccess = planResult.Any(r => r.Success);
                bool anyFail = planResult.Any(r => !r.Success);
                string responseType = anyFail ? (anySuccess ? "partial_error" : "error") : "result";

                // Messaggio complessivo
                var sb = new StringBuilder();
                foreach (var r in planResult)
                    sb.AppendLine(r.Message);

                // Payload di navigazione — ultima azione navigate del piano
                var navItem = planResult.LastOrDefault(r => r.Navigate != null);

                return new ChatAIOutModel
                {
                    SessionId = doneSession.SessionId,
                    Responce = sb.ToString().TrimEnd(),
                    ResponseType = responseType,
                    Suggestions = anyFail ? new() : BuildIntentSuggestions(),
                    Navigate = navItem?.Navigate,
                    ActionResults = planResult
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

        private static readonly string[] _helpKeywords =
            ["aiuto", "help", "comandi", "menu", "cosa sai fare", "cosa puoi fare"];

        private static bool IsHelpCommand(string text)
        {
            var t = text.Trim().ToLower();
            return _helpKeywords.Any(k => t.Contains(k));
        }

        // ---------------------------------------------------------------------------
        // Chiamata LLM — primo turno: estrae il piano multi-azione
        // ---------------------------------------------------------------------------

        private async Task<ExtractedPlan?> Call_LLM_FirstTurnAsync(ChatSession session,List<string> compatibleIntents)
        {
            try
            {
                //var messages = BuildChatMessages(_intentCatalog.OLD_BuildPlanSystemPrompt(), session.History);
                var messages = BuildChatMessages(_intentCatalog.BuildSystemPromptXS(compatibleIntents), session.History);

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

                if (!string.IsNullOrEmpty(raw))
                    session.AddToHistory("assistant", raw);

                return DeserializePlanFirstTurn(raw);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ChatAI] Call_LLM_ExtractPlanAsync fallita. Session={SessionId}", session.SessionId);
                //return null;
                throw;
            }
        }

        // ---------------------------------------------------------------------------
        // Chiamata Ollama — turni successivi: estrae UN singolo slot dal testo.
        // Passa la ConversationHistory per permettere al modello di disambiguare
        // risposte contestuali (es. "quello di prima", "stessa data").
        // ---------------------------------------------------------------------------

        private async Task<string?> Call_LLM_OtherTurnAsync(string slotName, string intentName, ChatSession session)
        {
            try
            {
                List<string> compatibleIntents = new List<string> { intentName };

                //var messages = BuildChatMessages(_intentCatalog.OLD_BuildSystemPrompt(compatibleIntents), session.History);
                var messages = BuildChatMessages(_intentCatalog.BuildSystemPromptXS(compatibleIntents), session.History);

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
                var extracted = DeserializePlanOtherTurn(raw, slotName, intentName);
                if (extracted?.Slots != null &&
                    extracted.Slots.TryGetValue(slotName, out var slotValue) &&
                    !IsNullString(slotValue))
                    return slotValue;

                return null;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ChatAI] CallOllamaExtractSingleSlotAsync fallita. Session={SessionId} Slot={SlotName}",session.SessionId, slotName);
                //return null;
                throw;
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
        private List<OllamaChatMessage> BuildChatMessages( string? systemPrompt , List<ConversationTurn> history )
        {
            var messages = new List<OllamaChatMessage>();
           

            if(!string.IsNullOrEmpty(systemPrompt))
            {
                messages.Add(new() { Role = "system", Content = systemPrompt });
            }

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

        private ExtractedIntent? DeserializePlanOtherTurn(string raw,string? expectedSlot = null, string? expectedIntent = null)
        {
            try
            {
                if (string.IsNullOrEmpty(raw)) return null;

                var start = raw.IndexOf('{');
                var end = raw.LastIndexOf('}');
                if (start == -1 || end == -1) return null;

                var cleanJson = raw.Substring(start, end - start + 1);

                // Prova prima il formato standard: { "intent": ..., "slots": {...} }
                var direct = JsonSerializer.Deserialize<ExtractedIntent>(cleanJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                if (direct != null && !string.IsNullOrEmpty(direct.Intent))
                    return direct;

                // Fallback: il LLM ha risposto con il formato piano { "actions": [...] }.
                // Accade quando la history contiene risposte del primo turno in formato piano
                // e il modello continua a rispondere nello stesso formato.
                // Cerca l'azione che corrisponde all'intent atteso ed estrae i suoi slot.
                if (expectedSlot != null)
                {
                    var plan = JsonSerializer.Deserialize<ExtractedPlan>(cleanJson,
                        new JsonSerializerOptions { PropertyNameCaseInsensitive = true });

                    if (plan?.Actions != null)
                    {
                        // Cerca l'azione con l'intent atteso, oppure la prima che ha lo slot
                        var match = plan.Actions
                            .Where(a => expectedIntent == null ||
                                        a.Intent.Equals(expectedIntent, StringComparison.OrdinalIgnoreCase))
                            .FirstOrDefault(a => a.Slots.ContainsKey(expectedSlot))
                            ?? plan.Actions.FirstOrDefault(a => a.Slots.ContainsKey(expectedSlot));

                        if (match != null)
                            return new ExtractedIntent
                            {
                                Intent = match.Intent,
                                Slots = match.Slots,
                                Confidence = match.Confidence
                            };
                    }
                }

                return direct;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[ChatAI] DeserializePlanOtherTurn: JSON non deserializzabile. Raw={Raw}",
                    raw?.Length > 500 ? raw[..500] + "…" : raw);
                return null;
            }
        }

        private ExtractedPlan? DeserializePlanFirstTurn(string raw)
        {
            try
            {
                if (string.IsNullOrEmpty(raw)) return null;

                var start = raw.IndexOf('{');
                var end = raw.LastIndexOf('}');
                if (start == -1 || end == -1) return null;

                var cleanJson = raw.Substring(start, end - start + 1);
                return JsonSerializer.Deserialize<ExtractedPlan>(cleanJson,
                    new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[ChatAI] DeserializePlanFirstTurn: JSON non deserializzabile. Raw={Raw}",
                    raw?.Length > 500 ? raw[..500] + "…" : raw);
                return null;
            }
        }

        // ---------------------------------------------------------------------------
        // Merge sicuro degli slot — opera sull'ActionEntry (non sulla sessione)
        // ---------------------------------------------------------------------------

        private void SafeMergeSlotsForAction(ActionEntry action, Dictionary<string, string> newSlots,
            string? userMessage = null)
        {
            if (newSlots == null) return;

            var intentDef = _intentCatalog.Intents
                .FirstOrDefault(i => i.Name == action.Intent);

            foreach (var kv in newSlots)
            {
                if (IsNullString(kv.Value)) continue;
                if (action.Slots.ContainsKey(kv.Key)) continue;

                var slotDef = intentDef?.Slots.FirstOrDefault(s => s.Name == kv.Key);

                if (slotDef != null &&
                    !string.IsNullOrEmpty(slotDef.PromptDescription) &&
                    kv.Value.Equals(slotDef.PromptDescription, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (slotDef != null &&
                    !string.IsNullOrEmpty(slotDef.Type) &&
                    kv.Value.Equals(slotDef.Type, StringComparison.OrdinalIgnoreCase))
                    continue;

                if (intentDef != null &&
                    intentDef.Keywords.Any(k => k.Equals(kv.Value, StringComparison.OrdinalIgnoreCase)))
                    continue;

                if (userMessage != null && slotDef != null)
                {
                    bool valueInText = userMessage.Contains(kv.Value, StringComparison.OrdinalIgnoreCase);
                    bool isDefaultValue = !string.IsNullOrEmpty(slotDef.Default) &&
                                           kv.Value.Equals(slotDef.Default, StringComparison.OrdinalIgnoreCase);
                    bool hasRelevantContent = slotDef.HasRelevantContent != null &&
                                             slotDef.HasRelevantContent(userMessage);

                    if (!valueInText && !isDefaultValue && !hasRelevantContent)
                    {
                        Log.Warning("[ChatAI] Slot scartato (anti-hallucination). Slot={Slot} Valore={Value} Testo={Text}",
                            kv.Key, kv.Value, userMessage);
                        continue;
                    }
                }

                action.Slots[kv.Key] = kv.Value;
            }
        }

        // Mantenuto per compatibilità con Call_LLM_ExtractSingleSlotAsync che riceve ChatSession
        private void SafeMergeSlots(ChatSession session, Dictionary<string, string> newSlots,
            string? userMessage = null)
        {
            var current = session.CurrentAction;
            if (current == null) return;
            SafeMergeSlotsForAction(current, newSlots, userMessage);
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
        // Validazione slot — opera sull'ActionEntry
        // ---------------------------------------------------------------------------

        private SlotValidationResult ValidateSlotFormatsForAction(ActionEntry action)
        {
            var intentDef = _intentCatalog.Intents
                .FirstOrDefault(i => i.Name == action.Intent);

            if (intentDef == null) return SlotValidationResult.Ok();

            string? pendingInfo = null;

            foreach (var slotDef in intentDef.Slots)
            {
                if (slotDef.Validator == null) continue;
                if (!action.Slots.TryGetValue(slotDef.Name, out var value)) continue;
                var result = slotDef.Validator(value);
                if (result == null) continue;
                if (result.IsValid)
                {
                    if (result.NormalizedValue != null)
                        action.Slots[slotDef.Name] = result.NormalizedValue;
                    if (result.InfoMessage != null)
                        pendingInfo = result.InfoMessage;
                    continue;
                }
                return result;
            }

            if (intentDef.CrossValidator != null)
            {
                var crossResult = intentDef.CrossValidator(action.Slots);
                if (crossResult != null) return crossResult;
            }

            return SlotValidationResult.Ok(action.Slots.GetValueOrDefault("employeeName", string.Empty), pendingInfo);
        }

        // Mantenuto per compatibilità interna con il vecchio flusso (usato da ValidateSlotFormats)
        private SlotValidationResult ValidateSlotFormats(ChatSession session)
        {
            var current = session.CurrentAction;
            return current != null ? ValidateSlotFormatsForAction(current) : SlotValidationResult.Ok();
        }

        // ---------------------------------------------------------------------------
        // Slot mancanti
        // ---------------------------------------------------------------------------

        private List<string> GetMissingRequiredSlotsForAction(ActionEntry action)
        {
            var intentDef = _intentCatalog.Intents
                .FirstOrDefault(i => i.Name == action.Intent);

            if (intentDef == null) return new();

            return intentDef.Slots
                .Where(s => s.Required && !action.Slots.ContainsKey(s.Name))
                .Select(s => s.Name)
                .ToList();
        }

        // Shim per compatibilità con Call_LLM_ExtractSingleSlotAsync
        private List<string> GetMissingRequiredSlots(ChatSession session)
        {
            var current = session.CurrentAction;
            return current != null ? GetMissingRequiredSlotsForAction(current) : new();
        }

        private string BuildMissingSlotQuestion(string slotName, string intentName,
            ActionEntry? action = null, string? actionTitle = null)
        {
            var slotDef = FindSlotDefinition(intentName, slotName);
            var question = !string.IsNullOrEmpty(slotDef?.Question)
                ? slotDef.Question
                : $"Puoi specificare '{slotName}'?";

            // Costruisce il contesto: titolo operazione (opzionale) + slot già raccolti
            var contextParts = new List<string>();

            if (!string.IsNullOrEmpty(actionTitle))
                contextParts.Add($"Operazione '{actionTitle}'");

            if (action != null && action.Slots.Count > 0)
            {
                var intentDef = _intentCatalog.Intents.FirstOrDefault(i => i.Name == intentName);
                if (intentDef != null)
                {
                    foreach (var sd in intentDef.Slots)
                    {
                        if (sd.Name == slotName) continue;
                        if (!action.Slots.TryGetValue(sd.Name, out var val)) continue;
                        var label = !string.IsNullOrEmpty(sd.Label) ? sd.Label : sd.Name;
                        contextParts.Add($"{label}: {val}");
                    }
                }
            }

            if (contextParts.Count > 0)
                return string.Join("\n", contextParts) + "\n" + question;

            return question;
        }

        // ---------------------------------------------------------------------------
        // Esecuzione singola azione
        // ---------------------------------------------------------------------------

        private async Task<CommandResult> ExecuteCommandAsync(ActionEntry action)
        {
            try
            {
                var handler = _commandRegistry.Resolve(action.Intent);
                return await handler.ExecuteAsync(action.Slots);
            }
            catch (InvalidOperationException ex)
            {
                Log.Warning(ex, "[ChatAI] Handler non trovato. Intent={Intent}", action.Intent);
                return CommandResult.Fail(ex.Message);
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ChatAI] ExecuteCommandAsync fallita. Intent={Intent} Slots={Slots}",
                    action.Intent, System.Text.Json.JsonSerializer.Serialize(action.Slots));
                return CommandResult.Fail("Errore durante l'esecuzione del comando. Riprova.");
            }
        }

        // Shim per compatibilità con il blocco Help in SendMessage
        private async Task<CommandResult> ExecuteCommandAsync(ChatSession session)
        {
            var current = session.CurrentAction;
            if (current == null) return CommandResult.Fail("Nessuna azione corrente.");
            return await ExecuteCommandAsync(current);
        }

        // ---------------------------------------------------------------------------
        // Esecuzione piano completo
        // ---------------------------------------------------------------------------

        private async Task<List<ActionResultItem>> ExecutePlanAsync(ChatSession session)
        {
            var results = new List<ActionResultItem>();

            foreach (var action in session.ActionQueue)
            {
                var intentDef = _intentCatalog.Intents
                    .FirstOrDefault(i => i.Name.Equals(action.Intent, StringComparison.OrdinalIgnoreCase));

                // Esegui validazione/normalizzazione degli slot per ogni azione del piano.
                // Le azioni successive alla prima non passano per il flusso SendMessage,
                // quindi il validator (es. risoluzione nome canonico dipendente) va rieseguito qui.
                var validation = ValidateSlotFormatsForAction(action);
                if (!validation.IsValid)
                {
                    action.State = ActionState.Failed;
                    var failItem = new ActionResultItem
                    {
                        Intent = action.Intent,
                        Success = false,
                        Message = $"[{GetDisplayName(action.Intent)}] {validation.MessageToUser}"
                    };
                    results.Add(failItem);

                    if ((intentDef?.ExecutionStrategy ?? ExecutionStrategy.StopOnError)
                        == ExecutionStrategy.StopOnError)
                        break;

                    continue;
                }

                var cmdResult = await ExecuteCommandAsync(action);
                action.State = cmdResult.Success ? ActionState.Done : ActionState.Failed;

                var item = new ActionResultItem
                {
                    Intent = action.Intent,
                    Success = cmdResult.Success,
                    Message = cmdResult.Message
                };

                // Se l'intent è di navigazione, il CommandResult.Data contiene il NavigatePayload
                if (intentDef?.IsNavigation == true && cmdResult.Data is NavigatePayload nav)
                    item.Navigate = nav;

                results.Add(item);

                // Strategia StopOnError: interrompe al primo errore
                if (!cmdResult.Success && (intentDef?.ExecutionStrategy ?? ExecutionStrategy.StopOnError)
                    == ExecutionStrategy.StopOnError)
                    break;
            }

            return results;
        }

        // ---------------------------------------------------------------------------
        // Riepilogo piano completo
        // ---------------------------------------------------------------------------

        private string BuildPlanConfirmationSummary(ChatSession session)
        {
            var sb = new StringBuilder();
            sb.AppendLine("Riepilogo operazioni:");
            sb.AppendLine();

            int idx = 1;
            foreach (var action in session.ActionQueue)
            {
                var intentDef = _intentCatalog.Intents.FirstOrDefault(i => i.Name == action.Intent);
                var label = !string.IsNullOrEmpty(intentDef?.DisplayName) ? intentDef.DisplayName : action.Intent;

                // Titolo numerato + DisplayName
                sb.AppendLine($"{"①②③④⑤⑥⑦⑧⑨⑩"[Math.Min(idx - 1, 9)]} {label}");

                // Descrizione dell'intent
                //if (!string.IsNullOrEmpty(intentDef?.Description))
                //    sb.AppendLine($"   {intentDef.Description}");

                // InfoMessage inline (es. "Dipendente agganciato: Rossi Mario")
                if (!string.IsNullOrEmpty(action.InfoMessage))
                    sb.AppendLine($"   ℹ️ {action.InfoMessage}");

                // Tutti gli slot (obbligatori e opzionali) nell'ordine di definizione
                if (intentDef != null)
                {
                    foreach (var slotDef in intentDef.Slots)
                    {
                        var value = action.Slots.GetValueOrDefault(slotDef.Name,
                            !string.IsNullOrEmpty(slotDef.Default) ? slotDef.Default : "-");
                        var slotLabel = !string.IsNullOrEmpty(slotDef.Label) ? slotDef.Label : slotDef.Name;
                        sb.AppendLine($"   {slotLabel}: {value}");
                    }
                }

                sb.AppendLine();
                idx++;
            }

            sb.AppendLine("Confermi? (sì / no)");
            return sb.ToString();
        }

        private string GetDisplayName(string intentName)
        {
            var intentDef = _intentCatalog.Intents.FirstOrDefault(i => i.Name == intentName);
            return !string.IsNullOrEmpty(intentDef?.DisplayName) ? intentDef.DisplayName : intentName;
        }

        private string BuildNextActionQuestion(ChatSession session)
        {
            var next = session.ActionQueue.ElementAtOrDefault(session.CurrentActionIndex + 1);
            if (next == null) return string.Empty;

            var missing = GetMissingRequiredSlotsForAction(next).FirstOrDefault();
            if (missing == null) return string.Empty;

            var actionTitle = GetDisplayName(next.Intent);
            return BuildMissingSlotQuestion(missing, next.Intent, next, actionTitle);
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
