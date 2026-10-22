using nvxapp.server.data.Localization;
using System.Globalization;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    /// <summary>
    /// Testi dell'assistente ChatAI nella lingua dell'utente (CultureInfo.CurrentUICulture).
    /// I .resx stanno nell'assembly dei dati: nvxapp.server.data/Localization/ChatAIMessages*.resx
    /// </summary>
    public static class ChatAIMessages
    {
        private static readonly ServerText _text =
            new("nvxapp.server.data.Localization.ChatAIMessages", typeof(InfrastructureMessages));

        public static string Get(string key, params object?[] args) => _text.Get(key, args);

        /// <summary>codice lingua della richiesta corrente ("it" o "en")</summary>
        public static string CurrentLanguage => CultureInfo.CurrentUICulture.TwoLetterISOLanguageName;

        /// <summary>nome della lingua corrente da usare nel system prompt del LLM</summary>
        public static string CurrentLanguageName => CurrentLanguage == "en" ? "inglese (English)" : "italiano";
    }

    /// <summary>
    /// Parole chiave dei comandi di controllo della chat.
    /// Si accetta sempre l'unione italiano + inglese, indipendentemente dalla lingua dell'interfaccia,
    /// cosi' funzionano anche i testi rimandati dai chip (es. "Si'"/"Yes") e gli utenti che mescolano le lingue.
    /// </summary>
    public static class ChatAIKeywords
    {
        // risposte esatte (dopo trim/minuscolo/punteggiatura finale) che confermano il piano
        public static readonly string[] Confirm =
        [
            "sì", "si", "sí", "confermo", "conferma", "ok", "okay", "va bene", "certo", "procedi",
            "yes", "yep", "yeah", "y", "confirm", "confirmed", "sure", "go ahead", "proceed"
        ];

        // risposte esatte che annullano il piano
        public static readonly string[] Cancel =
        [
            "no", "annulla", "cancel", "nope", "abort", "n"
        ];

        // contenuti (substring) che azzerano la conversazione
        public static readonly string[] Reset =
        [
            "annulla", "reset", "ricomincia", "riparti", "nuovo", "stop",
            "esci", "basta", "restart", "cancella", "abbandona", "interrompi",
            "cancel", "start over", "abort", "quit"
        ];

        // contenuti (substring) che chiedono l'help
        public static readonly string[] Help =
        [
            "aiuto", "help", "comandi", "menu", "cosa sai fare", "cosa puoi fare",
            "commands", "what can you do"
        ];

        /// <summary>testo normalizzato per il confronto: trim, minuscolo, senza punteggiatura finale</summary>
        public static string Normalize(string? text) =>
            (text ?? string.Empty).Trim().TrimEnd('.', '!', '?', ',', ';').Trim().ToLowerInvariant();

        public static bool IsExactMatch(string? text, string[] keywords)
        {
            var t = Normalize(text);
            return keywords.Contains(t);
        }

        public static bool ContainsAny(string? text, string[] keywords)
        {
            var t = Normalize(text);
            return keywords.Any(k => t.Contains(k));
        }
    }
}
