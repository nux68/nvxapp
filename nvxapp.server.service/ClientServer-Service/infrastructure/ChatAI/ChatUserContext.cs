using System.Text.RegularExpressions;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Utente della richiesta chat corrente, a disposizione degli handler (Scoped: un'istanza per richiesta).
    // Lo valorizza ChatAIService all'inizio di SendMessage; gli handler lo usano per risolvere ChatSelf.Token.
    public interface IChatUserContext
    {
        string UserId { get; set; }
    }

    public class ChatUserContext : IChatUserContext
    {
        public string UserId { get; set; } = string.Empty;
    }

    // L'utente che parla di sé ("timbrami alle 9", "le mie ferie", "my hours").
    // Per gli slot persona (SlotDefinition.IsPerson) il LLM restituisce Token; l'handler lo risolve
    // nella persona collegata all'utente corrente (IChatUserContext.UserId).
    public static class ChatSelf
    {
        public const string Token = "@me";

        private static readonly HashSet<string> _words = new(StringComparer.OrdinalIgnoreCase)
        {
            "io", "me", "mi", "mio", "mia", "miei", "mie", "stesso", "stessa",
            "i", "my", "myself", "mine"
        };

        // verbi con il pronome attaccato: "timbrami", "registrami", "segnami"
        private static readonly Regex _verbWithMi = new(@"\b\p{L}{4,}mi\b", RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        public static bool MentionsSelf(string? message)
        {
            if (string.IsNullOrWhiteSpace(message)) return false;
            if (_verbWithMi.IsMatch(message)) return true;
            return ChatAIKeywords.Normalize(message).Split(' ').Any(_words.Contains);
        }
    }
}
