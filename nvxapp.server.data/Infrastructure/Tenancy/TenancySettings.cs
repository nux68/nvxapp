namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Modalita' di funzionamento del database, decisa al PRIMO avvio e salvata nella tabella
     AppSetting (chiave TenancyMode). Agli avvii successivi il valore di configurazione
     (DbParameter:MultiTenant) deve coincidere, altrimenti l'applicazione non parte.

     Registrata come singleton e valorizzata una sola volta da DatabaseInitializer, prima che
     arrivino richieste: dopo l'avvio e' di sola lettura (non e' stato condiviso tra richieste).
    */
    public sealed class TenancySettings
    {
        public const string Mode_Single = "single";
        public const string Mode_Multi = "multi";

        private bool? _multiTenant;

        public bool IsInitialized => _multiTenant.HasValue;

        public bool MultiTenant =>
            _multiTenant ?? throw new InvalidOperationException("Modalita' multi-tenant non ancora inizializzata (DatabaseInitializer).");

        internal void Initialize(bool multiTenant)
        {
            if (_multiTenant.HasValue && _multiTenant.Value != multiTenant)
                throw new InvalidOperationException("La modalita' multi-tenant non puo' cambiare a runtime.");
            _multiTenant = multiTenant;
        }

        public static string ToMode(bool multiTenant) => multiTenant ? Mode_Multi : Mode_Single;
    }
}
