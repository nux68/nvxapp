namespace nvxapp.server.data.Localization
{
    /// <summary>
    /// Testi del server di Risorse Umane nella lingua dell'utente
    /// (Localization/HumanResourcesMessages*.resx: italiano di riferimento + inglese).
    /// File separato da ServerText.cs per non toccare i file di Infrastructure nei merge.
    /// </summary>
    public static class HumanResourcesMessages
    {
        private static readonly ServerText _text =
            new("nvxapp.server.data.Localization.HumanResourcesMessages", typeof(HumanResourcesMessages));

        public static string Get(string key, params object?[] args) => _text.Get(key, args);
    }
}
