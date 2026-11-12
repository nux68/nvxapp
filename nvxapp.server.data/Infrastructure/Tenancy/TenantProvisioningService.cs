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
        - Common (dati comuni, tenant_<IdAzienda>_0): schema creato per OGNI azienda e migrato
          sempre per primo, perche' gli schemi degli applicativi hanno chiavi esterne verso le sue tabelle
     Ogni schema ha il proprio storico (__EFMigrationsHistory_<Applicativo>), quindi sa da solo
     quali migration gli mancano.
    */
    public interface ITenantProvisioningService
    {
        /// <summary>
        /// Crea (se manca) e migra lo schema di un applicativo di un'azienda; per un applicativo diverso da Common
        /// prima crea/migra anche lo schema Common dell'azienda. In modalita' singola non fa nulla.
        /// </summary>
        Task EnsureApplicationAsync(int idCompany, ApplicationType application, CancellationToken cancellationToken = default);

        /// <summary>
        /// Migra le tabelle degli applicativi: public in modalita' singola (Common per primo), in multi-tenant lo schema
        /// Common di ogni azienda e poi ogni schema azienda/applicativo attivo.
        /// </summary>
        Task MigrateAllAsync(CancellationToken cancellationToken = default);

        /// <summary>Schemi esistenti degli applicativi di un'azienda (multi-tenant; vuoto in modalita' singola), anche di applicativi disattivati.</summary>
        Task<List<string>> CompanySchemasAsync(int idCompany, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina gli schemi degli applicativi di un'azienda con tutti i dati (DROP SCHEMA ... CASCADE), per la cancellazione
        /// dell'azienda. Usa la connessione di PublicDbContext: dentro una sua transazione si annulla con il rollback.
        /// In modalita' singola non fa nulla.
        /// </summary>
        Task<List<string>> DropCompanySchemasAsync(int idCompany, CancellationToken cancellationToken = default);

        /// <summary>
        /// Elimina lo schema di un solo applicativo di un'azienda con tutti i dati (DROP SCHEMA ... CASCADE), per la disdetta
        /// della licenza (ANALISI_INFRASTRUCTURE_LICENZE.md §15.9). Mai Common. Usa la connessione di PublicDbContext: dentro una
        /// sua transazione si annulla con il rollback. Restituisce lo schema eliminato, null se non esisteva o in modalita' singola.
        /// </summary>
        Task<string?> DropApplicationSchemaAsync(int idCompany, ApplicationType application, CancellationToken cancellationToken = default);
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

            // le tabelle dell'applicativo hanno chiavi esterne verso quelle comuni: Common per primo
            if (application.IsActivatable())
                await EnsureApplicationAsync(idCompany, ApplicationType.Common, cancellationToken);

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
                // Common (0) per primo: gli altri contesti hanno chiavi esterne verso le sue tabelle
                foreach (var application in _registry.Contexts.Keys.OrderBy(x => (int)x))
                    await MigrateSchemaAsync(application, TenantSchemaName.Public, createSchema: false, cancellationToken);
                return;
            }

            // schema Common per tutte le aziende, prima degli applicativi
            var companies = await _publicDbContext.Company.Select(x => x.Id).OrderBy(x => x).ToListAsync(cancellationToken);
            var actives = companies.Select(x => new { IdCompany = x, ApplicationType = ApplicationType.Common }).ToList();

            actives.AddRange(await _publicDbContext.CompanyApplication
                                                   .Where(x => x.Active && x.ApplicationType != ApplicationType.Common)
                                                   .OrderBy(x => x.IdCompany).ThenBy(x => x.ApplicationType)
                                                   .Select(x => new { x.IdCompany, x.ApplicationType })
                                                   .ToListAsync(cancellationToken));

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

        public async Task<List<string>> CompanySchemasAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            if (!_tenancySettings.MultiTenant)
                return new List<string>();

            // solo i nomi esatti tenant_<IdAzienda>_<numero> (in LIKE "_" vale un carattere qualsiasi: si filtra dopo)
            var prefix = $"tenant_{idCompany}_";
            var schemas = await _publicDbContext.Database
                .SqlQueryRaw<string>("SELECT nspname::text AS \"Value\" FROM pg_namespace WHERE nspname LIKE {0}", prefix + "%")
                .ToListAsync(cancellationToken);

            return schemas.Where(x => x.Length > prefix.Length && x[prefix.Length..].All(char.IsDigit))
                          .OrderBy(x => x)
                          .ToList();
        }

        public async Task<List<string>> DropCompanySchemasAsync(int idCompany, CancellationToken cancellationToken = default)
        {
            var schemas = await CompanySchemasAsync(idCompany, cancellationToken);
            foreach (var schema in schemas)
            {
                // un identificatore non puo' essere un parametro SQL: il nome e' validato e quotato da TenantSchemaName
                string dropSchemaSql = "DROP SCHEMA IF EXISTS " + TenantSchemaName.Quote(schema) + " CASCADE";
                await _publicDbContext.Database.ExecuteSqlRawAsync(dropSchemaSql, cancellationToken);
                _logger.LogInformation("[Tenancy] Schema {Schema} eliminato (azienda {IdCompany}).", schema, idCompany);
            }
            return schemas;
        }

        public async Task<string?> DropApplicationSchemaAsync(int idCompany, ApplicationType application, CancellationToken cancellationToken = default)
        {
            if (application == ApplicationType.Common)
                throw new InvalidOperationException("Lo schema Common non si elimina con la disdetta di una licenza.");

            var schema = TenantSchemaName.For(idCompany, application);
            if (!(await CompanySchemasAsync(idCompany, cancellationToken)).Contains(schema))
                return null;

            // un identificatore non puo' essere un parametro SQL: il nome e' validato e quotato da TenantSchemaName
            string dropSchemaSql = "DROP SCHEMA IF EXISTS " + TenantSchemaName.Quote(schema) + " CASCADE";
            await _publicDbContext.Database.ExecuteSqlRawAsync(dropSchemaSql, cancellationToken);
            _logger.LogWarning("[Tenancy] Schema {Schema} eliminato per la disdetta della licenza (azienda {IdCompany}, {Application}).",
                               schema, idCompany, application);
            return schema;
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
