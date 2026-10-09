using Microsoft.AspNetCore.Http;

namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /// <summary>Fornisce lo schema dell'azienda su cui deve lavorare un TenantDbContext.</summary>
    public interface ITenantSchemaAccessor
    {
        string GetSchema();
    }


    /*
     Schema della richiesta corrente (registrato come scoped).
        - modalita' singola: sempre "public"
        - multi-tenant: schema impostato esplicitamente con TenantScope (job in background,
          inizializzatori) oppure claim "tenant" del token JWT della richiesta HTTP.
     Nessuna variabile statica condivisa: ogni richiesta risolve il proprio valore.
    */
    public sealed class TenantSchemaAccessor : ITenantSchemaAccessor
    {
        public const string TenantClaim = "tenant";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TenancySettings _tenancySettings;

        public TenantSchemaAccessor(IHttpContextAccessor httpContextAccessor, TenancySettings tenancySettings)
        {
            _httpContextAccessor = httpContextAccessor;
            _tenancySettings = tenancySettings;
        }

        public string GetSchema()
        {
            if (!_tenancySettings.MultiTenant)
                return TenantSchemaName.Public;

            var schema = TenantScope.CurrentSchema
                         ?? _httpContextAccessor.HttpContext?.User?.FindFirst(TenantClaim)?.Value;

            if (string.IsNullOrWhiteSpace(schema))
                throw new InvalidOperationException(
                    "Multi-tenant attivo: nessuna azienda selezionata per l'operazione corrente (claim 'tenant' assente).");

            return schema;
        }
    }


    /// <summary>Schema fisso: usato per migration, provisioning e accessi espliciti a un'azienda.</summary>
    public sealed class FixedTenantSchemaAccessor : ITenantSchemaAccessor
    {
        private readonly string _schema;

        public FixedTenantSchemaAccessor(string schema)
        {
            _schema = TenantSchemaName.Validate(schema);
        }

        public string GetSchema() => _schema;
    }


    /*
     Imposta lo schema per il flusso asincrono corrente (AsyncLocal: non e' condiviso tra
     richieste o thread diversi). Serve dove non c'e' una richiesta HTTP, es. RunInBackground.
        using (TenantScope.Use(schema)) { ... }
    */
    public static class TenantScope
    {
        private static readonly AsyncLocal<string?> _current = new AsyncLocal<string?>();

        public static string? CurrentSchema => _current.Value;

        public static IDisposable Use(string? schema)
        {
            var previous = _current.Value;
            _current.Value = string.IsNullOrWhiteSpace(schema) ? null : schema;
            return new Restore(previous);
        }

        private sealed class Restore : IDisposable
        {
            private readonly string? _previous;
            public Restore(string? previous) { _previous = previous; }
            public void Dispose() => _current.Value = _previous;
        }
    }
}
