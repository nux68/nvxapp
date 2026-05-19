using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;
using System.Text.Json.Serialization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models
{

    // ---------------------------------------------------------------------------
    // Modelli Client ↔ Server
    // ---------------------------------------------------------------------------

    public class ChatAIInModel
    {
        public string Request   { get; set; } = string.Empty;
        public string SessionId { get; set; } = string.Empty;  // identifica la sessione conversazionale
    }

    public class ChatAIOutModel : ModelResult
    {
        public string Responce      { get; set; } = string.Empty;
        public string SessionId     { get; set; } = string.Empty;  // restituito al client per i turni successivi
        public string ResponseType  { get; set; } = string.Empty;  // "question" | "confirmation" | "result" | "error"
        public List<string> Suggestions { get; set; } = new();     // chip/bottoni opzionali da mostrare al client

        public ChatAIOutModel() { }
    }

    // ---------------------------------------------------------------------------
    // Sessione conversazionale
    // ---------------------------------------------------------------------------

    public class ChatSession
    {
        public string SessionId     { get; set; } = Guid.NewGuid().ToString();
        public string UserId        { get; set; } = string.Empty;
        public DateTime CreatedAt   { get; set; } = DateTime.UtcNow;
        public DateTime LastActivity{ get; set; } = DateTime.UtcNow;
        public SessionState State   { get; set; } = SessionState.Collecting;

        public string Intent        { get; set; } = string.Empty;
        public Dictionary<string, string> Slots { get; set; } = new();
        public List<ConversationTurn> History   { get; set; } = new();

        public void AddToHistory(string role, string content)
        {
            History.Add(new ConversationTurn { Role = role, Content = content });
            LastActivity = DateTime.UtcNow;
        }

        // Merge: aggiorna solo gli slot non null ricevuti dal LLM
        public void MergeSlots(Dictionary<string, string> newSlots)
        {
            foreach (var kv in newSlots)
                if (!string.IsNullOrEmpty(kv.Value) && kv.Value != "null")
                    Slots[kv.Key] = kv.Value;
        }

        public void ResetSlot(string slotName)
        {
            Slots.Remove(slotName);
        }
    }

    public class ConversationTurn
    {
        public string Role    { get; set; } = string.Empty;  // "user" | "assistant"
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

    // ---------------------------------------------------------------------------
    // Validazione slot
    // ---------------------------------------------------------------------------

    public class SlotValidationResult
    {
        public bool IsValid             { get; set; }
        public SlotValidationError ErrorType { get; set; }
        public string MessageToUser     { get; set; } = string.Empty;
        public string InvalidSlotName   { get; set; } = string.Empty;  // slot da resettare in sessione
        public List<string> Suggestions { get; set; } = new();         // per AmbiguousEntity
        public static SlotValidationResult Ok() =>new() { IsValid = true };

        public static SlotValidationResult Failed(SlotValidationError type, string message, string slotName, List<string>? suggestions = null) =>
            new()
            {
                IsValid         = false,
                ErrorType       = type,
                MessageToUser   = message,
                InvalidSlotName = slotName,
                Suggestions     = suggestions ?? new()
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
        public bool Success         { get; set; }
        public string Message       { get; set; } = string.Empty;
        public object ?Data          { get; set; }

        public static CommandResult Ok(string message, object ?data = null) =>
            new() { Success = true, Message = message, Data = data };

        public static CommandResult Fail(string message) =>
            new() { Success = false, Message = message };
    }

    // ---------------------------------------------------------------------------
    // Ollama — Request / Response
    // ---------------------------------------------------------------------------

    public class OllamaRequest
    {
        [JsonPropertyName("model")]
        public string Model  { get; set; } = string.Empty;

        [JsonPropertyName("prompt")]
        public string Prompt { get; set; } = string.Empty;

        [JsonPropertyName("system")]
        public string System { get; set; } = string.Empty;

        [JsonPropertyName("stream")]
        public bool Stream   { get; set; } = false;

        [JsonPropertyName("format")]
        public string Format { get; set; } = "json";
    }

    public class OllamaResponse
    {
        [JsonPropertyName("response")]
        public string Response { get; set; } = string.Empty;

        [JsonPropertyName("done")]
        public bool Done { get; set; }
    }

    // ---------------------------------------------------------------------------
    // Catalogo Intent — si auto-costruisce dagli ICommandHandler iniettati.
    // Non contiene definizioni hard-coded: ogni handler porta la propria.
    // ---------------------------------------------------------------------------

    public interface IIntentCatalog
    {
        IReadOnlyList<IntentDefinition> Intents { get; }
        string BuildSystemPrompt();
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

        public string BuildSystemPrompt()
        {
            var sb = new StringBuilder();

            sb.AppendLine("Sei un assistente che estrae intent e slot da testo in italiano.");
            sb.AppendLine("Rispondi SOLO con un oggetto JSON valido, nessun testo aggiuntivo.");
            sb.AppendLine();
            sb.AppendLine($"Data di oggi: {DateTime.Today:yyyy-MM-dd}");
            sb.AppendLine();
            sb.AppendLine("Intent disponibili:");
            sb.AppendLine();

            for (int i = 0; i < _intents.Count; i++)
            {
                var intent = _intents[i];
                sb.AppendLine($"{i + 1}. {intent.Name}");
                sb.AppendLine($"   Descrizione: {intent.Description}");
                sb.AppendLine($"   Slot:");

                foreach (var slot in intent.Slots)
                {
                    var obbligatorio = slot.Required ? "obbligatorio" : "opzionale";
                    var defaultVal   = !string.IsNullOrEmpty(slot.Default) ? $", default {slot.Default}" : "";
                    sb.AppendLine($"   - {slot.Name} ({slot.Type}, {obbligatorio}{defaultVal})");
                }

                sb.AppendLine();
            }

            sb.AppendLine("Rispondi sempre e solo con questo JSON:");
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

    public class IntentDefinition
    {
        public string Name        { get; set; } = string.Empty;
        public string Description { get; set; } = string.Empty;
        public List<SlotDefinition> Slots { get; set; } = new();
    }

    public class SlotDefinition
    {
        public string Name     { get; set; } = string.Empty;
        public string Type     { get; set; } = string.Empty;
        public bool   Required { get; set; }
        public string Default  { get; set; } = string.Empty;
    }

}
