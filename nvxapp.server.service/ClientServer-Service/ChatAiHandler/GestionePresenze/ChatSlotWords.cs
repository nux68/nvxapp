using System.Globalization;
using System.Text.RegularExpressions;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Parole degli slot riconosciute in italiano e in inglese, qualunque sia la lingua dell'interfaccia
    // (stesso criterio delle keywords: unione delle due lingue).
    public static class ChatSlotWords
    {
        // giorni relativi che indicano una data
        public static readonly string[] RelativeDays =
        {
            "oggi", "domani", "ieri",
            "today", "tomorrow", "yesterday"
        };

        // giorni della settimana e mesi: indicano una data anche senza cifre ("lunedì", "primo maggio")
        private static readonly string[] DateNames =
        {
            "dopodomani", "altroieri",
            "lunedì", "lunedi", "martedì", "martedi", "mercoledì", "mercoledi", "giovedì", "giovedi",
            "venerdì", "venerdi", "sabato", "domenica",
            "monday", "tuesday", "wednesday", "thursday", "friday", "saturday", "sunday",
            "gennaio", "febbraio", "marzo", "aprile", "maggio", "giugno", "luglio", "agosto",
            "settembre", "ottobre", "novembre", "dicembre",
            "january", "february", "march", "april", "may", "june", "july", "august",
            "september", "october", "november", "december"
        };

        // date scritte con cifre: "15/05", "15-5-2026", "15.05.2026", "il 15", "dal 3 al 7", "on the 15th".
        // Non basta una cifra qualsiasi: "alle 9" o "alle 8.30" sono orari, non date.
        private static readonly Regex DateDigits = new(
            @"\b\d{1,2}\s*[/\-]\s*\d{1,2}\b|\b\d{1,2}\.\d{1,2}\.\d{2,4}\b|\b(il|del|dal|al|fino al|entro il|on|the|from|to)\s+\d{1,2}\b|\b\d{1,2}(st|nd|rd|th)\b",
            RegexOptions.IgnoreCase | RegexOptions.CultureInvariant);

        // Da usare come SlotDefinition.HasRelevantContent degli slot data: il LLM normalizza la data
        // (es. "il 15 maggio" -> "15/05/2026"), quindi il valore non compare nel testo e il filtro
        // anti-allucinazione lo scarterebbe se il messaggio non fosse riconosciuto come "contiene una data".
        public static bool HasDateContent(string? msg)
        {
            if (string.IsNullOrWhiteSpace(msg)) return false;
            if (RelativeDays.Any(w => msg.Contains(w, StringComparison.OrdinalIgnoreCase))) return true;
            if (DateDigits.IsMatch(msg)) return true;

            var words = msg.ToLowerInvariant()
                .Split(new[] { ' ', ',', '.', ';', ':', '!', '?', '\'', '"', '(', ')' }, StringSplitOptions.RemoveEmptyEntries);
            return words.Any(w => DateNames.Contains(w));
        }

        // "oggi" (anche valore di default degli slot data) oppure "today"
        public static bool IsToday(string? value)
        {
            var v = (value ?? string.Empty).Trim();
            return v.Equals("oggi", StringComparison.OrdinalIgnoreCase) ||
                   v.Equals("today", StringComparison.OrdinalIgnoreCase);
        }

        // Valore di uno slot data -> data. Formati espliciti e cultura invariante: con DateOnly.TryParse
        // un utente con interfaccia inglese otterrebbe "15/05/2026" non valido e quindi la data di oggi.
        public static DateTime ParseDateOrToday(string? value)
        {
            if (IsToday(value)) return DateTime.Today;

            var v = (value ?? string.Empty).Trim();
            return DateOnly.TryParseExact(v, new[] { "dd/MM/yyyy", "d/M/yyyy", "yyyy-MM-dd" },
                       CultureInfo.InvariantCulture, DateTimeStyles.None, out var d)
                ? d.ToDateTime(TimeOnly.MinValue)
                : DateTime.Today;
        }
    }
}
