using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Creazione e aggiornamento delle tabelle degli applicativi.
        - modalita' singola: tutti gli applicativi in public, migrati all'avvio
        - multi-tenant: uno schema per azienda e applicativo ATTIVO (tenant_<IdAzienda>_<IdApp>),
          creato se manca e migrato; gli applicativi non attivi non hanno schema (o lo
          conservano, se disattivati dopo l'uso)
     Ogni schema ha il proprio storico (__EFMigrationsHistory_<Applicativo>), quindi sa da solo
     quali migration gli mancano.
    */
    public interface ITenantProvisioningService
    {
        /// <summary>Crea (se manca) e migra lo schema di un applicativo di un'azienda. In modalita' singola non fa nulla.</summary>
        Task EnsureApplicationAsync(int idCompany, ApplicationType application, CancellationToken cancellationToken = default);

        /// <summary>Migra le tabelle degli applicativi: public in modalita' singola, ogni schema azienda/applicativo attivo in multi-tenant.</summary>
        Task MigrateAllAsync(CancellationToken cancellationToken = default);
    }

    public sealed class TenantProvisioningService : ITenantProvisioningService
    {
        private readonly PublicDbContext _publicDbContext;
        private readonly IApplicationDbContextFactory _applicationDbContextFactory;
        private readonly ApplicationDbContextRegistry _registry;
        private readonly TenancySettings _tenancySettings;
        private readonly ILogger<TenantProvisioningService> _logger;

        public TenantProvisioningService(PublicDbContext publicDbContext,
                                         IApplicationDbContextFactory applicationDbContextFactory,
                                         ApplicationDbContextRegistry registry,
                                         TenancySettings tenancySettings,
                                         ILogger<TenantProvisioningService> logger)
        {
            _publicDbContext = publicDbContext;
            _applicationDbContextFactory = applicationDbContextFactory;
            _registry = registry;
            _tenancySettings = tenancySettings;
            _logger = logger;
        }

        public async Task EnsureApplicationAsync(int idCompany, ApplicationType application, CancellationToken cancellationToken = default)
        {
            if (!_tenancySettings.MultiTenant)
                return;

            if (!_registry.IsRegistered(application))
            {
                _logger.LogWarning("[Tenancy] Applicativo {Application} senza contesto registrato: schema non creato per l'azienda {IdCompany}.",
                                   application, idCompany);
                return;
            }

            await MigrateSchemaAsync(application, TenantSchemaName.For(idCompany, application), createSchema: true, cancellationToken);
        }

        public async Task MigrateAllAsync(CancellationToken cancellationToken = default)
        {
            if (!_tenancySettings.MultiTenant)
            {
                foreach (var application in _registry.Contexts.Keys)
                    await MigrateSchemaAsync(application, TenantSchemaName.Public, createSchema: false, cancellationToken);
                return;
            }

            var actives = await _publicDbContext.CompanyApplication
                                                .Where(x => x.Active)
                                                .Select(x => new { x.IdCompany, x.ApplicationType })
                                                .ToListAsync(cancellationToken);

            var skipped = actives.Where(x => !_registry.IsRegistered(x.ApplicationType)).Select(x => x.ApplicationType).Distinct().ToList();
            if (skipped.Count > 0)
                _logger.LogInformation("[Tenancy] Applicativi attivi senza contesto registrato (saltati): {Applications}", string.Join(", ", skipped));

            var targets = actives.Where(x => _registry.IsRegistered(x.ApplicationType)).ToList();
            _logger.LogInformation("[Tenancy] Migrazione di {Count} schemi azienda/applicativo.", targets.Count);

            var failed = new List<string>();
            foreach (var target in targets)
            {
                var schema = TenantSchemaName.For(target.IdCompany, target.ApplicationType);
                try
                {
                    await MigrateSchemaAsync(target.ApplicationType, schema, createSchema: true, cancellationToken);
                }
                catch (Exception ex)
                {
                    // un'azienda con problemi non deve bloccare le altre
                    failed.Add(schema);
                    _logger.LogError(ex, "[Tenancy] Migrazione fallita per lo schema {Schema}.", schema);
                }
            }

            if (failed.Count > 0)
                _logger.LogError("[Tenancy] Migrazione fallita per {Count} schemi: {Schemas}", failed.Count, string.Join(", ", failed));
            else
                _logger.LogInformation("[Tenancy] Migrazione schemi completata.");
        }

        private async Task MigrateSchemaAsync(ApplicationType application, string schema, bool createSchema, CancellationToken cancellationToken)
        {
            await using var context = _applicationDbContextFactory.Create(application, schema);

            if (createSchema)
            {
                // un identificatore non puo' essere un parametro SQL: il nome e' validato e quotato da TenantSchemaName
                string createSchemaSql = "CREATE SCHEMA IF NOT EXISTS " + TenantSchemaName.Quote(schema);
                await context.Database.ExecuteSqlRawAsync(createSchemaSql, cancellationToken);
            }

            var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
            if (pending.Count == 0)
                return;

            _logger.LogInformation("[Tenancy] {Application} - schema {Schema}: applico {Count} migration ({Migrations}).",
                                   application, schema, pending.Count, string.Join(", ", pending));

            await context.Database.MigrateAsync(cancellationToken);
        }
    }
}
