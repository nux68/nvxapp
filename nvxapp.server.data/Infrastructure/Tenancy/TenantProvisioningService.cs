using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;

namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Creazione e aggiornamento degli schemi delle aziende.
        - modalita' singola: le tabelle tenant stanno in public, un solo "tenant" da migrare
        - multi-tenant: uno schema per azienda (Company.Schema), creato se manca e migrato
     Ogni schema ha il proprio storico (__EFMigrationsHistory_Tenant), quindi sa da solo
     quali migration gli mancano.
    */
    public interface ITenantProvisioningService
    {
        /// <summary>Crea (se manca) e migra lo schema di un'azienda. In modalita' singola non fa nulla.</summary>
        Task EnsureTenantAsync(string schema, CancellationToken cancellationToken = default);

        /// <summary>Migra le tabelle tenant: "public" in modalita' singola, ogni schema azienda in multi-tenant.</summary>
        Task MigrateAllTenantsAsync(CancellationToken cancellationToken = default);
    }

    public sealed class TenantProvisioningService : ITenantProvisioningService
    {
        private readonly PublicDbContext _publicDbContext;
        private readonly ITenantDbContextFactory _tenantDbContextFactory;
        private readonly TenancySettings _tenancySettings;
        private readonly ILogger<TenantProvisioningService> _logger;

        public TenantProvisioningService(PublicDbContext publicDbContext,
                                         ITenantDbContextFactory tenantDbContextFactory,
                                         TenancySettings tenancySettings,
                                         ILogger<TenantProvisioningService> logger)
        {
            _publicDbContext = publicDbContext;
            _tenantDbContextFactory = tenantDbContextFactory;
            _tenancySettings = tenancySettings;
            _logger = logger;
        }

        public async Task EnsureTenantAsync(string schema, CancellationToken cancellationToken = default)
        {
            if (!_tenancySettings.MultiTenant)
                return;

            await MigrateSchemaAsync(TenantSchemaName.Validate(schema), createSchema: true, cancellationToken);
        }

        public async Task MigrateAllTenantsAsync(CancellationToken cancellationToken = default)
        {
            if (!_tenancySettings.MultiTenant)
            {
                await MigrateSchemaAsync(TenantSchemaName.Public, createSchema: false, cancellationToken);
                return;
            }

            var schemas = await _publicDbContext.Company
                                                .Where(x => x.Schema != null)
                                                .Select(x => x.Schema!)
                                                .Distinct()
                                                .ToListAsync(cancellationToken);

            _logger.LogInformation("[Tenancy] Migrazione di {Count} schemi azienda.", schemas.Count);

            var failed = new List<string>();
            foreach (var schema in schemas)
            {
                try
                {
                    await MigrateSchemaAsync(TenantSchemaName.Validate(schema), createSchema: true, cancellationToken);
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

        private async Task MigrateSchemaAsync(string schema, bool createSchema, CancellationToken cancellationToken)
        {
            await using var context = _tenantDbContextFactory.Create(schema);

            if (createSchema)
            {
                // un identificatore non puo' essere un parametro SQL: il nome e' validato e quotato da TenantSchemaName
                string createSchemaSql = "CREATE SCHEMA IF NOT EXISTS " + TenantSchemaName.Quote(schema);
                await context.Database.ExecuteSqlRawAsync(createSchemaSql, cancellationToken);
            }

            var pending = (await context.Database.GetPendingMigrationsAsync(cancellationToken)).ToList();
            if (pending.Count == 0)
                return;

            _logger.LogInformation("[Tenancy] Schema {Schema}: applico {Count} migration ({Migrations}).",
                                   schema, pending.Count, string.Join(", ", pending));

            await context.Database.MigrateAsync(cancellationToken);
        }
    }
}
