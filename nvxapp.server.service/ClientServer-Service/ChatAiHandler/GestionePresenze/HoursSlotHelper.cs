using System.Globalization;
using System.Text.RegularExpressions;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Converte una stringa ore in TimeSpan.
    // Formati accettati:
    //   HH:mm        (es. "08:00", "04:30")
    //   numero intero scritto in lettere IT (es. "uno", "due", ..., "ventiquattro")
    //   espressioni di durata (es. "2 ore", "due ore", "un'ora e mezza", "mezz'ora", "2 ore e 30",
    //   "2,5 ore", "8h", "4 hours", "half an hour")
    public static class HoursSlotHelper
    {
        // Descrizione degli slot ore per il LLM: copia l'espressione, la conversione la fa Normalize
        public const string PromptDescription =
            "ore: copia l'espressione usata dall'utente così come l'ha scritta " +
            "(es. '2 ore', 'otto', '08:00', 'mezz'ora', 'un'ora e mezza', '4 hours'). Non convertirla: la converte il sistema.";

        // Normalizzatore dello slot (SlotDefinition.Normalizer): espressione -> "HH:mm", null se non riconosciuta
        public static string? Normalize(string value) =>
            TryParse(value, out var ts) ? $"{(int)ts.TotalHours:D2}:{ts.Minutes:D2}" : null;

        private static readonly Dictionary<string, int> _fractions = new(StringComparer.OrdinalIgnoreCase)
        {
            ["mezza"] = 30, ["mezzo"] = 30, ["un quarto"] = 15, ["tre quarti"] = 45, ["a half"] = 30, ["half"] = 30,
        };

        private static readonly Dictionary<string, int> _wordsToHours =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["uno"]           = 1,
                ["un"]            = 1,
                ["due"]           = 2,
                ["tre"]           = 3,
                ["quattro"]       = 4,
                ["cinque"]        = 5,
                ["sei"]           = 6,
                ["sette"]         = 7,
                ["otto"]          = 8,
                ["nove"]          = 9,
                ["dieci"]         = 10,
                ["undici"]        = 11,
                ["dodici"]        = 12,
                ["tredici"]       = 13,
                ["quattordici"]   = 14,
                ["quindici"]      = 15,
                ["sedici"]        = 16,
                ["diciassette"]   = 17,
                ["diciotto"]      = 18,
                ["diciannove"]    = 19,
                ["venti"]         = 20,
                ["ventuno"]       = 21,
                ["ventidue"]      = 22,
                ["ventitre"]      = 23,
                ["ventitré"]      = 23,
                ["ventiquattro"]  = 24,

                // numeri in inglese (l'utente puo' scrivere in entrambe le lingue)
                ["one"]           = 1,
                ["an"]            = 1,
                ["two"]           = 2,
                ["three"]         = 3,
                ["four"]          = 4,
                ["five"]          = 5,
                ["six"]           = 6,
                ["seven"]         = 7,
                ["eight"]         = 8,
                ["nine"]          = 9,
                ["ten"]           = 10,
                ["eleven"]        = 11,
                ["twelve"]        = 12,
                ["thirteen"]      = 13,
                ["fourteen"]      = 14,
                ["fifteen"]       = 15,
                ["sixteen"]       = 16,
                ["seventeen"]     = 17,
                ["eighteen"]      = 18,
                ["nineteen"]      = 19,
                ["twenty"]        = 20,
                ["twenty-one"]    = 21,
                ["twenty-two"]    = 22,
                ["twenty-three"]  = 23,
                ["twenty-four"]   = 24,
            };

        /// <summary>
        /// Prova a convertire <paramref name="value"/> in un TimeSpan.
        /// Accetta formato "HH:mm" oppure numero in lettere italiane (es. "otto").
        /// </summary>
        public static bool TryParse(string value, out TimeSpan result)
        {
            result = TimeSpan.Zero;
            if (string.IsNullOrWhiteSpace(value)) return false;

            var trimmed = value.Trim();

            // Formato HH:mm
            if (TimeSpan.TryParseExact(trimmed, @"h\:mm", null, out result)) return true;
            if (TimeSpan.TryParseExact(trimmed, @"hh\:mm", null, out result)) return true;

            // Numero in lettere
            if (_wordsToHours.TryGetValue(trimmed, out var hours))
            {
                result = TimeSpan.FromHours(hours);
                return true;
            }

            return TryParseDuration(trimmed, out result);
        }

        // "2 ore", "due ore", "un'ora e mezza", "mezz'ora", "2 ore e 30", "2,5 ore", "8h", "4 hours", "half an hour"
        private static bool TryParseDuration(string value, out TimeSpan result)
        {
            result = TimeSpan.Zero;
            var t = value.ToLowerInvariant().Replace('\u2019', '\'').Trim().TrimEnd('.', ',', ';', '!', '?').Trim();

            if (t is "mezz'ora" or "mezzora" or "mezza ora" or "half an hour" or "half hour")
            {
                result = TimeSpan.FromMinutes(30);
                return true;
            }

            // "un'ora", "una ora", "an hour" -> "1 ora"
            t = Regex.Replace(t, @"^(un'|un |una |an |one )(?=ora|hour)", "1 ");

            var m = Regex.Match(t,
                @"^(?<h>\d{1,2}(?:[.,]\d{1,2})?|\p{L}+)\s*(?:ore|ora|h|hours?|hrs?)?" +
                @"(?:\s+(?:e|and)\s+(?<f>mezza|mezzo|un quarto|tre quarti|a half|\d{1,2})(?:\s*(?:minuti|min|minutes))?)?$");
            if (!m.Success) return false;

            double h;
            var hText = m.Groups["h"].Value;
            if (double.TryParse(hText.Replace(',', '.'), NumberStyles.Float, CultureInfo.InvariantCulture, out var hNum))
                h = hNum;
            else if (_wordsToHours.TryGetValue(hText, out var hWord))
                h = hWord;
            else
                return false;

            int minutes = 0;
            if (m.Groups["f"].Success)
            {
                var f = m.Groups["f"].Value;
                if (!_fractions.TryGetValue(f, out minutes) && !int.TryParse(f, out minutes)) return false;
                if (minutes > 59) return false;
            }

            result = TimeSpan.FromHours(h) + TimeSpan.FromMinutes(minutes);
            return result > TimeSpan.Zero && result <= TimeSpan.FromHours(24);
        }
    }
}
