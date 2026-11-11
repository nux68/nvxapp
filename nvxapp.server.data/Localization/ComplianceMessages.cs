namespace nvxapp.server.data.Localization
{
    /// <summary>
    /// Testi del server di Compliance nella lingua dell'utente
    /// (Localization/ComplianceMessages*.resx: italiano di riferimento + inglese).
    /// File separato da ServerText.cs per non toccare i file di Infrastructure nei merge.
    /// </summary>
    public static class ComplianceMessages
    {
        private static readonly ServerText _text =
            new("nvxapp.server.data.Localization.ComplianceMessages", typeof(ComplianceMessages));

        public static string Get(string key, params object?[] args) => _text.Get(key, args);
    }
}
