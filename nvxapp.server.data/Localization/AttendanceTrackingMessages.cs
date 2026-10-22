namespace nvxapp.server.data.Localization
{
    /// <summary>
    /// Testi del server di AttendanceTracking (gestione presenze) nella lingua dell'utente
    /// (Localization/AttendanceTrackingMessages*.resx: italiano di riferimento + inglese).
    /// File separato da ServerText.cs per non toccare i file di Infrastructure nei merge.
    /// </summary>
    public static class AttendanceTrackingMessages
    {
        private static readonly ServerText _text =
            new("nvxapp.server.data.Localization.AttendanceTrackingMessages", typeof(AttendanceTrackingMessages));

        public static string Get(string key, params object?[] args) => _text.Get(key, args);
    }
}
