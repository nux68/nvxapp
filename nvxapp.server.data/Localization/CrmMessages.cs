namespace nvxapp.server.data.Localization
{
    /// <summary>
    /// Testi del server del CRM nella lingua dell'utente
    /// (Localization/CrmMessages*.resx: italiano di riferimento + inglese).
    /// File separato da ServerText.cs per non toccare i file di Infrastructure nei merge.
    /// </summary>
    public static class CrmMessages
    {
        private static readonly ServerText _text =
            new("nvxapp.server.data.Localization.CrmMessages", typeof(CrmMessages));

        public static string Get(string key, params object?[] args) => _text.Get(key, args);
    }
}
