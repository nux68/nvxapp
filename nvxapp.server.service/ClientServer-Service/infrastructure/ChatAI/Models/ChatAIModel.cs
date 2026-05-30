using Newtonsoft.Json;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
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
    }

    public class ChatAIOutModel : ModelResult
    {
        public string Responce { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;  // restituito al client per i turni successivi
        public string ResponseType { get; set; } = string.Empty;  // "question" | "confirmation" | "result" | "error" | "navigate" | "partial_error"
        public List<string> Suggestions { get; set; } = new();     // chip/bottoni opzionali da mostrare al client
        public NavigatePayload? Navigate { get; set; }             // payload di navigazione (ResponseType == "navigate")
        public List<ActionResultItem> ActionResults { get; set; } = new(); // risultati per singola azione del piano

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

        // Shim di compatibilità — delegano all'azione corrente
        public string Intent => CurrentAction?.Intent ?? string.Empty;
        public Dictionary<string, string> Slots => CurrentAction?.Slots ?? new();

        public List<ConversationTurn> History { get; set; } = new();

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
    }

    // ---------------------------------------------------------------------------
    // Risultato estrazione LLM
    // ---------------------------------------------------------------------------

    public class ExtractedIntent
    {
        [JsonPropertyName("intent")]
        public string Intent { get; set; } = string.Empty;

        [JsonPropertyName("slots")]
        public Dictionary<string, string> Slots { get; set; } = new();

        [JsonPropertyName("missingRequired")]
        public List<string> MissingRequired { get; set; } = new();

        [JsonPropertyName("confidence")]
        public double Confidence { get; set; }
    }

    // Piano multi-azione restituito dal LLM al primo turno
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

        [JsonPropertyName("format")]
        [JsonProperty("format")]
        public string Format { get; set; } = "json";
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
        string BuildSystemPrompt(string? intentName = null);
        string BuildPlanSystemPrompt();
    }

    public class IntentCatalog : IIntentCatalog
    {
        private readonly IReadOnlyList<IntentDefinition> _intents;

        // Riceve tutti gli ICommandHandler registrati nella DI.
        // Estrae la IntentDefinition da ognuno e costruisce il catalogo.
        public IntentCatalog(IEnumerable<ICommandHandler> handlers)
        {
            _intents = handlers
                .Select(h => h.IntentDefinition)
                .ToList()
                .AsReadOnly();
        }

        public IReadOnlyList<IntentDefinition> Intents => _intents;

        public string BuildPlanSystemPrompt()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Risondi in italiano");
            sb.AppendLine("Sei un assistente che estrae un piano di azioni da testo in italiano.");
            sb.AppendLine("La risposta deve contenere UNA O PIÙ azioni da eseguire in sequenza.");
            sb.AppendLine("Rispondi SOLO con un oggetto JSON valido, nessun testo aggiuntivo.");
            sb.AppendLine();

            BuildSystemPromptUtil.BuildSystemPrompt_Append_Intestazione_Comune(sb);
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Intent_Definition(sb, _intents, null);

            sb.AppendLine("Intent disponibili:");
            sb.AppendLine();

            for (int i = 0; i < _intents.Count; i++)
            {
                var intent = _intents[i];
                sb.AppendLine($"{i + 1}. {intent.Name}");
                sb.AppendLine($"   Descrizione: {intent.Description}");

                if (intent.Keywords.Count > 0)
                {
                    sb.Append("   Parole chiave: ");
                    sb.AppendLine(string.Join(", ", intent.Keywords));
                }

                if (intent.Slots.Count > 0)
                {
                    sb.AppendLine("   Slot:");
                    foreach (var slot in intent.Slots)
                    {
                        var req = slot.Required ? "obbligatorio" : "opzionale";
                        var def = !string.IsNullOrEmpty(slot.Default) ? $", default {slot.Default}" : "";
                        var desc = !string.IsNullOrEmpty(slot.PromptDescription) ? $": {slot.PromptDescription}" : "";
                        sb.AppendLine($"   - {slot.Name} ({slot.Type}, {req}{def}){desc}");
                    }
                }

                sb.AppendLine();
            }

            sb.AppendLine("Rispondi SEMPRE e SOLO con questo JSON (array di azioni):");
            sb.AppendLine("{");
            sb.AppendLine("  \"actions\": [");
            sb.AppendLine("    {");
            sb.AppendLine("      \"intent\": \"NomeIntent\",");
            sb.AppendLine("      \"slots\": { \"nomeSlot\": \"valore o null se non presente\" },");
            sb.AppendLine("      \"confidence\": 0.95");
            sb.AppendLine("    }");
            sb.AppendLine("  ]");
            sb.AppendLine("}");

            return sb.ToString();
        }

        public string BuildSystemPrompt(string? currIntentName)
        {
            var sb = new StringBuilder();

            sb.AppendLine("Sei un assistente che estrae intent e slot da testo in italiano.");
            sb.AppendLine("Rispondi SOLO con un oggetto JSON valido, nessun testo aggiuntivo.");

            BuildSystemPromptUtil.BuildSystemPrompt_Append_Intestazione_Comune(sb);

            BuildSystemPromptUtil.BuildSystemPrompt_Append_Intent_Definition(sb, _intents,currIntentName);

            #region "Intent"

                sb.AppendLine("Intent disponibili:");

                sb.AppendLine();

                List<IntentDefinition> intentsToInclude = string.IsNullOrEmpty(currIntentName)
                    ? _intents.ToList()
                    : _intents.Where(i => i.Name.Equals(currIntentName, StringComparison.OrdinalIgnoreCase)).ToList();

                for (int i = 0; i < intentsToInclude.Count; i++)
                {
                    var intent = intentsToInclude[i];
                    sb.AppendLine($"{i + 1}. {intent.Name}");
                    sb.AppendLine($"   Descrizione: {intent.Description}");

                    if (intent.Keywords.Count > 0)
                    {
                        //sb.AppendLine();
                        sb.Append("   La richiesta può contenere le parole: ");
                        foreach (var iKey in intent.Keywords)
                        {
                            sb.Append($"{iKey},");
                        }
                        sb.AppendLine();
                        sb.AppendLine();
                    }


                    sb.AppendLine($"   Slot:");

                    foreach (var slot in intent.Slots)
                    {
                        var obbligatorio = slot.Required ? "obbligatorio" : "opzionale";
                        var defaultVal = !string.IsNullOrEmpty(slot.Default) ? $", default {slot.Default}" : "";
                        var description = !string.IsNullOrEmpty(slot.PromptDescription) ? $": {slot.PromptDescription}" : "";
                        sb.AppendLine($"   - {slot.Name} ({slot.Type}, {obbligatorio}{defaultVal}){description}");
                    }

                    sb.AppendLine();
                }

            #endregion


            sb.AppendLine("Rispondi sempre e solo con questo JSON,\n"
                          //+ "senza modificare, riformulare o reinterpretare alcun testo degli intent\n" 
                          //+ "o delle loro descrizioni.\n" 
                          //+ "Mantieni esattamente i nomi e le descrizioni come definiti sopra:"
                          );

            sb.AppendLine("{");
            sb.AppendLine("  \"intent\": \"NomeIntent\",");
            sb.AppendLine("  \"slots\": {");
            sb.AppendLine("    \"nomeSlot\": \"valore o null se non presente\"");
            sb.AppendLine("  },");
            sb.AppendLine("  \"missingRequired\": [\"slot1\"],");
            sb.AppendLine("  \"confidence\": 0.95");
            sb.AppendLine("}");



            return sb.ToString();
        }



    }

    public static class BuildSystemPromptUtil
    {

        public static void BuildSystemPrompt_Append_Intestazione_Comune(StringBuilder sb)
        {
            sb.AppendLine("REGOLA FONDAMENTALE:");
            sb.AppendLine("Estrai SOLO i valori esplicitamente scritti dall'utente, per i queli è possibile una interpretazione usando gli esempi specificati negli slot");
            sb.AppendLine("Se uno slot obbligatorio non è presente nel testo, inseriscilo in \"missingRequired\" e metti null come valore.");
            sb.AppendLine("NON inventare valori. NON completare slot mancanti con valori plausibili o di esempio.");
            sb.AppendLine("Un valore mancante in \"missingRequired\" è la risposta corretta — non un errore.");
            sb.AppendLine("");

            BuildSystemPrompt_Append_Data_Oggi(sb);

        }

        public static void BuildSystemPrompt_Append_Intent_Definition(StringBuilder sb, IReadOnlyList<IntentDefinition> intents,string? intentName)
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

        public static void BuildSystemPrompt_Append_Data_Oggi(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine(@"Data nel formato dd/MM/yyyy.
                            Accetta anche date scritte come '18/05/2026', '18-05-2026', '18 maggio 2026'.
                            o anche date scritte come 'primo maggio 2026', 'due maggio 2026', '3 maggio 2026'.
                            Normalizza sempre in formato dd/MM/yyyy");

            sb.AppendLine();
            sb.AppendLine($"Data di oggi: {DateTime.Today:dd/MM/yyyy}");
            sb.AppendLine();
            sb.AppendLine("IMPORTANTE:");
            sb.AppendLine("Quando l’utente usa date relative come 'oggi', 'domani', 'ieri',");
            sb.AppendLine("Il nome del mese puo essere fornito in forma letterale 'gennaio', 'febbraio', 'marzo',");
            sb.AppendLine("devi sempre convertirle in una data assoluta nel formato dd/MM/yyyy.");
            sb.AppendLine($"Usa come riferimento la data indicata sopra. Oggi è {DateTime.Today:dd/MM/yyyy}");
        }

        public static void BuildSystemPrompt_Append_Orario(StringBuilder sb)
        {
            sb.AppendLine();
            sb.AppendLine(@"Orario nel formato HH:mm.
                            Se l'utente dice 'alle 9' restituisci '09:00'.
                            Accetta anche varianti come '9', '9.00', '9:0' e normalizzale.
                            Se l'utente dice 'dieci' restituisci '10:00'.
                            Se l'utente dice 'dieci e mezza' restituisci '10:30'.
                            Non dedurre la risposta dagli esempi, al massimo puoi trasformare un
                            un numero espresso in forma letterale trasformandolo un cifra numerica come nell' esempio fornito");
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
    }

    public class IntentDefinition
    {
        public string Name { get; set; } = string.Empty;
        // Etichetta breve per chip/bottoni UI (es. "Timbratura")
        public string DisplayName { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<SlotDefinition> Slots { get; set; } = new();

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
