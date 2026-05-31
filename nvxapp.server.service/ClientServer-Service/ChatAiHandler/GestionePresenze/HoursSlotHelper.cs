namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Converte una stringa ore in TimeSpan.
    // Formati accettati:
    //   HH:mm        (es. "08:00", "04:30")
    //   numero intero scritto in lettere IT (es. "uno", "due", ..., "ventiquattro")
    public static class HoursSlotHelper
    {
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

            return false;
        }
    }
}
