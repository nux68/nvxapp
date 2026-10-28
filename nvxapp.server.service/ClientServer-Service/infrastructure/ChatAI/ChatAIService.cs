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
using System.Text.RegularExpressions;


namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Dialogo a turni:
    //   ogni messaggio (che non sia reset, help, conferma o annullamento) va al LLM insieme al catalogo
    //   degli intent e allo stato della conversazione; il LLM restituisce il piano completo aggiornato
    //   (più valori in una risposta, correzioni, cambio di richiesta, riferimenti alle ultime azioni).
    //   Il server accetta solo i valori giustificati dal testo dell'utente (anti-allucinazione),
    //   li normalizza (SlotNormalizers), li valida, chiede quelli mancanti e infine conferma ed esegue.
    public class ChatAIService : ServiceBase, IChatAIService
    {
        private readonly iRabbitMqConnection _rabbitMqConnection;
        private readonly IWebApiService _webApiService;
        private readonly ICommandRegistry _commandRegistry;
        private readonly IIntentCatalog _intentCatalog;
        private readonly IChatSessionStore _sessionStore;
        private readonly IChatUserContext _chatUserContext;

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

        private readonly string _deepseekUrl;
        private readonly string _deepseekModel;
        private readonly string _deepseekMethod;
        private readonly string _deepseekApiKey;

        private readonly int _maxHistoryTurns;
        private readonly string _useLLM;

        // Output strutturato del provider in uso (AI:<provider>:ResponseFormat):
        // "json_schema" (schema con i nomi degli intent), "json_object" (solo JSON valido) o "none".
        private readonly string _responseFormat;
        // AI:<provider>:StrictSchema: schema rispettato in modo rigoroso (solo modelli che lo supportano)
        private readonly bool _strictSchema;
        // AI:<provider>:ReasoningEffort: ragionamento dei modelli che lo prevedono ("low", "medium", "high";
        // "none" = non inviato). Default "low" per gpt-oss: meno token (limite al minuto) e risposte più rapide.
        private readonly string? _reasoningEffort;

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
                             IChatSessionStore sessionStore,
                             IChatUserContext chatUserContext
                             ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _rabbitMqConnection = rabbitMqConnection;
            _webApiService = webApiService;
            _commandRegistry = commandRegistry;
            _intentCatalog = intentCatalog;
            _sessionStore = sessionStore;
            _chatUserContext = chatUserContext;

            _maxHistoryTurns = int.TryParse(configuration["AI:MaxHistoryTurns"], out var n) && n > 0 ? n : 20;

            _useLLM = configuration["AI:UseLLM"] ?? throw new InvalidOperationException("UseLLM non configurato");

            _ollamaUrl = configuration["AI:AI_local:Url"] ?? throw new InvalidOperationException("AI_local:Url non configurato");
            _ollamaModel = configuration["AI:AI_local:model"] ?? throw new InvalidOperationException("AI_local:model non configurato");
            _ollamaMethod = configuration["AI:AI_local:method"] ?? throw new InvalidOperationException("AI_local:method non configurato");

            _openrouterUrl = configuration["AI:AI_openrouter:Url"] ?? throw new InvalidOperationException("AI_openrouter:Url non configurato");
            _openrouterModel = configuration["AI:AI_openrouter:model"] ?? throw new InvalidOperationException("AI_openrouter:model non configurato");
            _openrouterMethod = configuration["AI:AI_openrouter:method"] ?? throw new InvalidOperationException("AI_openrouter:method non configurato");
            _openrouterApiKey = ApiKey(configuration, "AI_openrouter");

            _groqUrl = configuration["AI:AI_groq:Url"] ?? throw new InvalidOperationException("AI_groq:Url non configurato");
            _groqModel = configuration["AI:AI_groq:model"] ?? throw new InvalidOperationException("AI_groq:model non configurato");
            _groqMethod = configuration["AI:AI_groq:method"] ?? throw new InvalidOperationException("AI_groq:method non configurato");
            _groqApiKey = ApiKey(configuration, "AI_groq");

            _deepseekUrl = configuration["AI:AI_deepseek:Url"] ?? throw new InvalidOperationException("AI_deepseek:Url non configurato");
            _deepseekModel = configuration["AI:AI_deepseek:model"] ?? throw new InvalidOperationException("AI_deepseek:model non configurato");
            _deepseekMethod = configuration["AI:AI_deepseek:method"] ?? throw new InvalidOperationException("AI_deepseek:method non configurato");
            _deepseekApiKey = ApiKey(configuration, "AI_deepseek");

            // OpenRouter instrada su modelli diversi, non tutti con lo schema: di default solo JSON valido.
            // DeepSeek non supporta json_schema, solo json_object.
            _responseFormat = configuration[$"AI:{_useLLM}:ResponseFormat"]
                              ?? (_useLLM is "AI_openrouter" or "AI_deepseek" ? "json_object" : "json_schema");
            _strictSchema = bool.TryParse(configuration[$"AI:{_useLLM}:StrictSchema"], out var strict) && strict;

            var model = _useLLM switch { "AI_groq" => _groqModel, "AI_openrouter" => _openrouterModel, "AI_deepseek" => _deepseekModel, _ => _ollamaModel };
            _reasoningEffort = configuration[$"AI:{_useLLM}:ReasoningEffort"]
                               ?? (model.Contains("gpt-oss", StringComparison.OrdinalIgnoreCase) ? "low" : null);
            if (_reasoningEffort == "none") _reasoningEffort = null;
        }

        // ---------------------------------------------------------------------------
        // Entry point principale
        // ---------------------------------------------------------------------------

        public virtual async Task<GenericResult<ChatAIOutModel>> SendMessage(
            GenericRequest<ChatAIInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {

                var userRoles = new List<string>();
                var applicationUser = await _userManager.FindByIdAsync(this.CurrentUserId);
                if(applicationUser!=null)
                {
                    var roles = await _userManager.GetRolesAsync(applicationUser);
                    if(roles!=null)
                    {
                        userRoles = roles.ToList();
                        _intentCatalog.Fuilter4UserRoles(userRoles);
                    }
                    else
                        _intentCatalog.Fuilter4UserRoles(new List<string> { "*" });
                }
                else
                {
                    _intentCatalog.Fuilter4UserRoles(new List<string> { "*" });
                }

                // utente corrente per gli handler: "@me", limiti per ruolo, help contestuale
                _chatUserContext.UserId = CurrentUserId;
                _chatUserContext.Roles = userRoles;
                _chatUserContext.CurrentRoute = model.Data.CurrentRoute ?? string.Empty;

                return await ProcessMessageAsync(model.Data);

            }, isSubProcess);
        }

        // Un turno di dialogo: comandi di controllo, conferma, chiamata al LLM, avanzamento del piano.
        // internal: lo usa anche il programma di prova delle frasi (tools/ChatAiPhraseTest, InternalsVisibleTo).
        internal async Task<ChatAIOutModel> ProcessMessageAsync(ChatAIInModel input)
        {
            var userMessage = (input.Request ?? string.Empty).Trim();

            // 1. Cerca la sessione esistente (solo se appartiene all'utente corrente).
            // Senza sessionId (prima richiesta), o con una sessione scaduta o di un altro utente,
            // il messaggio si elabora in una sessione nuova: non va perso.
            var session = TryGetSession(input.SessionId) ?? CreateSession();
            session.CurrentRoute = input.CurrentRoute ?? string.Empty;

            // 2. Comando di reset
            if (IsResetCommand(userMessage))
            {
                _sessionStore.Delete(session.SessionId);
                var resetSession = CreateSession();
                return new ChatAIOutModel
                {
                    SessionId = resetSession.SessionId,
                    Responce = ChatAIMessages.Get("ResetDone"),
                    ResponseType = "result",
                    Suggestions = BuildIntentSuggestions()
                };
            }

            // 2b. Comando di help
            if (IsHelpCommand(userMessage))
            {
                // elenco completo dei comandi del ruolo (prima quelli della pagina), come l'intent Help:
                // i chip sono solo alcune scorciatoie (MaxIntentSuggestions)
                var help = await ExecuteCommandAsync(new ActionEntry { Intent = "Help" });
                _sessionStore.Delete(session.SessionId);
                var helpSession = CreateSession();
                return new ChatAIOutModel
                {
                    SessionId = helpSession.SessionId,
                    Responce = help.Success ? help.Message : string.Empty,
                    SpeechText = help.Success ? help.Message : string.Empty,
                    ResponseType = "result",
                    Suggestions = BuildIntentSuggestions()
                };
            }

            // 3. Piano in attesa di conferma: "sì" esegue, "no" annulla, il resto è una modifica
            //    del piano ("cambia l'ora in 10") e passa dal LLM come ogni altro messaggio.
            bool wasReady = session.State == SessionState.ReadyToExecute;
            if (wasReady)
            {
                // parole accettate in italiano e in inglese, qualunque sia la lingua dell'interfaccia;
                // tolleranti alle parole di contorno ("sì grazie", "no lascia stare")
                if (ChatAIKeywords.IsConfirm(userMessage))
                {
                    session.AddToHistory("user", userMessage);
                    return await ExecutePlanAndFinishAsync(session);
                }

                if (ChatAIKeywords.IsCancel(userMessage))
                {
                    session.AddToHistory("user", userMessage);
                    return CancelPlan(session);
                }
            }

            // 4. Turno del LLM: piano completo aggiornato con il nuovo messaggio
            session.AddToHistory("user", userMessage);
            var previousPlan = ClonePlan(session.ActionQueue);

            ExtractedPlan? plan;
            try
            {
                plan = await Call_LLM_TurnAsync(session);
            }
            catch (Exception ex)
            {
                // Il dialogo in corso resta valido: si ripete la richiesta invece di chiuderlo.
                // Il dettaglio dell'errore è già nel log (Call_LLM_TurnAsync).
                if (wasReady)
                    return BuildClarificationResponse(session);
                if (session.ActionQueue.Count > 0)
                    return BuildCurrentQuestionResponse(session, ChatAIMessages.Get("AnswerNotProcessed"));
                return BuildErrorResponse(session, ex.Message, BuildIntentSuggestions());
            }

            var newPlan = BuildPlanFromLlm(plan, session);

            if (newPlan.Count == 0)
            {
                // In attesa di conferma un messaggio non interpretabile non cambia il piano
                if (wasReady)
                    return BuildClarificationResponse(session);

                // Piano in corso: il testo intero è la risposta alla domanda posta (es. "Rossi")
                if (!TryAnswerPendingSlot(session, userMessage))
                {
                    LogNotUnderstood(session, userMessage);

                    if (session.ActionQueue.Count > 0)
                        return BuildCurrentQuestionResponse(session, ChatAIMessages.Get("AnswerNotProcessed"));

                    var notUnderstood = ChatAIMessages.Get("NotUnderstood");
                    session.AddToHistory("assistant", notUnderstood);
                    return BuildErrorResponse(session, notUnderstood, BuildIntentSuggestions());
                }
            }
            else
            {
                // Modifica richiesta sul riepilogo ma il piano non è cambiato: si chiede di nuovo sì/no
                if (wasReady && SamePlan(previousPlan, newPlan))
                    return BuildClarificationResponse(session);

                session.ActionQueue = newPlan;
            }

            session.State = SessionState.Collecting;
            return await AdvancePlanAsync(session);
        }

        // ---------------------------------------------------------------------------
        // Avanzamento del piano: validazione, domande per i valori mancanti, conferma o esecuzione
        // ---------------------------------------------------------------------------

        private async Task<ChatAIOutModel> AdvancePlanAsync(ChatSession session)
        {
            for (int idx = 0; idx < session.ActionQueue.Count; idx++)
            {
                var action = session.ActionQueue[idx];
                session.CurrentActionIndex = idx;

                // Valida e normalizza i valori dell'azione (es. nome canonico del dipendente)
                var validation = ValidateSlotFormatsForAction(action);
                if (!validation.IsValid)
                {
                    action.Slots.Remove(validation.InvalidSlotName);
                    session.AddToHistory("assistant", validation.MessageToUser);
                    return BuildQuestionResponse(session, validation.MessageToUser, validation.Suggestions);
                }

                // Messaggio informativo dell'azione (es. "Dipendente agganciato: Rossi Mario")
                if (validation.InfoMessage != null)
                    action.InfoMessage = validation.InfoMessage;

                // Valori obbligatori mancanti: si chiede il primo, con il contesto di quelli noti
                var missing = GetMissingRequiredSlotsForAction(action).FirstOrDefault();
                if (missing != null)
                {
                    var question = BuildMissingSlotQuestion(missing, action.Intent, action, GetDisplayName(action.Intent));
                    session.AddToHistory("assistant", question);
                    return BuildQuestionResponse(session, question, speechText: GetSlotQuestion(action.Intent, missing));
                }

                action.State = ActionState.Ready;
            }

            // Tutte le azioni sono complete: riepilogo e conferma se almeno una lo richiede
            // (es. scritture di dati), altrimenti esecuzione immediata (es. navigazione)
            if (session.ActionQueue.Any(a => FindIntent(a.Intent)?.NeedsConfirmation ?? true))
            {
                session.State = SessionState.ReadyToExecute;
                var summary = BuildPlanConfirmationSummary(session);
                session.AddToHistory("assistant", summary);
                return BuildConfirmationResponse(session, summary);
            }

            return await ExecutePlanAndFinishAsync(session);
        }

        // Il LLM non ha estratto nulla ma c'è una domanda in sospeso: il testo intero è la risposta
        // al primo valore mancante (es. "Rossi" alla domanda "Quale dipendente?").
        private bool TryAnswerPendingSlot(ChatSession session, string userMessage)
        {
            var action = session.ActionQueue.FirstOrDefault(a => GetMissingRequiredSlotsForAction(a).Any());
            if (action == null || string.IsNullOrWhiteSpace(userMessage)) return false;

            var slotName = GetMissingRequiredSlotsForAction(action).First();
            var slotDef = FindSlotDefinition(action.Intent, slotName);
            var normalizer = slotDef?.Normalizer ?? SlotNormalizers.ForType(slotDef?.Type);

            action.Slots[slotName] = normalizer?.Invoke(userMessage) ?? userMessage;
            return true;
        }

        // ---------------------------------------------------------------------------
        // Esecuzione del piano e chiusura del turno
        // ---------------------------------------------------------------------------

        private async Task<ChatAIOutModel> ExecutePlanAndFinishAsync(ChatSession session)
        {
            session.State = SessionState.Confirmed;
            var planResult = await ExecutePlanAsync(session);

            // Le azioni eseguite restano come contesto ("anche per Bianchi"); la sessione continua
            var done = session.ActionQueue.Where(a => a.State == ActionState.Done).ToList();
            if (done.Count > 0)
                session.LastExecuted = ClonePlan(done);
            session.ActionQueue = new();
            session.CurrentActionIndex = 0;
            session.State = SessionState.Collecting;

            bool anySuccess = planResult.Any(r => r.Success);
            bool anyFail = planResult.Any(r => !r.Success);
            string responseType = anyFail ? (anySuccess ? "partial_error" : "error") : "result";

            // Messaggio complessivo
            var sb = new StringBuilder();
            foreach (var r in planResult)
                sb.AppendLine(r.Message);
            var message = sb.ToString().TrimEnd();
            session.AddToHistory("assistant", message);

            // Payload di navigazione — ultima azione navigate del piano
            var navItem = planResult.LastOrDefault(r => r.Navigate != null);
            var clientItem = planResult.LastOrDefault(r => r.ClientAction != null);

            return new ChatAIOutModel
            {
                SessionId = session.SessionId,
                Responce = message,
                SpeechText = message,
                ResponseType = responseType,
                Suggestions = anyFail ? new() : BuildIntentSuggestions(),
                Navigate = navItem?.Navigate,
                ClientAction = clientItem?.ClientAction,
                ActionResults = planResult
            };
        }

        private ChatAIOutModel CancelPlan(ChatSession session)
        {
            session.ActionQueue = new();
            session.CurrentActionIndex = 0;
            session.State = SessionState.Collecting;

            var message = ChatAIMessages.Get("OperationCancelled");
            session.AddToHistory("assistant", message);
            return new ChatAIOutModel
            {
                SessionId = session.SessionId,
                Responce = message,
                SpeechText = message,
                ResponseType = "result",
                Suggestions = BuildIntentSuggestions()
            };
        }

        // ---------------------------------------------------------------------------
        // Reset — riconosce comandi di interruzione esplicita
        // ---------------------------------------------------------------------------

        // Frasi (italiano + inglese) in ChatAIKeywords.Reset / ChatAIKeywords.Help, riconosciute
        // solo a frase intera: "cancella la timbratura di Rossi" è un comando, non un reset.
        private static bool IsResetCommand(string text) =>
            ChatAIKeywords.IsExactMatch(text, ChatAIKeywords.Reset);

        private static bool IsHelpCommand(string text) =>
            ChatAIKeywords.IsExactMatch(text, ChatAIKeywords.Help);

        // ---------------------------------------------------------------------------
        // Chiamata LLM — un solo tipo di turno: catalogo + stato -> piano completo aggiornato
        // ---------------------------------------------------------------------------

        private async Task<ExtractedPlan?> Call_LLM_TurnAsync(ChatSession session)
        {
            try
            {
                // Dettagli (slot) solo per gli intent pertinenti: contiene i token del prompt
                var detailed = RelevantIntents(session);
                var plan = await Call_LLM_OnceAsync(session, detailed);

                // Il LLM ha scelto intent con slot di cui non aveva i dettagli: si ripete il turno
                // una volta con i loro dettagli, così i valori del messaggio vengono estratti subito.
                var missingDetails = plan?.Actions
                    .Select(a => FindIntent(a.Intent))
                    .Where(i => i != null && i.Slots.Count > 0 && !detailed.Contains(i.Name))
                    .Select(i => i!.Name)
                    .ToList() ?? new();

                if (missingDetails.Count > 0)
                {
                    detailed.UnionWith(missingDetails);
                    plan = await Call_LLM_OnceAsync(session, detailed);
                }

                return plan;
            }
            catch (Exception ex)
            {
                Log.Error(ex, "[ChatAI] Call_LLM_TurnAsync fallita. Session={SessionId}", session.SessionId);
                throw;
            }
        }

        private async Task<ExtractedPlan?> Call_LLM_OnceAsync(ChatSession session, ICollection<string> detailedIntents)
        {
            var systemPrompt = _intentCatalog.BuildTurnSystemPrompt(session, detailedIntents);
            var raw = await PostToLlmAsync(BuildChatMessages(systemPrompt, session.History));
            Log.Debug("[ChatAI] Risposta LLM. Session={SessionId} Raw={Raw}", session.SessionId, raw);

            // Risposta vuota: capita (es. DeepSeek in modalità JSON) dopo vari scambi con risposte testuali
            // dell'assistente nella cronologia. Si ripete una volta con la sola ultima frase dell'utente:
            // lo stato della conversazione (piano in corso, ultime azioni) è comunque nel prompt di sistema.
            if (string.IsNullOrWhiteSpace(raw))
            {
                var lastUser = session.History.LastOrDefault(t => t.Role == "user");
                var shortHistory = lastUser != null ? new List<ConversationTurn> { lastUser } : new List<ConversationTurn>();
                raw = await PostToLlmAsync(BuildChatMessages(systemPrompt, shortHistory));
                Log.Debug("[ChatAI] Risposta LLM (nuovo tentativo, senza cronologia). Session={SessionId} Raw={Raw}", session.SessionId, raw);
            }

            return ParsePlan(raw);
        }

        // Intent di cui mandare i dettagli al LLM: quelli del piano in corso, delle ultime azioni
        // e quelli con una parola chiave nell'ultimo messaggio dell'utente. Le parole chiave di una
        // parola valgono anche come inizio di parola ("timbra" -> "timbrami"): così i dettagli arrivano
        // già alla prima chiamata, senza la seconda (con molti intent conta il limite di token al minuto).
        private HashSet<string> RelevantIntents(ChatSession session)
        {
            var relevant = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
            foreach (var action in session.ActionQueue.Concat(session.LastExecuted))
                relevant.Add(action.Intent);

            var lastMessage = session.History.LastOrDefault(t => t.Role == "user")?.Content;
            var normalized = ChatAIKeywords.Normalize(lastMessage);
            var text = $" {normalized} ";
            var words = normalized.Split(' ', StringSplitOptions.RemoveEmptyEntries);

            bool Matches(string keyword)
            {
                var nk = ChatAIKeywords.Normalize(keyword);
                if (nk.Length == 0) return false;
                if (nk.Contains(' ')) return text.Contains($" {nk} ");
                return words.Any(w => w == nk || (nk.Length >= 4 && w.StartsWith(nk, StringComparison.Ordinal)));
            }

            foreach (var intent in _intentCatalog.Intents)
                if (intent.Keywords.Any(Matches))
                    relevant.Add(intent.Name);

            return relevant;
        }

        // Cerca la SlotDefinition per nome nell'intent indicato.
        // Unico punto di accesso alla struttura dello slot — nessun switch sui nomi.
        private SlotDefinition? FindSlotDefinition(string intentName, string slotName) =>
            FindIntent(intentName)?.Slots.FirstOrDefault(s => s.Name == slotName);

        private IntentDefinition? FindIntent(string intentName) =>
            _intentCatalog.Intents.FirstOrDefault(i => i.Name.Equals(intentName, StringComparison.OrdinalIgnoreCase));

        // ---------------------------------------------------------------------------
        // HTTP helper — /api/chat (supporta messages array con history)
        // ---------------------------------------------------------------------------

        // Costruisce la lista messages: [system] + [history].
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
            foreach (var turn in TrimmedHistory(history))
                messages.Add(new OllamaChatMessage
                {
                    Role = turn.Role,
                    Content = turn.Content
                });

            return messages;
        }

        private IEnumerable<ConversationTurn> TrimmedHistory(List<ConversationTurn> history) =>
            history.Count > _maxHistoryTurns
                ? history.Skip(history.Count - _maxHistoryTurns)
                : history;

        // Invia la richiesta al provider configurato (AI:UseLLM) con l'output strutturato richiesto
        private async Task<string> PostToLlmAsync(List<OllamaChatMessage> messages)
        {
            var schema = _intentCatalog.BuildResponseSchema();

            return _useLLM switch
            {
                "AI_local" => await PostToOllamaChatAsync(_ollamaUrl, new OllamaChatRequest
                {
                    Model = _ollamaModel,
                    Messages = messages,
                    Stream = false,
                    Format = _responseFormat == "json_schema" ? schema : "json"
                }),
                "AI_openrouter" => await PostToOpenRouterChatAsync(messages, BuildOpenAiResponseFormat(schema)),
                "AI_groq" => await PostToGroqChatAsync(messages, BuildOpenAiResponseFormat(schema)),
                "AI_deepseek" => await PostToDeepSeekChatAsync(messages, BuildOpenAiResponseFormat(schema)),
                _ => throw new InvalidOperationException($"LLM non supportato: {_useLLM}")
            };
        }

        // response_format delle API compatibili OpenAI (Groq, OpenRouter, DeepSeek)
        private object? BuildOpenAiResponseFormat(object schema) => _responseFormat switch
        {
            "json_schema" => new Dictionary<string, object>
            {
                ["type"] = "json_schema",
                ["json_schema"] = new Dictionary<string, object>
                {
                    ["name"] = "action_plan",
                    ["strict"] = _strictSchema,
                    ["schema"] = schema
                }
            },
            "json_object" => new Dictionary<string, object> { ["type"] = "json_object" },
            _ => null
        };

        // Corpo della richiesta per le API compatibili OpenAI
        private Dictionary<string, object> BuildOpenAiBody(string model, List<OllamaChatMessage> messages, object? responseFormat)
        {
            var body = new Dictionary<string, object>
            {
                ["model"] = model,
                ["messages"] = messages.Select(m => new { role = m.Role, content = m.Content }).ToList(),
                ["stream"] = false,
                ["temperature"] = 0.2
            };
            if (responseFormat != null)
                body["response_format"] = responseFormat;

            // livello di ragionamento: Groq lo vuole come reasoning_effort, OpenRouter come reasoning.effort
            if (_reasoningEffort != null)
            {
                if (_useLLM == "AI_openrouter")
                    body["reasoning"] = new Dictionary<string, object> { ["effort"] = _reasoningEffort };
                else
                    body["reasoning_effort"] = _reasoningEffort;
            }
            return body;
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

        private async Task<string> PostToOpenRouterChatAsync(List<OllamaChatMessage> messages, object? responseFormat)
        {
            var openRouterBody = BuildOpenAiBody(_openrouterModel, messages, responseFormat);

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


        private async Task<string> PostToGroqChatAsync(List<OllamaChatMessage> messages, object? responseFormat)
        {
            var groqBody = BuildOpenAiBody(_groqModel, messages, responseFormat);

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


        private async Task<string> PostToDeepSeekChatAsync(List<OllamaChatMessage> messages, object? responseFormat)
        {
            var deepseekBody = BuildOpenAiBody(_deepseekModel, messages, responseFormat);

            var headers = new Dictionary<string, string>
            {
                ["Authorization"] = $"Bearer {_deepseekApiKey}"
            };

            var result = await _webApiService.Post<object, DeepSeekChatResponse>(
                serverUrl: _deepseekUrl,          // es: "https://api.deepseek.com"
                authentication: null,
                action: _deepseekMethod,          // es: "/chat/completions"
                body: deepseekBody,
                bodyType: WebApiBodyType.raw,
                headers: headers
            );

            if (result?.Data == null)
                throw new InvalidOperationException(
                    $"DeepSeek ha risposto con status {result?.StatusCode}: {result?.ReasonPhrase}");

            return result.Data.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        }

        // ---------------------------------------------------------------------------
        // Lettura della risposta del LLM
        // ---------------------------------------------------------------------------

        // Accetta gli slot sia come lista nome/valore (schema) sia come oggetto { "nome": "valore" },
        // e un singolo intent senza "actions". Valori numerici o booleani diventano testo.
        public static ExtractedPlan? ParsePlan(string? raw)
        {
            try
            {
                if (string.IsNullOrEmpty(raw)) return null;

                var start = raw.IndexOf('{');
                var end = raw.LastIndexOf('}');
                if (start == -1 || end <= start) return null;

                using var doc = JsonDocument.Parse(raw.Substring(start, end - start + 1));
                var root = doc.RootElement;

                var items = root.TryGetProperty("actions", out var actions) && actions.ValueKind == JsonValueKind.Array
                    ? actions.EnumerateArray().ToList()
                    : root.TryGetProperty("intent", out _) ? new List<JsonElement> { root } : new List<JsonElement>();

                var plan = new ExtractedPlan();
                foreach (var item in items)
                {
                    if (item.ValueKind != JsonValueKind.Object) continue;

                    var action = new ExtractedAction
                    {
                        Intent = item.TryGetProperty("intent", out var intent) ? intent.ToString() : string.Empty,
                        Confidence = item.TryGetProperty("confidence", out var conf) && conf.TryGetDouble(out var c) ? c : 1.0
                    };

                    if (item.TryGetProperty("slots", out var slots))
                    {
                        if (slots.ValueKind == JsonValueKind.Array)
                        {
                            foreach (var s in slots.EnumerateArray())
                                if (s.ValueKind == JsonValueKind.Object &&
                                    s.TryGetProperty("name", out var name) && s.TryGetProperty("value", out var value) &&
                                    JsonValueToString(value) is { } v)
                                    action.Slots[name.ToString()] = v;
                        }
                        else if (slots.ValueKind == JsonValueKind.Object)
                        {
                            foreach (var p in slots.EnumerateObject())
                                if (JsonValueToString(p.Value) is { } v)
                                    action.Slots[p.Name] = v;
                        }
                    }

                    plan.Actions.Add(action);
                }

                return plan;
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "[ChatAI] ParsePlan: JSON non deserializzabile. Raw={Raw}",
                    raw?.Length > 500 ? raw[..500] + "…" : raw);
                return null;
            }
        }

        private static string? JsonValueToString(JsonElement value) => value.ValueKind switch
        {
            JsonValueKind.String => value.GetString(),
            JsonValueKind.Number or JsonValueKind.True or JsonValueKind.False => value.GetRawText(),
            _ => null
        };

        // ---------------------------------------------------------------------------
        // Costruzione del piano dalla risposta del LLM, con controllo anti-allucinazione
        // ---------------------------------------------------------------------------

        private List<ActionEntry> BuildPlanFromLlm(ExtractedPlan? plan, ChatSession session)
        {
            var result = new List<ActionEntry>();
            if (plan == null) return result;

            // Testo dell'utente nella conversazione recente: i valori devono venire da lì
            var userText = string.Join("\n", TrimmedHistory(session.History)
                .Where(t => t.Role == "user").Select(t => t.Content));
            var known = KnownSlotValues(session);
            var usedOld = new HashSet<ActionEntry>();

            foreach (var extracted in plan.Actions)
            {
                if (extracted.Intent.Equals("unknown", StringComparison.OrdinalIgnoreCase) || extracted.Confidence < 0.5)
                    continue;

                var intentDef = FindIntent(extracted.Intent);
                if (intentDef == null) continue;

                var entry = new ActionEntry { Intent = intentDef.Name, State = ActionState.Collecting };
                foreach (var kv in extracted.Slots)
                    AcceptSlot(entry, intentDef, kv.Key, kv.Value, userText, known);

                // Continuità: l'azione del piano in corso con lo stesso intent conserva i valori già
                // raccolti che il LLM non ha ripetuto (i valori nuovi o corretti hanno la precedenza)
                var old = session.ActionQueue.FirstOrDefault(a => a.Intent == intentDef.Name && !usedOld.Contains(a));
                if (old != null)
                {
                    usedOld.Add(old);
                    foreach (var kv in old.Slots)
                        entry.Slots.TryAdd(kv.Key, kv.Value);
                    entry.InfoMessage = old.InfoMessage;
                }

                result.Add(entry);
            }

            return result;
        }

        // Accetta il valore di uno slot solo se è giustificato da ciò che l'utente ha scritto:
        // presente nel testo, già noto (piano in corso o ultime azioni), default, contenuto pertinente
        // secondo lo slot, espressione del testo che si normalizza nello stesso valore, oppure "@me"
        // quando l'utente parla di sé.
        private void AcceptSlot(ActionEntry entry, IntentDefinition intentDef, string slotName, string value,
            string userText, Dictionary<string, HashSet<string>> known)
        {
            var slotDef = intentDef.Slots.FirstOrDefault(s => s.Name == slotName);
            if (slotDef == null || IsNullString(value)) return;

            value = value.Trim();

            // placeholder copiati dal prompt (descrizione, tipo, parole chiave). Una parola chiave è
            // un valore valido se la descrizione dello slot la cita (es. la pagina "logout" per Navigate).
            if (value.Equals(slotDef.PromptDescription, StringComparison.OrdinalIgnoreCase) ||
                value.Equals(slotDef.Type, StringComparison.OrdinalIgnoreCase) ||
                (intentDef.Keywords.Any(k => k.Equals(value, StringComparison.OrdinalIgnoreCase)) &&
                 !ContainsAllWords(slotDef.PromptDescription, value)))
                return;

            var normalizer = slotDef.Normalizer ?? SlotNormalizers.ForType(slotDef.Type);
            var normalized = normalizer?.Invoke(value);
            var stored = normalized ?? value;

            bool isSelf = slotDef.IsPerson && value.Equals(ChatSelf.Token, StringComparison.OrdinalIgnoreCase);

            bool justified =
                isSelf ? ChatSelf.MentionsSelf(userText)
                : ContainsAllWords(userText, value)
                  || (known.TryGetValue(slotName, out var values) && (values.Contains(stored) || values.Contains(value)))
                  || IsDefaultValue(slotDef, value, normalizer, stored)
                  || (slotDef.HasRelevantContent?.Invoke(userText) ?? false)
                  || (normalizer != null && normalized != null && ContainsExpressionFor(userText, normalizer, normalized));

            if (!justified)
            {
                Log.Warning("[ChatAI] Slot scartato (anti-hallucination). Slot={Slot} Valore={Value} Testo={Text}",
                    slotName, value, userText);
                return;
            }

            entry.Slots[slotName] = isSelf ? ChatSelf.Token : stored;
        }

        private static bool IsDefaultValue(SlotDefinition slotDef, string value, Func<string, string?>? normalizer, string stored) =>
            !string.IsNullOrEmpty(slotDef.Default) &&
            (value.Equals(slotDef.Default, StringComparison.OrdinalIgnoreCase) ||
             (normalizer != null && stored == normalizer(slotDef.Default)));

        // tutte le parole del valore compaiono nel testo, in qualunque ordine ("Rossi Mario" in "timbra Mario Rossi")
        private static bool ContainsAllWords(string text, string value)
        {
            var words = ChatAIKeywords.Normalize(text).Split(' ').ToHashSet();
            var valueWords = ChatAIKeywords.Normalize(value).Split(' ', StringSplitOptions.RemoveEmptyEntries);
            return valueWords.Length > 0 && valueWords.All(words.Contains);
        }

        // nel testo c'è un'espressione (fino a 5 parole) che si normalizza nello stesso valore
        // (es. "lunedì scorso" -> la data restituita dal LLM)
        private static bool ContainsExpressionFor(string text, Func<string, string?> normalizer, string normalized)
        {
            var words = text.Split((char[]?)null, StringSplitOptions.RemoveEmptyEntries)
                .Select(w => w.Trim(',', ';', '!', '?', '"', '(', ')'))
                .Where(w => w.Length > 0).ToArray();

            for (int len = 1; len <= 5; len++)
                for (int i = 0; i + len <= words.Length; i++)
                    if (normalizer(string.Join(' ', words, i, len)) == normalized)
                        return true;
            return false;
        }

        // valori già noti per nome di slot: piano in corso e ultime azioni eseguite
        private static Dictionary<string, HashSet<string>> KnownSlotValues(ChatSession session)
        {
            var known = new Dictionary<string, HashSet<string>>();
            foreach (var action in session.ActionQueue.Concat(session.LastExecuted))
                foreach (var kv in action.Slots)
                {
                    if (!known.TryGetValue(kv.Key, out var set))
                        known[kv.Key] = set = new HashSet<string>(StringComparer.OrdinalIgnoreCase);
                    set.Add(kv.Value);
                }
            return known;
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

        private static List<ActionEntry> ClonePlan(IEnumerable<ActionEntry> actions) =>
            actions.Select(a => new ActionEntry
            {
                Intent = a.Intent,
                Slots = new Dictionary<string, string>(a.Slots),
                State = a.State,
                InfoMessage = a.InfoMessage
            }).ToList();

        private static bool SamePlan(List<ActionEntry> a, List<ActionEntry> b) =>
            a.Count == b.Count &&
            a.Zip(b).All(p => p.First.Intent == p.Second.Intent &&
                              p.First.Slots.Count == p.Second.Slots.Count &&
                              p.First.Slots.All(kv => p.Second.Slots.TryGetValue(kv.Key, out var v) && v == kv.Value));

        // ---------------------------------------------------------------------------
        // Validazione slot — opera sull'ActionEntry
        // ---------------------------------------------------------------------------

        private SlotValidationResult ValidateSlotFormatsForAction(ActionEntry action)
        {
            var intentDef = FindIntent(action.Intent);

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

        // ---------------------------------------------------------------------------
        // Slot mancanti
        // ---------------------------------------------------------------------------

        private List<string> GetMissingRequiredSlotsForAction(ActionEntry action)
        {
            var intentDef = FindIntent(action.Intent);

            if (intentDef == null) return new();

            return intentDef.Slots
                .Where(s => s.Required && !action.Slots.ContainsKey(s.Name))
                .Select(s => s.Name)
                .ToList();
        }

        // domanda dello slot senza contesto (per il testo vocale)
        private string GetSlotQuestion(string intentName, string slotName)
        {
            var slotDef = FindSlotDefinition(intentName, slotName);
            return !string.IsNullOrEmpty(slotDef?.Question)
                ? slotDef.Question
                : ChatAIMessages.Get("SlotQuestionDefault", slotName);
        }

        private string BuildMissingSlotQuestion(string slotName, string intentName,
            ActionEntry? action = null, string? actionTitle = null)
        {
            var question = GetSlotQuestion(intentName, slotName);

            // Costruisce il contesto: titolo operazione (opzionale) + slot già raccolti
            var contextParts = new List<string>();

            if (!string.IsNullOrEmpty(actionTitle))
                contextParts.Add(ChatAIMessages.Get("OperationTitle", actionTitle));

            if (action != null && action.Slots.Count > 0)
            {
                var intentDef = FindIntent(intentName);
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
                return CommandResult.Fail(ChatAIMessages.Get("CommandExecutionError"));
            }
        }

        // ---------------------------------------------------------------------------
        // Esecuzione piano completo
        // ---------------------------------------------------------------------------

        private async Task<List<ActionResultItem>> ExecutePlanAsync(ChatSession session)
        {
            var results = new List<ActionResultItem>();

            foreach (var action in session.ActionQueue)
            {
                var intentDef = FindIntent(action.Intent);

                // Esegui validazione/normalizzazione degli slot per ogni azione del piano
                // (es. risoluzione nome canonico dipendente) subito prima dell'esecuzione.
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

                // Azione da eseguire nel client (es. cambio tema o lingua)
                if (cmdResult.Data is ClientActionPayload clientAction)
                    item.ClientAction = clientAction;

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
            sb.AppendLine(ChatAIMessages.Get("PlanSummaryTitle"));
            sb.AppendLine();

            int idx = 1;
            foreach (var action in session.ActionQueue)
            {
                var intentDef = FindIntent(action.Intent);
                var label = !string.IsNullOrEmpty(intentDef?.DisplayName) ? intentDef.DisplayName : action.Intent;

                // Titolo numerato + DisplayName
                sb.AppendLine($"{"①②③④⑤⑥⑦⑧⑨⑩"[Math.Min(idx - 1, 9)]} {label}");

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

            sb.AppendLine(ChatAIMessages.Get("PlanConfirmQuestion"));
            return sb.ToString();
        }

        // Riepilogo in forma di frase per il testo vocale: SummaryTemplate dell'intent con i valori,
        // altrimenti nome dell'operazione e valori. Termina con la domanda di conferma.
        private string BuildPlanSpeech(ChatSession session)
        {
            var sentences = session.ActionQueue.Select(action =>
            {
                var intentDef = FindIntent(action.Intent);
                var label = GetDisplayName(action.Intent);

                if (!string.IsNullOrEmpty(intentDef?.SummaryTemplate))
                    return Regex.Replace(intentDef.SummaryTemplate, @"\{(\w+)\}", m =>
                    {
                        if (action.Slots.TryGetValue(m.Groups[1].Value, out var value)) return value;

                        // valore mancante: il default, normalizzato ("oggi" -> la data)
                        var slotDef = intentDef.Slots.FirstOrDefault(s => s.Name == m.Groups[1].Value);
                        if (string.IsNullOrEmpty(slotDef?.Default)) return string.Empty;
                        var normalizer = slotDef.Normalizer ?? SlotNormalizers.ForType(slotDef.Type);
                        return normalizer?.Invoke(slotDef.Default) ?? slotDef.Default;
                    });

                return action.Slots.Count == 0 ? label : $"{label}: {string.Join(", ", action.Slots.Values)}";
            });

            return string.Join(". ", sentences) + ". " + ChatAIMessages.Get("PlanConfirmQuestion");
        }

        private string GetDisplayName(string intentName)
        {
            var intentDef = FindIntent(intentName);
            return !string.IsNullOrEmpty(intentDef?.DisplayName) ? intentDef.DisplayName : intentName;
        }

        // ---------------------------------------------------------------------------
        // Helper risposta
        // ---------------------------------------------------------------------------

        private ChatAIOutModel BuildQuestionResponse(ChatSession session, string message,
            List<string>? suggestions = null, string? speechText = null) =>
            new()
            {
                SessionId = session.SessionId,
                Responce = message,
                SpeechText = speechText ?? message,
                ResponseType = "question",
                Suggestions = suggestions ?? new()
            };

        // Ripete la domanda in sospeso (piano in corso) preceduta da un avviso
        private ChatAIOutModel BuildCurrentQuestionResponse(ChatSession session, string notice)
        {
            var action = session.ActionQueue.FirstOrDefault(a => GetMissingRequiredSlotsForAction(a).Any())
                         ?? session.CurrentAction;
            var missing = action != null ? GetMissingRequiredSlotsForAction(action).FirstOrDefault() : null;

            var message = missing != null && action != null
                ? notice + "\n" + BuildMissingSlotQuestion(missing, action.Intent, action, GetDisplayName(action.Intent))
                : notice;
            session.AddToHistory("assistant", message);
            return BuildQuestionResponse(session, message,
                speechText: missing != null && action != null ? notice + " " + GetSlotQuestion(action.Intent, missing) : notice);
        }

        // Piano in attesa di conferma e risposta né sì né no: si resta in attesa di conferma,
        // così la risposta successiva ("sì") esegue subito il piano
        private ChatAIOutModel BuildClarificationResponse(ChatSession session)
        {
            session.State = SessionState.ReadyToExecute;
            var clarification = ChatAIMessages.Get("ConfirmClarification");
            session.AddToHistory("assistant", clarification);
            return BuildQuestionResponse(session, clarification, BuildYesNoSuggestions());
        }

        private ChatAIOutModel BuildConfirmationResponse(ChatSession session, string message) =>
            new()
            {
                SessionId = session.SessionId,
                Responce = message,
                SpeechText = BuildPlanSpeech(session),
                ResponseType = "confirmation",
                Suggestions = BuildYesNoSuggestions(),
                AutoConfirm = IsNavigationOnly(session)
            };

        // Vero se il piano contiene solo navigazioni (nessuna scrittura di dati):
        // solo in questo caso il client vocale può confermare da solo.
        private bool IsNavigationOnly(ChatSession session) =>
            session.ActionQueue.All(a => FindIntent(a.Intent)?.IsNavigation == true);

        // Chip di conferma nella lingua dell'utente: il testo viene rimandato al server,
        // che riconosce si/yes/ok/no... in entrambe le lingue (ChatAIKeywords).
        // Il primo chip e' sempre la conferma (il client lo usa per l'auto-conferma vocale).
        private static List<string> BuildYesNoSuggestions() =>
            new() { ChatAIMessages.Get("SuggestionYes"), ChatAIMessages.Get("SuggestionNo") };

        private ChatAIOutModel BuildErrorResponse(ChatSession session, string message,
            List<string>? suggestions = null) =>
            new()
            {
                SessionId = session.SessionId,
                Responce = message,
                SpeechText = message,
                ResponseType = "error",
                Suggestions = suggestions ?? new()
            };

        // massimo dei chip con i comandi suggeriti (Help compreso)
        private const int MaxIntentSuggestions = 6;

        // Chip dei comandi suggeriti (DisplayName, che il LLM riconosce come richiesta): prima quelli
        // utili nella pagina corrente (IntentDefinition.Pages), poi gli altri, per ultimo l'help che
        // li elenca tutti. Il catalogo è già filtrato sui ruoli dell'utente.
        private List<string> BuildIntentSuggestions()
        {
            static string Label(IntentDefinition i) => !string.IsNullOrEmpty(i.DisplayName) ? i.DisplayName : i.Name;
            bool IsHelp(IntentDefinition i) => i.Name.Equals("Help", StringComparison.OrdinalIgnoreCase);

            var route = _chatUserContext.CurrentRoute ?? string.Empty;
            var intents = _intentCatalog.Intents.Where(i => !IsHelp(i)).ToList();
            var ofPage = route.Length == 0
                ? new List<IntentDefinition>()
                : intents.Where(i => i.Pages.Any(p => route.StartsWith(p, StringComparison.OrdinalIgnoreCase))).ToList();

            var suggestions = ofPage.Concat(intents.Except(ofPage))
                .Take(MaxIntentSuggestions - 1)
                .Select(Label)
                .ToList();

            var help = _intentCatalog.Intents.FirstOrDefault(IsHelp);
            if (help != null) suggestions.Add(Label(help));
            return suggestions;
        }

        // Frasi che la chat non ha capito: elenco da cui ricavare sinonimi, esempi e comandi nuovi.
        // Testo fisso "ChatAI frase non capita" per filtrarle nei log (file o tabella Serilog).
        private void LogNotUnderstood(ChatSession session, string userMessage)
        {
            var pending = session.ActionQueue.FirstOrDefault()?.Intent ?? string.Empty;
            Log.Information("[ChatAI] ChatAI frase non capita. Frase={Frase} Pagina={Pagina} Ruoli={Ruoli} PianoInCorso={PianoInCorso} Lingua={Lingua}",
                userMessage, session.CurrentRoute, string.Join(",", _chatUserContext.Roles), pending, ChatAIMessages.CurrentLanguage);
        }

        // ---------------------------------------------------------------------------
        // Session store
        // ---------------------------------------------------------------------------

        // Cerca una sessione esistente valida. Ritorna null se:
        // - sessionId è vuoto (primo messaggio del client)
        // - sessione non trovata (mai esistita)
        // - sessione scaduta (> 10 minuti di inattività) → rimossa
        // - sessione di un altro utente
        private ChatSession? TryGetSession(string sessionId) =>
            _sessionStore.TryGet(sessionId, CurrentUserId);

        private ChatSession CreateSession() =>
            _sessionStore.Create(CurrentUserId);

        private void DeleteSession(string sessionId) =>
            _sessionStore.Delete(sessionId);

        // ---------------------------------------------------------------------------
        // API key dei provider LLM
        // ---------------------------------------------------------------------------

        // AI:<provider>:ApiKey, segreto: variabile d'ambiente NVXAPP_AI__<provider>__ApiKey (non in appsettings).
        // Obbligatoria solo per il provider in uso (AI:UseLLM); per gli altri puo' mancare.
        private string ApiKey(IConfiguration configuration, string provider)
        {
            var key = configuration[$"AI:{provider}:ApiKey"]?.Trim() ?? string.Empty;
            if (key.Length == 0 && _useLLM == provider)
                throw new InvalidOperationException(
                    $"API key di {provider} non configurata: impostare la variabile d'ambiente NVXAPP_AI__{provider}__ApiKey.");
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
