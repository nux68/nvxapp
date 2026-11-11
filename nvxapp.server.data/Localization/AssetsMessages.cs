namespace nvxapp.server.data.Localization
{
    /// <summary>
    /// Testi del server di Risorse aziendali nella lingua dell'utente
    /// (Localization/AssetsMessages*.resx: italiano di riferimento + inglese).
    /// File separato da ServerText.cs per non toccare i file di Infrastructure nei merge.
    /// </summary>
    public static class AssetsMessages
    {
        private static readonly ServerText _text =
            new("nvxapp.server.data.Localization.AssetsMessages", typeof(AssetsMessages));

        public static string Get(string key, params object?[] args) => _text.Get(key, args);
    }
}
