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

        // "oggi" (anche valore di default degli slot data) oppure "today"
        public static bool IsToday(string? value)
        {
            var v = (value ?? string.Empty).Trim();
            return v.Equals("oggi", StringComparison.OrdinalIgnoreCase) ||
                   v.Equals("today", StringComparison.OrdinalIgnoreCase);
        }
    }
}
