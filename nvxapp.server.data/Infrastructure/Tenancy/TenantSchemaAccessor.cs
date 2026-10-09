using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;

namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /// <summary>Fornisce lo schema su cui deve lavorare il contesto di un applicativo.</summary>
    public interface ITenantSchemaAccessor
    {
        string GetSchema(ApplicationType application);
    }


    /// <summary>L'applicativo non e' attivo per l'azienda (o nessuna azienda e' selezionata).</summary>
    public sealed class ApplicationNotActiveException : InvalidOperationException
    {
        public ApplicationNotActiveException(string message) : base(message) { }
    }


    /*
     Schema della richiesta corrente (registrato come scoped).
        - azienda: TenantScope (job in background, inizializzatori) oppure claim "company" del token
        - l'applicativo deve essere ATTIVO per l'azienda (CompanyApplication), in entrambe le modalita'
        - modalita' singola: "public"; multi-tenant: tenant_<IdAzienda>_<IdApplicativo>
     Nessuna variabile statica condivisa: ogni richiesta risolve il proprio valore.
    */
    public sealed class TenantSchemaAccessor : ITenantSchemaAccessor
    {
        public const string CompanyClaim = "company";

        private readonly IHttpContextAccessor _httpContextAccessor;
        private readonly TenancySettings _tenancySettings;
        private readonly PublicDbContext _publicDbContext;

        // applicativi attivi per azienda, letti una volta per richiesta
        private readonly Dictionary<int, HashSet<ApplicationType>> _activeApplications = new();

        public TenantSchemaAccessor(IHttpContextAccessor httpContextAccessor,
                                    TenancySettings tenancySettings,
                                    PublicDbContext publicDbContext)
        {
            _httpContextAccessor = httpContextAccessor;
            _tenancySettings = tenancySettings;
            _publicDbContext = publicDbContext;
        }

        public string GetSchema(ApplicationType application)
        {
            var idCompany = CurrentCompany()
                ?? throw new ApplicationNotActiveException(
                    $"Nessuna azienda selezionata per l'operazione corrente: impossibile accedere ai dati dell'applicativo {application}.");

            if (!ActiveApplications(idCompany).Contains(application))
                throw new ApplicationNotActiveException(
                    $"L'applicativo {application} non e' attivo per l'azienda {idCompany}.");

            return _tenancySettings.MultiTenant
                ? TenantSchemaName.For(idCompany, application)
                : TenantSchemaName.Public;
        }

        private int? CurrentCompany()
        {
            if (TenantScope.CurrentCompany.HasValue)
                return TenantScope.CurrentCompany;

            var claim = _httpContextAccessor.HttpContext?.User?.FindFirst(CompanyClaim)?.Value;
            return int.TryParse(claim, out var idCompany) && idCompany > 0 ? idCompany : null;
        }

        private HashSet<ApplicationType> ActiveApplications(int idCompany)
        {
            if (!_activeApplications.TryGetValue(idCompany, out var active))
            {
                active = _publicDbContext.CompanyApplication
                                         .Where(x => x.IdCompany == idCompany && x.Active)
                                         .Select(x => x.ApplicationType)
                                         .ToHashSet();
                _activeApplications[idCompany] = active;
            }
            return active;
        }
    }


    /// <summary>Schema fisso, senza controllo di attivazione: per migration, provisioning e accessi espliciti.</summary>
    public sealed class FixedTenantSchemaAccessor : ITenantSchemaAccessor
    {
        private readonly string _schema;

        public FixedTenantSchemaAccessor(string schema)
        {
            _schema = TenantSchemaName.Validate(schema);
        }

        public string GetSchema(ApplicationType application) => _schema;
    }


    /*
     Imposta l'azienda per il flusso asincrono corrente (AsyncLocal: non e' condiviso tra
     richieste o thread diversi). Serve dove non c'e' una richiesta HTTP (RunInBackground) o
     dove si lavora per un'azienda diversa da quella del token (inizializzatori).
        using (TenantScope.Use(idCompany)) { ... }
    */
    public static class TenantScope
    {
        private static readonly AsyncLocal<int?> _current = new AsyncLocal<int?>();

        public static int? CurrentCompany => _current.Value;

        public static IDisposable Use(int? idCompany)
        {
            var previous = _current.Value;
            _current.Value = idCompany.HasValue && idCompany.Value > 0 ? idCompany : null;
            return new Restore(previous);
        }

        private sealed class Restore : IDisposable
        {
            private readonly int? _previous;
            public Restore(int? previous) { _previous = previous; }
            public void Dispose() => _current.Value = _previous;
        }
    }
}
