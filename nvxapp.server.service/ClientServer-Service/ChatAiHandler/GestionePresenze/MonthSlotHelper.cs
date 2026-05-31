namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Converte una stringa mese in un (Anno, Mese).
    // Formati accettati:
    //   MM/yyyy       (es. "07/2025")
    //   nome mese     (es. "luglio")
    //   nome mese anno (es. "luglio 2025")
    // Se l'anno non è specificato viene usato l'anno corrente.
    public static class MonthSlotHelper
    {
        private static readonly Dictionary<string, int> _monthNames =
            new(StringComparer.OrdinalIgnoreCase)
            {
                ["gennaio"]   = 1,
                ["febbraio"]  = 2,
                ["marzo"]     = 3,
                ["aprile"]    = 4,
                ["maggio"]    = 5,
                ["giugno"]    = 6,
                ["luglio"]    = 7,
                ["agosto"]    = 8,
                ["settembre"] = 9,
                ["ottobre"]   = 10,
                ["novembre"]  = 11,
                ["dicembre"]  = 12,
            };

        /// <summary>
        /// Prova a convertire <paramref name="value"/> in un (Year, Month).
        /// Restituisce <c>true</c> e valorizza <paramref name="result"/> in caso di successo.
        /// </summary>
        public static bool TryParse(string value, out (int Year, int Month) result)
        {
            result = (DateTime.Today.Year, DateTime.Today.Month);
            if (string.IsNullOrWhiteSpace(value)) return false;

            var trimmed = value.Trim();

            // Formato MM/yyyy
            if (trimmed.Length == 7 && trimmed[2] == '/')
            {
                if (int.TryParse(trimmed[..2], out var m) && int.TryParse(trimmed[3..], out var y)
                    && m >= 1 && m <= 12 && y >= 2000)
                {
                    result = (y, m);
                    return true;
                }
            }

            // "nome" oppure "nome anno"
            var parts = trimmed.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length >= 1 && _monthNames.TryGetValue(parts[0], out var month))
            {
                int year = DateTime.Today.Year;
                if (parts.Length >= 2 && int.TryParse(parts[1], out var parsedYear) && parsedYear >= 2000)
                    year = parsedYear;

                result = (year, month);
                return true;
            }

            return false;
        }
    }
}
