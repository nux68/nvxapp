namespace nvxapp.server.data.Localization
{
    /// <summary>
    /// Testi del server del documentale nella lingua dell'utente
    /// (Localization/DocumentManagementMessages*.resx: italiano di riferimento + inglese).
    /// File separato da ServerText.cs per non toccare i file di Infrastructure nei merge.
    /// </summary>
    public static class DocumentManagementMessages
    {
        private static readonly ServerText _text =
            new("nvxapp.server.data.Localization.DocumentManagementMessages", typeof(DocumentManagementMessages));

        public static string Get(string key, params object?[] args) => _text.Get(key, args);
    }
}
