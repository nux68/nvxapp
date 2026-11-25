namespace nvxapp.server.service.Helpers
{
    /// <summary>
    /// Marca una proprieta' (o un campo) il cui valore non deve mai finire nel log: <see cref="LogSanitizer"/> lo scrive come "***",
    /// qualunque sia il nome e il tipo. Serve per i valori riservati che la regola sui nomi non riconosce.
    /// Non cambia la serializzazione delle risposte al client.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field, AllowMultiple = false, Inherited = true)]
    public sealed class LogSensitiveAttribute : Attribute
    {
    }
}
