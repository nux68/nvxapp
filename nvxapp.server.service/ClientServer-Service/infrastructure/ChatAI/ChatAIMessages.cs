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
        // espressioni che confermano il piano
        public static readonly string[] Confirm =
        [
            "sì", "si", "sí", "confermo", "conferma", "ok", "okay", "va bene", "certo", "procedi",
            "yes", "yep", "yeah", "y", "confirm", "confirmed", "sure", "go ahead", "proceed"
        ];

        // parole che possono accompagnare una conferma senza cambiarne il senso ("sì grazie", "ok procedi pure")
        private static readonly string[] ConfirmFiller =
        [
            "grazie", "pure", "vai", "perfetto", "esatto", "giusto", "certamente", "tutto", "bene", "corretto",
            "please", "thanks", "thank", "you", "do", "it", "fine", "correct", "right", "all", "good", "go"
        ];

        // espressioni che annullano il piano
        public static readonly string[] Cancel =
        [
            "no", "annulla", "cancel", "nope", "abort", "n", "non confermo", "non procedere", "don't"
        ];

        // parole che possono accompagnare un annullamento ("no grazie", "no lascia stare")
        private static readonly string[] CancelFiller =
        [
            "grazie", "lascia", "lasciamo", "stare", "perdere", "niente", "nulla", "tutto", "non", "importa", "fa",
            "thanks", "thank", "you", "never", "mind", "forget", "it", "nothing", "do", "proceed", "confirm"
        ];

        // frasi intere che azzerano la conversazione
        // (a frase intera: "cancella la timbratura di Rossi" non deve azzerare nulla)
        public static readonly string[] Reset =
        [
            "annulla", "annulla tutto", "reset", "ricomincia", "ricominciamo", "ricomincia da capo", "da capo",
            "riparti", "ripartiamo", "nuovo", "nuova richiesta", "stop", "esci", "basta", "basta così",
            "lascia stare", "lascia perdere", "lasciamo perdere", "cancella", "cancella tutto",
            "abbandona", "interrompi",
            "restart", "start over", "cancel", "abort", "quit", "never mind", "forget it"
        ];

        // frasi intere che chiedono l'help
        public static readonly string[] Help =
        [
            "aiuto", "help", "comandi", "menu", "cosa sai fare", "cosa puoi fare", "che cosa sai fare",
            "che cosa puoi fare", "che comandi ci sono", "quali comandi", "quali sono i comandi",
            "elenco comandi", "lista comandi", "mi aiuti",
            "commands", "what can you do", "help me", "list commands"
        ];

        /// <summary>testo normalizzato per il confronto: minuscolo, punteggiatura sostituita da spazi, spazi singoli</summary>
        public static string Normalize(string? text)
        {
            var chars = (text ?? string.Empty).ToLowerInvariant()
                .Select(c => char.IsLetterOrDigit(c) || c == '\'' ? c : ' ')
                .ToArray();
            return string.Join(' ', new string(chars).Split(' ', StringSplitOptions.RemoveEmptyEntries));
        }

        public static bool IsExactMatch(string? text, string[] keywords)
        {
            var t = Normalize(text);
            return keywords.Contains(t);
        }

        /// <summary>conferma tollerante: "sì", ma anche "sì grazie", "ok procedi pure" (non "sì ma alle 10")</summary>
        public static bool IsConfirm(string? text) => IsTolerantMatch(text, Confirm, ConfirmFiller);

        /// <summary>annullamento tollerante: "no", ma anche "no grazie", "no lascia stare" (non "no, alle 10")</summary>
        public static bool IsCancel(string? text) => IsTolerantMatch(text, Cancel, CancelFiller);

        // vero se il testo contiene almeno una delle espressioni (a parole intere)
        // e tutte le sue parole appartengono alle espressioni o alle parole di contorno
        private static bool IsTolerantMatch(string? text, string[] keywords, string[] filler)
        {
            var t = Normalize(text);
            if (t.Length == 0) return false;
            if (keywords.Contains(t)) return true;

            var padded = $" {t} ";
            if (!keywords.Any(k => padded.Contains($" {k} "))) return false;

            var vocabulary = keywords.SelectMany(k => k.Split(' ')).Concat(filler).ToHashSet();
            return t.Split(' ').All(vocabulary.Contains);
        }
    }
}
