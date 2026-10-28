using Newtonsoft.Json;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using System.Text;
using System.Text.Json.Serialization;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models
{

    // ---------------------------------------------------------------------------
    // Modelli Client ↔ Server
    // ---------------------------------------------------------------------------

    public class ChatAIInModel
    {
        public string Request { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;  // identifica la sessione conversazionale
        public string CurrentRoute { get; set; } = string.Empty; // pagina del client in cui si trova l'utente (help contestuale)
    }

    public class ChatAIOutModel : ModelResult
    {
        public string Responce { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;  // restituito al client per i turni successivi
        public string ResponseType { get; set; } = string.Empty;  // "question" | "confirmation" | "result" | "error" | "navigate" | "partial_error"
        public List<string> Suggestions { get; set; } = new();     // chip/bottoni opzionali da mostrare al client
        public NavigatePayload? Navigate { get; set; }             // payload di navigazione (ResponseType == "navigate")
        public List<ActionResultItem> ActionResults { get; set; } = new(); // risultati per singola azione del piano
        // Solo con ResponseType == "confirmation": true se il piano contiene solo navigazioni,
        // quindi il client vocale può confermare da solo; con scritture di dati la conferma è dell'utente.
        public bool AutoConfirm { get; set; }
        // Testo breve da pronunciare (risposta vocale): la domanda senza il contesto, il riepilogo
        // in forma di frase, i risultati. Vuoto = usare Responce.
        public string SpeechText { get; set; } = string.Empty;
        // Azione da eseguire nel client (es. cambio tema o lingua): ultima azione client del piano
        public ClientActionPayload? ClientAction { get; set; }

        public ChatAIOutModel() { }
    }

    // ---------------------------------------------------------------------------
    // Sessione conversazionale
    // ---------------------------------------------------------------------------

    public class ChatSession
    {
        public string SessionId { get; set; } = Guid.NewGuid().ToString();
        public string UserId { get; set; } = string.Empty;
        public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity { get; set; } = DateTime.UtcNow;
        public SessionState State { get; set; } = SessionState.Collecting;

        // Piano di esecuzione multi-azione
        public List<ActionEntry> ActionQueue { get; set; } = new();
        public int CurrentActionIndex { get; set; } = 0;

        // Azione corrente nel piano (null se piano non ancora inizializzato)
        public ActionEntry? CurrentAction =>
            ActionQueue.Count > CurrentActionIndex ? ActionQueue[CurrentActionIndex] : null;

        // Azioni dell'ultimo piano eseguito: restano come contesto per i messaggi successivi
        // ("anche per Bianchi", "stessa cosa domani").
        public List<ActionEntry> LastExecuted { get; set; } = new();

        // Shim di compatibilità — delegano all'azione corrente
        public string Intent => CurrentAction?.Intent ?? string.Empty;
        public Dictionary<string, string> Slots => CurrentAction?.Slots ?? new();

        public List<ConversationTurn> History { get; set; } = new();

        // Pagina del client al momento dell'ultimo messaggio (contesto per il LLM)
        public string CurrentRoute { get; set; } = string.Empty;

        public void AddToHistory(string role, string content)
        {
            History.Add(new ConversationTurn { Role = role, Content = content });
            LastActivity = DateTime.UtcNow;
        }

        public void MergeSlots(Dictionary<string, string> newSlots)
        {
            var current = CurrentAction;
            if (current == null) return;
            foreach (var kv in newSlots)
                if (!string.IsNullOrEmpty(kv.Value) && kv.Value != "null")
                    current.Slots[kv.Key] = kv.Value;
        }

        public void ResetSlot(string slotName) =>
            CurrentAction?.Slots.Remove(slotName);
    }

    public class ConversationTurn
    {
        public string Role { get; set; } = string.Empty;  // "user" | "assistant"
        public string Content { get; set; } = string.Empty;
    }

    public enum SessionState
    {
        Collecting,      // raccolta slot in corso
        ReadyToExecute,  // tutti gli slot presenti, in attesa conferma
        Confirmed,       // utente ha confermato, pronto per eseguire
        Executing,       // chiamata API in corso
        Done,            // completato con successo
        Error            // errore di business o tecnico
    }

    public enum ActionState
    {
        Collecting,  // raccolta slot in corso
        Ready,       // tutti gli slot presenti, pronta per essere eseguita
        Done,        // eseguita con successo
        Failed       // errore durante l'esecuzione
    }

    public enum ExecutionStrategy
    {
        StopOnError,    // interrompe il piano al primo errore (default)
        ContinueOnError // esegue tutto e riporta i risultati parziali
    }

    // Singola azione nel piano di esecuzione
    public class ActionEntry
    {
        public string Intent { get; set; } = string.Empty;
        public Dictionary<string, string> Slots { get; set; } = new();
        public ActionState State { get; set; } = ActionState.Collecting;
        public bool IsConditional { get; set; }
        public CommandResult? Result { get; set; }
        // Messaggio informativo prodotto dal validator (es. "Dipendente agganciato: Rossi Mario").
        // Viene mostrato in linea con i valori dell'azione nel riepilogo di conferma.
        public string? InfoMessage { get; set; }
    }

    // Azione eseguita dal client Angular senza navigare (CommandResult.Data di un handler)
    public class ClientActionPayload
    {
        public string Action { get; set; } = string.Empty;   // "theme" | "language"
        public string Value { get; set; } = string.Empty;    // theme: "dark" | "light" | "toggle"; language: "it" | "en"
    }

    // Payload di navigazione restituito al client Angular
    public class NavigatePayload
    {
        public string Route { get; set; } = string.Empty;
        public Dictionary<string, string> Params { get; set; } = new();
        // Stato passato al router Angular (es. { id: 42 } per aprire un record specifico)
        public Dictionary<string, object> State { get; set; } = new();
    }

    // Risultato di una singola azione nel piano
    public class ActionResultItem
    {
        public string Intent { get; set; } = string.Empty;
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public NavigatePayload? Navigate { get; set; }
        public ClientActionPayload? ClientAction { get; set; }
    }

    // ---------------------------------------------------------------------------
    // Risultato estrazione LLM
    // ---------------------------------------------------------------------------

    // Piano multi-azione restituito dal LLM (letto da ChatAIService.ParsePlan)
    public class ExtractedPlan
    {
        [JsonPropertyName("actions")]
        public List<ExtractedAction> Actions { get; set; } = new();
    }

    public class ExtractedAction
    {
        [JsonPropertyName("intent")]
        public string Intent { get; set; } = string.Empty;

        [JsonPropertyName("slots")]
        public Dictionary<string, string> Slots { get; set; } = new();

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }

    // ---------------------------------------------------------------------------
    // Validazione slot
    // ---------------------------------------------------------------------------

    public class SlotValidationResult
    {
        public bool IsValid { get; set; }
        public SlotValidationError ErrorType { get; set; }
        public string MessageToUser { get; set; } = string.Empty;
        public string InvalidSlotName { get; set; } = string.Empty;  // slot da resettare in sessione
        public List<string> Suggestions { get; set; } = new();         // per AmbiguousEntity
        public string? NormalizedValue { get; set; }                   // valore canonico da salvare in sessione
        public string? InfoMessage { get; set; }                       // messaggio informativo (non blocca il flusso)

        public static SlotValidationResult Ok() => new() { IsValid = true };

        // Ok con normalizzazione: il validator ha trovato il valore canonico da salvare
        public static SlotValidationResult Ok(string normalizedValue, string? infoMessage = null) =>
            new() { IsValid = true, NormalizedValue = normalizedValue, InfoMessage = infoMessage };

        public static SlotValidationResult Failed(SlotValidationError type, string message, string slotName, List<string>? suggestions = null) =>
            new()
            {
                IsValid = false,
                ErrorType = type,
                MessageToUser = message,
                InvalidSlotName = slotName,
                Suggestions = suggestions ?? new()
            };
    }

    public enum SlotValidationError
    {
        InvalidFormat,        // valore non parsabile (es. orario "26:00")
        EntityNotFound,       // dipendente non trovato nel DB
        AmbiguousEntity,      // più dipendenti con lo stesso nome
        BusinessRuleViolation // ferie sovrapposte, periodo non valido, ecc.
    }

    // ---------------------------------------------------------------------------
    // Risultato esecuzione comando
    // ---------------------------------------------------------------------------

    public class CommandResult
    {
        public bool Success { get; set; }
        public string Message { get; set; } = string.Empty;
        public object? Data { get; set; }

        public static CommandResult Ok(string message, object? data = null) =>
            new() { Success = true, Message = message, Data = data };

        public static CommandResult Fail(string message) =>
            new() { Success = false, Message = message };
    }

    // ---------------------------------------------------------------------------
    // Ollama — Request / Response
    // ---------------------------------------------------------------------------

    // ---------------------------------------------------------------------------
    // Ollama /api/generate — usato internamente, mantenuto per compatibilità
    // ---------------------------------------------------------------------------

    //public class OllamaRequest
    //{
    //    [JsonPropertyName("model")]
    //    public string Model  { get; set; } = string.Empty;

    //    [JsonPropertyName("prompt")]
    //    public string Prompt { get; set; } = string.Empty;

    //    [JsonPropertyName("system")]
    //    public string System { get; set; } = string.Empty;

    //    [JsonPropertyName("stream")]
    //    public bool Stream   { get; set; } = false;

    //    [JsonPropertyName("format")]
    //    public string Format { get; set; } = "json";
    //}

    //public class OllamaResponse
    //{
    //    [JsonPropertyName("response")]
    //    public string Response { get; set; } = string.Empty;

    //    [JsonPropertyName("done")]
    //    public bool Done { get; set; }
    //}

    // ---------------------------------------------------------------------------
    // Ollama /api/chat — supporta array messages per la ConversationHistory
    // ---------------------------------------------------------------------------

    public class OllamaChatRequest
    {
        [JsonPropertyName("model")]
        [JsonProperty("model")]
        public string Model { get; set; } = string.Empty;

        [JsonPropertyName("messages")]
        [JsonProperty("messages")]
        public List<OllamaChatMessage> Messages { get; set; } = new();

        [JsonPropertyName("stream")]
        [JsonProperty("stream")]
        public bool Stream { get; set; } = false;

        // "json" oppure lo schema JSON della risposta (output strutturato di Ollama)
        [JsonPropertyName("format")]
        [JsonProperty("format")]
        public object Format { get; set; } = "json";
    }

    public class OllamaChatMessage
    {
        [JsonPropertyName("role")]
        [JsonProperty("role")]
        public string Role { get; set; } = string.Empty;

        [JsonPropertyName("content")]
        [JsonProperty("content")]
        public string Content { get; set; } = string.Empty;
    }

    public class OllamaChatResponse
    {
        [JsonPropertyName("message")]
        [JsonProperty("message")]
        public OllamaChatMessage? Message { get; set; }

        [JsonPropertyName("done")]
        [JsonProperty("done")]
        public bool Done { get; set; }
    }

    // ---------------------------------------------------------------------------
    // Catalogo Intent — si auto-costruisce dagli ICommandHandler iniettati.
    // Non contiene definizioni hard-coded: ogni handler porta la propria.
    // ---------------------------------------------------------------------------

    public interface IIntentCatalog
    {
        IReadOnlyList<IntentDefinition> Intents { get; }


        //string OLD_BuildSystemPrompt(List<string>? currIntentNames);
        //string OLD_BuildPlanSystemPrompt();
        string BuildTurnSystemPrompt(ChatSession session, ICollection<string> detailedIntents);
        object BuildResponseSchema();
        void Fuilter4UserRoles(List<string> userRoles);
    }

    public class IntentCatalog : IIntentCatalog
    {
        private /*readonly*/ IReadOnlyList<IntentDefinition> _intents;

        // Riceve tutti gli ICommandHandler registrati nella DI.
        // Estrae la IntentDefinition da ognuno e costruisce il catalogo.
        public IntentCatalog(IEnumerable<ICommandHandler> handlers)
        {
            _intents = handlers
                .Select(h => h.IntentDefinition)
                .ToList()
                .AsReadOnly();
        }

        public void Fuilter4UserRoles(List<string> userRoles)
        {
            if (_intents != null)
            {
                _intents = _intents
                    .Where(intent => intent.Usable4Role.Contains("*") || intent.Usable4Role.Intersect(userRoles).Any())
                    .ToList()
                    .AsReadOnly();
            }
        }


        public IReadOnlyList<IntentDefinition> Intents => _intents;

        //public string OLD_BuildPlanSystemPrompt()
        //{
        //    var sb = new StringBuilder();

        //    sb.AppendLine("Risondi in italiano");
        //    sb.AppendLine("Sei un assistente che estrae un piano di azioni da testo in italiano.");
        //    sb.AppendLine("La risposta deve contenere UNA O PIÙ azioni da eseguire in sequenza.");
        //    sb.AppendLine("Rispondi SOLO con un oggetto JSON valido, nessun testo aggiuntivo.");
        //    sb.AppendLine();

        //    BuildSystemPromptUtil.BuildSystemPrompt_Append_Intestazione_Comune(sb);
        //    BuildSystemPromptUtil.OLD_BuildSystemPrompt_Append_Intent_Definition(sb, _intents, null);

        //    sb.AppendLine("Intent disponibili:");
        //    sb.AppendLine();

        //    for (int i = 0; i < _intents.Count; i++)
        //    {
        //        var intent = _intents[i];
        //        sb.AppendLine($"{i + 1}. {intent.Name}");
        //        sb.AppendLine($"   Descrizione: {intent.Description}");

        //        if (intent.Keywords.Count > 0)
        //        {
        //            sb.Append("   Parole chiave: ");
        //            sb.AppendLine(string.Join(", ", intent.Keywords));
        //        }

        //        if (intent.Slots.Count > 0)
        //        {
        //            sb.AppendLine("   Slot:");
        //            foreach (var slot in intent.Slots)
        //            {
        //                var req = slot.Required ? "obbligatorio" : "opzionale";
        //                var def = !string.IsNullOrEmpty(slot.Default) ? $", default {slot.Default}" : "";
        //                var desc = !string.IsNullOrEmpty(slot.PromptDescription) ? $": {slot.PromptDescription}" : "";
        //                sb.AppendLine($"   - {slot.Name} ({slot.Type}, {req}{def}){desc}");
        //            }
        //        }

        //        sb.AppendLine();
        //    }

        //    sb.AppendLine("Rispondi SEMPRE e SOLO con questo JSON (array di azioni):");
        //    sb.AppendLine("{");
        //    sb.AppendLine("  \"actions\": [");
        //    sb.AppendLine("    {");
        //    sb.AppendLine("      \"intent\": \"NomeIntent\",");
        //    sb.AppendLine("      \"slots\": { \"nomeSlot\": \"valore o null se non presente\" },");
        //    sb.AppendLine("      \"confidence\": 0.95");
        //    sb.AppendLine("    }");
        //    sb.AppendLine("  ]");
        //    sb.AppendLine("}");

        //    return sb.ToString();
        //}
        //public string OLD_BuildSystemPrompt(List<string>? currIntentNames)
        //{
        //    var sb = new StringBuilder();

        //    sb.AppendLine("Sei un assistente che estrae intent e slot da testo in italiano.");
        //    sb.AppendLine("Rispondi SOLO con un oggetto JSON valido, nessun testo aggiuntivo.");

        //    // NOTA: Se anche questi metodi di utilità accettavano il vecchio parametro stringa, 
        //    // andranno eventualmente aggiornati o gestiti separatamente se necessario.
        //    BuildSystemPromptUtil.BuildSystemPrompt_Append_Intestazione_Comune(sb);

        //    // Se necessario, estrai il primo nome o adatta il metodo di utilità qui sotto
        //    string? firstIntentName = currIntentNames?.FirstOrDefault();
        //    BuildSystemPromptUtil.OLD_BuildSystemPrompt_Append_Intent_Definition(sb, _intents, firstIntentName);

        //    #region "Intent"

        //    sb.AppendLine("Intent disponibili:");
        //    sb.AppendLine();

        //    // Gestione della lista di intenti ricevuta come parametro
        //    List<IntentDefinition> intentsToInclude = (currIntentNames == null || currIntentNames.Count == 0)
        //        ? _intents.ToList()
        //        : _intents.Where(i => currIntentNames.Contains(i.Name, StringComparer.OrdinalIgnoreCase)).ToList();

        //    for (int i = 0; i < intentsToInclude.Count; i++)
        //    {
        //        var intent = intentsToInclude[i];
        //        sb.AppendLine($"{i + 1}. {intent.Name}");
        //        sb.AppendLine($"   Descrizione: {intent.Description}");

        //        if (intent.Keywords.Count > 0)
        //        {
        //            //sb.AppendLine();
        //            sb.Append("   La richiesta può contenere le parole: ");
        //            foreach (var iKey in intent.Keywords)
        //            {
        //                sb.Append($"{iKey},");
        //            }
        //            sb.AppendLine();
        //            sb.AppendLine();
        //        }

        //        sb.AppendLine($"   Slot:");

        //        foreach (var slot in intent.Slots)
        //        {
        //            var obbligatorio = slot.Required ? "obbligatorio" : "opzionale";
        //            var defaultVal = !string.IsNullOrEmpty(slot.Default) ? $", default {slot.Default}" : "";
        //            var description = !string.IsNullOrEmpty(slot.PromptDescription) ? $": {slot.PromptDescription}" : "";
        //            sb.AppendLine($"   - {slot.Name} ({slot.Type}, {obbligatorio}{defaultVal}){description}");
        //        }

        //        sb.AppendLine();
        //    }

        //    #endregion

        //    sb.AppendLine("Rispondi sempre e solo con questo JSON,\n");

        //    sb.AppendLine("{");
        //    sb.AppendLine("  \"intent\": \"NomeIntent\",");
        //    sb.AppendLine("  \"slots\": {");
        //    sb.AppendLine("    \"nomeSlot\": \"valore o null se non presente\"");
        //    sb.AppendLine("  },");
        //    sb.AppendLine("  \"missingRequired\": [\"slot1\"],");
        //    sb.AppendLine("  \"confidence\": 0.95");
        //    sb.AppendLine("}");

        //    return sb.ToString();
        //}

        // System prompt di ogni turno: regole, contesto (data e ora), catalogo degli intent e stato
        // della conversazione. Il LLM restituisce ogni volta il piano completo aggiornato.
        // Catalogo a due livelli per contenere i token: tutti gli intent in forma compatta (nome,
        // descrizione, esempi: il riconoscimento non dipende dalle parole chiave), slot e parole chiave
        // solo per detailedIntents (piano in corso, ultime azioni, parole chiave presenti nel messaggio).
        public string BuildTurnSystemPrompt(ChatSession session, ICollection<string> detailedIntents)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Sei l'assistente di un'applicazione gestionale: trasformi il messaggio dell'utente in un piano di azioni (intent e slot).");
            sb.AppendLine("Rispondi SOLO con un oggetto JSON valido.");
            sb.AppendLine();

            BuildSystemPromptUtil.BuildSystemPrompt_Append_Intestazione_Comune(sb);
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Contesto(sb, DateTime.Now, session.CurrentRoute);
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Intents(sb, _intents, detailedIntents);
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Stato(sb, session, _intents);
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Regole_Piano(sb, _intents.Any(i => i.Slots.Any(s => s.IsPerson)));
            BuildSystemPromptUtil.RetValJsoDefinition(sb);

            return sb.ToString();
        }

        // Schema JSON della risposta (output strutturato): il nome dell'intent è vincolato al catalogo.
        // Gli slot sono una lista nome/valore per restare compatibili con la modalità strict.
        public object BuildResponseSchema()
        {
            var slotSchema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["name"] = new Dictionary<string, object> { ["type"] = "string" },
                    ["value"] = new Dictionary<string, object> { ["type"] = "string" }
                },
                ["required"] = new[] { "name", "value" },
                ["additionalProperties"] = false
            };

            var actionSchema = new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["intent"] = new Dictionary<string, object> { ["type"] = "string", ["enum"] = _intents.Select(i => i.Name).ToArray() },
                    ["slots"] = new Dictionary<string, object> { ["type"] = "array", ["items"] = slotSchema },
                    ["confidence"] = new Dictionary<string, object> { ["type"] = "number" }
                },
                ["required"] = new[] { "intent", "slots", "confidence" },
                ["additionalProperties"] = false
            };

            return new Dictionary<string, object>
            {
                ["type"] = "object",
                ["properties"] = new Dictionary<string, object>
                {
                    ["actions"] = new Dictionary<string, object> { ["type"] = "array", ["items"] = actionSchema }
                },
                ["required"] = new[] { "actions" },
                ["additionalProperties"] = false
            };
        }

    }

    public static class BuildSystemPromptUtil
    {

        public static void BuildSystemPrompt_Append_Intestazione_Comune(StringBuilder sb)
        {
            sb.AppendLine("REGOLA FONDAMENTALE:");
            sb.AppendLine("Estrai SOLO i valori esplicitamente scritti dall'utente (nel messaggio o nella conversazione), per i quali è possibile una interpretazione usando le descrizioni degli slot.");
            sb.AppendLine("Se il valore di uno slot non è noto, non includere lo slot: le domande per i valori mancanti le fa il sistema.");
            //sb.AppendLine("NON inventare valori. NON completare slot mancanti con valori plausibili o di esempio.");
            //sb.AppendLine("Un valore mancante in \"missingRequired\" è la risposta corretta — non un errore.");
            sb.AppendLine("");

            BuildSystemPrompt_Append_Lingua(sb);

            //BuildSystemPrompt_Append_Data_Oggi(sb);

        }

        // Lingua dell'utente: CultureInfo.CurrentUICulture della richiesta ("it" o "en").
        // Il prompt resta in italiano; il modello deve capire testi in italiano e in inglese.
        public static void BuildSystemPrompt_Append_Lingua(StringBuilder sb)
        {
            sb.AppendLine("LINGUA:");
            sb.AppendLine($"La lingua dell'interfaccia dell'utente è: {ChatAIMessages.CurrentLanguageName} (codice \"{ChatAIMessages.CurrentLanguage}\").");
            sb.AppendLine("L'utente può scrivere in italiano o in inglese: interpreta la richiesta in entrambe le lingue.");
            sb.AppendLine("Esempi: 'vai alla pagina aziende' e 'go to the companies page' indicano lo stesso intent con lo stesso slot.");
            sb.AppendLine("I nomi degli intent, i nomi degli slot e le chiavi JSON restano SEMPRE quelli indicati (non tradurli).");
            sb.AppendLine("Gli eventuali testi liberi nei valori vanno scritti nella lingua dell'utente; i valori vanno estratti come scritti dall'utente.");
            sb.AppendLine("Date e orari si copiano come li ha espressi l'utente (es. 'lunedì scorso', 'il 15 maggio', 'alle 9 e mezza'): la conversione la fa il sistema.");
            sb.AppendLine("");
        }

        // Data e ora correnti: servono al LLM per capire riferimenti come "stamattina" o "venerdì"
        // (la conversione delle date resta al sistema, vedi SlotNormalizers).
        public static void BuildSystemPrompt_Append_Contesto(StringBuilder sb, DateTime now, string? currentRoute = null)
        {
            var giorno = CultureInfo.GetCultureInfo("it-IT").DateTimeFormat.GetDayName(now.DayOfWeek);
            sb.AppendLine("CONTESTO:");
            sb.AppendLine($"Oggi è {giorno} {now:dd/MM/yyyy}, sono le {now:HH:mm}.");
            if (!string.IsNullOrWhiteSpace(currentRoute))
                sb.AppendLine($"L'utente si trova nella pagina {currentRoute}.");
            sb.AppendLine("");
        }

        // Catalogo degli intent: per tutti nome, descrizione ed esempi; per quelli in detailedIntents
        // anche parole chiave (solo indicative) e slot.
        public static void BuildSystemPrompt_Append_Intents(StringBuilder sb, IReadOnlyList<IntentDefinition> intents,
            ICollection<string> detailedIntents)
        {
            sb.AppendLine("INTENT DISPONIBILI:");
            sb.AppendLine("Il valore del campo \"intent\" deve essere SEMPRE uno dei nomi seguenti: non tradurlo, non usare la descrizione, non inventare nomi.");
            sb.AppendLine("L'utente può esprimere la stessa richiesta con parole diverse da quelle degli esempi.");
            sb.AppendLine("Per gli intent elencati senza slot, se la richiesta corrisponde restituisci comunque l'intent (con slot vuoti): i dettagli arrivano dopo.");
            sb.AppendLine();

            for (int i = 0; i < intents.Count; i++)
            {
                var intent = intents[i];
                bool detailed = detailedIntents.Contains(intent.Name);
                sb.AppendLine($"{i + 1}. {intent.Name} — {intent.Description}");

                // forma compatta: un solo esempio (con molti intent il prompt resta contenuto)
                var examples = detailed ? intent.Examples : intent.Examples.Take(1).ToList();
                if (examples.Count > 0)
                    sb.AppendLine($"   Esempi: {string.Join(" | ", examples.Select(e => $"\"{e}\""))}");

                if (!detailed) continue;

                if (intent.Keywords.Count > 0)
                    sb.AppendLine($"   Parole chiave: {string.Join(", ", intent.Keywords)}");

                if (intent.Slots.Count > 0)
                {
                    sb.AppendLine("   Slot:");
                    foreach (var slot in intent.Slots)
                    {
                        var req = slot.Required ? "obbligatorio" : "opzionale";
                        var def = !string.IsNullOrEmpty(slot.Default) ? $", default {slot.Default}" : "";
                        var person = slot.IsPerson ? $", persona: \"{ChatSelf.Token}\" se l'utente parla di sé" : "";
                        var desc = !string.IsNullOrEmpty(slot.PromptDescription) ? $": {slot.PromptDescription.Trim()}" : "";
                        sb.AppendLine($"   - {slot.Name} ({slot.Type}, {req}{def}{person}){desc}");
                    }
                }
            }

            sb.AppendLine();
        }

        // Stato della conversazione: piano in corso con i valori già raccolti e ultime azioni eseguite.
        public static void BuildSystemPrompt_Append_Stato(StringBuilder sb, ChatSession session, IReadOnlyList<IntentDefinition> intents)
        {
            sb.AppendLine("STATO DELLA CONVERSAZIONE:");

            if (session.ActionQueue.Count == 0)
                sb.AppendLine("Nessun piano in corso.");
            else
            {
                sb.AppendLine("Piano in corso (azioni non ancora eseguite):");
                for (int i = 0; i < session.ActionQueue.Count; i++)
                {
                    var action = session.ActionQueue[i];
                    var missing = intents.FirstOrDefault(d => d.Name == action.Intent)?.Slots
                        .Where(s => s.Required && !action.Slots.ContainsKey(s.Name))
                        .Select(s => s.Name).ToList() ?? new();
                    var line = $"{i + 1}. {DescribeAction(action)}";
                    if (missing.Count > 0) line += $" — mancano: {string.Join(", ", missing)}";
                    sb.AppendLine(line);
                }
            }

            if (session.LastExecuted.Count > 0)
            {
                sb.AppendLine("Ultime azioni eseguite (per richieste come \"anche per Bianchi\" o \"stessa cosa domani\"):");
                for (int i = 0; i < session.LastExecuted.Count; i++)
                    sb.AppendLine($"{i + 1}. {DescribeAction(session.LastExecuted[i])}");
            }

            sb.AppendLine();
        }

        private static string DescribeAction(ActionEntry action) =>
            action.Slots.Count == 0
                ? action.Intent
                : $"{action.Intent}: {string.Join("; ", action.Slots.Select(kv => $"{kv.Key} = \"{kv.Value}\""))}";

        public static void BuildSystemPrompt_Append_Regole_Piano(StringBuilder sb, bool hasPersonSlots)
        {
            sb.AppendLine("COME COSTRUIRE IL PIANO:");
            sb.AppendLine("- Restituisci SEMPRE il piano COMPLETO aggiornato: le azioni ancora da eseguire, ciascuna con TUTTI i valori noti.");
            sb.AppendLine("- Se il messaggio risponde a una domanda, completa o corregge il piano in corso: mantieni le stesse azioni con i valori già noti e aggiungi o sostituisci quelli indicati. Il messaggio può contenere più valori insieme: estraili tutti.");
            sb.AppendLine("- Se il messaggio chiede una cosa diversa dal piano in corso, sostituisci il piano con le nuove azioni.");
            sb.AppendLine("- Se il messaggio si riferisce alle ultime azioni eseguite (\"anche per Bianchi\", \"stessa cosa domani\"), crea nuove azioni dello stesso intent copiando i loro valori e cambiando solo ciò che l'utente indica.");
            sb.AppendLine("- Una richiesta può contenere più azioni (\"timbra Rossi alle 9 e Bianchi alle 10\" = due azioni).");
            sb.AppendLine("- Includi solo gli slot di cui conosci il valore: non inventare valori e non usare i valori degli esempi.");
            if (hasPersonSlots)
                sb.AppendLine($"- Se l'utente parla di sé (io, me, mi, mio, mia, \"timbrami\", I, me, my), negli slot persona usa il valore \"{ChatSelf.Token}\".");
            sb.AppendLine("- Se il messaggio non corrisponde a nessun intent, restituisci \"actions\": [].");
            sb.AppendLine("- \"confidence\" indica quanto sei sicuro dell'intent (da 0 a 1).");
            sb.AppendLine();
        }

        public static void OLD_BuildSystemPrompt_Append_Intent_Definition(StringBuilder sb, IReadOnlyList<IntentDefinition> intents, string? intentName)
        {
            sb.AppendLine();
            sb.AppendLine("IMPORTANTE:");
            sb.AppendLine("Il valore del campo \"intent\" deve essere SEMPRE uno dei seguenti:");


            IReadOnlyList<IntentDefinition> intentsToInclude = string.IsNullOrEmpty(intentName)
                ? intents
                : intents.Where(i => i.Name.Equals(intentName, StringComparison.OrdinalIgnoreCase)).ToList();


            foreach (var intent in intentsToInclude)
            {
                sb.AppendLine($"- {intent.Name}");
            }

            sb.AppendLine("Non usare mai la descrizione come nome dell’intent.");
            sb.AppendLine("Non inventare nuovi nomi.");
            sb.AppendLine("Non tradurre i nomi degli intent.");
        }


        // Descrizione degli slot data (PromptDescription): il LLM copia l'espressione dell'utente,
        // la conversione in dd/MM/yyyy la fa SlotNormalizers.Date.
        public static void BuildSystemPrompt_Append_Data_Oggi(StringBuilder sb)
        {
            sb.Append("data: copia l'espressione usata dall'utente così come l'ha scritta ");
            sb.Append("(es. 'oggi', 'ieri', 'lunedì scorso', 'il 15', '15 maggio', '15/05/2026', 'yesterday', 'May 15'). ");
            sb.Append("Non calcolare la data: la converte il sistema.");
        }

        // Descrizione degli slot orario (PromptDescription): il LLM copia l'espressione dell'utente,
        // la conversione in HH:mm la fa SlotNormalizers.Time.
        public static void BuildSystemPrompt_Append_Orario(StringBuilder sb)
        {
            sb.Append("orario: copia l'espressione usata dall'utente così come l'ha scritta ");
            sb.Append("(es. 'alle 9', '9 e mezza', '14:30', 'adesso', '2 pm', 'half past ten'). ");
            sb.Append("Non convertirla: la converte il sistema.");
        }

        public static void RetValJsoDefinition(StringBuilder sb)
        {
            sb.AppendLine("FORMATO DELLA RISPOSTA (sempre e solo questo JSON):");
            sb.AppendLine("{\"actions\":[{\"intent\":\"NomeIntent\",\"slots\":[{\"name\":\"nomeSlot\",\"value\":\"valore\"}],\"confidence\":0.95}]}");
        }

    }



    public class SlotDefinition
    {
        public string Name { get; set; } = string.Empty;
        public string Type { get; set; } = string.Empty;
        public bool Required { get; set; }
        public string Default { get; set; } = string.Empty;

        // Descrizione usata da Ollama nel system prompt per estrarre il valore
        // (es. "orario nel formato HH:mm. Se l'utente dice 'alle 9' restituisci '09:00'").
        public string PromptDescription { get; set; } = string.Empty;

        // Domanda da porre all'utente quando lo slot è mancante
        // (es. "A che orario? (es. 09:00)").
        public string Question { get; set; } = string.Empty;

        // Etichetta leggibile per il riepilogo di conferma
        // (es. "Orario").
        public string Label { get; set; } = string.Empty;

        // Validazione del singolo valore dello slot.
        // Ritorna null se il valore è valido, SlotValidationResult.Failed(...) altrimenti.
        // Vive nel handler — ChatAIService non hardcoda nomi di slot.
        public Func<string, SlotValidationResult?>? Validator { get; set; }

        // Determina se il messaggio utente contiene contenuto rilevante per questo slot.
        // Se null, l'handler usa il comportamento di default (true).
        // Usare per filtrare slot temporali/numerici prima di interrogare il modello.
        public Func<string, bool>? HasRelevantContent { get; set; }

        // Converte l'espressione dell'utente nel valore canonico (null se non riesce: resta il testo
        // e lo valuta il Validator). Se non è impostato si usa quello del tipo:
        // SlotNormalizers.ForType ("dd/MM/yyyy" -> data, "HH:mm" -> orario).
        public Func<string, string?>? Normalizer { get; set; }

        // Slot che indica una persona: se l'utente parla di sé ("timbrami", "le mie ore") il LLM
        // restituisce ChatSelf.Token e il Validator dell'handler lo risolve nell'utente corrente.
        public bool IsPerson { get; set; }
    }

    public class IntentDefinition
    {
        public string Name { get; set; } = string.Empty;
        // Etichetta breve per chip/bottoni UI (es. "Timbratura")
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<SlotDefinition> Slots { get; set; } = new();

        public List<string> Usable4Role { get; set; } = new List<string>();

        // Validazione cross-slot (es. startDate <= endDate).
        // Chiamata dopo che tutti gli slot singoli sono validi.
        // Ritorna null se tutto è ok, SlotValidationResult.Failed(...) altrimenti.
        public Func<Dictionary<string, string>, SlotValidationResult?>? CrossValidator { get; set; }

        // Parole chiave minime che devono essere presenti nel testo dell'utente
        // per considerare l'intent plausibile PRIMA di chiamare Ollama.
        // Almeno una keyword deve fare match (case-insensitive, substring).
        // Se vuoto, il pre-filtro viene saltato per questo intent.
        // ATTENZIONE CON MODELLI EVOLUTI, SI POTRA ELIMINARE
        public List<string> Keywords { get; set; } = new();

        // Se true, l'intent esegue solo una navigazione lato client (nessun accesso dati).
        // Il CommandHandler restituisce un NavigatePayload in CommandResult.Data.
        public bool IsNavigation { get; set; }

        // Strategia di esecuzione in un piano multi-azione.
        public ExecutionStrategy ExecutionStrategy { get; set; } = ExecutionStrategy.StopOnError;

        // Frasi di esempio (italiano e inglese) che l'utente potrebbe dire: aiutano il LLM
        // a riconoscere l'intent anche con parole diverse dalle Keywords.
        public List<string> Examples { get; set; } = new();

        // Riepilogo e conferma prima dell'esecuzione. Se non impostato: richiesti, tranne per
        // gli intent di navigazione (non modificano dati e si eseguono subito).
        public bool? RequiresConfirmation { get; set; }
        public bool NeedsConfirmation => RequiresConfirmation ?? !IsNavigation;

        // Frase dell'azione per il testo vocale, con segnaposto {nomeSlot}
        // (es. "timbratura di {employeeName} alle {time} del {date}"). Vuoto = nome e valori.
        public string SummaryTemplate { get; set; } = string.Empty;

        // Pagine del client (route, anche solo l'inizio) in cui l'intent è particolarmente utile:
        // l'help le usa per proporre per primi i comandi della pagina corrente.
        public List<string> Pages { get; set; } = new();
    }


    #region "OpenRouter"

    public class OpenRouterChatResponse
    {
        public List<OpenRouterChoice> Choices { get; set; } = new List<OpenRouterChoice>();
    }

    public class OpenRouterChoice
    {
        public OpenRouterMessage Message { get; set; } = new OpenRouterMessage();
    }

    public class OpenRouterMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    #endregion


    #region "GroQ"

    public class GroqChatResponse
    {
        public List<GroqChoice> Choices { get; set; } = new List<GroqChoice>();
    }

    public class GroqChoice
    {
        public GroqMessage Message { get; set; } = new GroqMessage();
    }

    public class GroqMessage
    {
        public string Role { get; set; } = string.Empty;
        public string Content { get; set; } = string.Empty;
    }

    #endregion

}
