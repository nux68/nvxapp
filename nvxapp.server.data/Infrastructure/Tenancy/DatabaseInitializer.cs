using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Infrastructure;
using Microsoft.EntityFrameworkCore.Metadata;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Eseguito all'avvio dell'applicazione, prima di accettare richieste:
        0. controllo dei modelli: nessun nome di tabella in due contesti (in modalita' singola
           tutti gli applicativi stanno in public insieme alle tabelle comuni)
        1. migra le tabelle condivise (PublicDbContext, schema public)
        2. modalita' multi-tenant:
             - primo avvio: salva in AppSetting il valore di configurazione (DbParameter:MultiTenant)
             - avvii successivi: il valore di configurazione deve coincidere con quello salvato,
               altrimenti l'avvio viene interrotto (la modalita' non si cambia dopo il primo avvio)
        3. migra le tabelle degli applicativi (public oppure gli schemi azienda/applicativo attivi)
        4. (solo fase beta) crea, se mancanti, Dealer e FinancialAdvisor 'DEMO'
    */
    public static class DatabaseInitializer
    {
        public static async Task InitializeAsync(IServiceProvider services, bool configuredMultiTenant, CancellationToken cancellationToken = default)
        {
            using var scope = services.CreateScope();
            var sp = scope.ServiceProvider;

            var logger = sp.GetRequiredService<ILoggerFactory>().CreateLogger(typeof(DatabaseInitializer).FullName!);
            var publicDbContext = sp.GetRequiredService<PublicDbContext>();
            var tenancySettings = sp.GetRequiredService<TenancySettings>();

            // 0. nomi di tabella univoci tra i contesti
            CheckTableNames(sp, publicDbContext);

            // 1. tabelle condivise
            await publicDbContext.Database.MigrateAsync(cancellationToken);

            // 2. modalita' multi-tenant
            var multiTenant = await ResolveTenancyModeAsync(publicDbContext, configuredMultiTenant, logger, cancellationToken);
            tenancySettings.Initialize(multiTenant);

            // 3. tabelle degli applicativi
            await sp.GetRequiredService<ITenantProvisioningService>().MigrateAllAsync(cancellationToken);

            // 4. dati demo (solo fase beta)
            await SeedDemoAsync(publicDbContext, logger, cancellationToken);
        }

        /*
         FASE BETA: crea (se mancanti) il Dealer 'DEMO' e il FinancialAdvisor 'DEMO' collegato.
         Da rimuovere al termine della fase beta.
        */
        public const string DemoDescrizione = "DEMO";

        private static async Task SeedDemoAsync(PublicDbContext publicDbContext, ILogger logger, CancellationToken cancellationToken)
        {
            var now = DateTime.Now;

            var dealer = await publicDbContext.Dealer
                                              .AsTracking()
                                              .FirstOrDefaultAsync(x => x.Descrizione == DemoDescrizione, cancellationToken);
            if (dealer == null)
            {
                dealer = new Dealer
                {
                    Descrizione = DemoDescrizione,
                    CreationDate = now,
                    ModifiedDate = now
                };
                publicDbContext.Dealer.Add(dealer);
                await publicDbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("[Init] Creato Dealer '{Descrizione}'.", DemoDescrizione);
            }

            var exists = await publicDbContext.FinancialAdvisor
                                              .AnyAsync(x => x.IdDealer == dealer.Id && x.Descrizione == DemoDescrizione, cancellationToken);
            if (!exists)
            {
                publicDbContext.FinancialAdvisor.Add(new FinancialAdvisor
                {
                    IdDealer = dealer.Id,
                    Descrizione = DemoDescrizione,
                    CreationDate = now,
                    ModifiedDate = now
                });
                await publicDbContext.SaveChangesAsync(cancellationToken);

                logger.LogInformation("[Init] Creato FinancialAdvisor '{Descrizione}'.", DemoDescrizione);
            }
        }

        private static void CheckTableNames(IServiceProvider sp, PublicDbContext publicDbContext)
        {
            var owners = new Dictionary<string, List<string>>(StringComparer.OrdinalIgnoreCase);
            void Add(string? table, string owner)
            {
                if (string.IsNullOrEmpty(table)) return;
                if (!owners.TryGetValue(table, out var list)) owners[table] = list = new List<string>();
                if (!list.Contains(owner)) list.Add(owner);
            }

            // modello completo (design-time): contiene anche l'esclusione dalle migration
            foreach (var entity in publicDbContext.GetService<IDesignTimeModel>().Model.GetEntityTypes())
                Add(entity.GetTableName(), nameof(PublicDbContext));

            var factory = sp.GetRequiredService<IApplicationDbContextFactory>();
            foreach (var application in sp.GetRequiredService<ApplicationDbContextRegistry>().Contexts.Keys)
            {
                // solo il modello: nessuna connessione aperta
                using var context = factory.Create(application, TenantSchemaName.Public);
                foreach (var entity in context.GetService<IDesignTimeModel>().Model.GetEntityTypes())
                {
                    // tabelle di public referenziate in sola lettura: non appartengono all'applicativo
                    if (entity.IsTableExcludedFromMigrations()) continue;
                    Add(entity.GetTableName(), context.GetType().Name);
                }
            }

            var duplicates = owners.Where(x => x.Value.Count > 1)
                                   .Select(x => $"{x.Key} ({string.Join(", ", x.Value)})")
                                   .ToList();
            if (duplicates.Count > 0)
                throw new InvalidOperationException(
                    "Tabelle con lo stesso nome in piu' contesti (non ammesso: in modalita' singola stanno tutte in public): "
                    + string.Join("; ", duplicates));
        }

        private static async Task<bool> ResolveTenancyModeAsync(PublicDbContext publicDbContext,
                                                                bool configuredMultiTenant,
                                                                ILogger logger,
                                                                CancellationToken cancellationToken)
        {
            var configuredMode = TenancySettings.ToMode(configuredMultiTenant);

            var setting = await publicDbContext.AppSetting
                                               .AsTracking()
                                               .FirstOrDefaultAsync(x => x.Key == AppSetting.Key_TenancyMode, cancellationToken);

            if (setting == null)
            {
                publicDbContext.AppSetting.Add(new AppSetting
                {
                    Key = AppSetting.Key_TenancyMode,
                    Value = configuredMode,
                    ModifiedDate = DateTime.Now
                });
                await publicDbContext.SaveChangesAsync(cancellationToken);

                logger.LogWarning("[Tenancy] Primo avvio: modalita' database impostata a '{Mode}'. Non potra' piu' essere cambiata.", configuredMode);
                return configuredMultiTenant;
            }

            if (setting.Value != configuredMode)
                throw new InvalidOperationException(
                    $"Il database e' in modalita' '{setting.Value}' ma la configurazione (DbParameter:MultiTenant) indica '{configuredMode}'. " +
                    "La modalita' multi-tenant si decide al primo avvio e non puo' essere cambiata: correggere la configurazione.");

            logger.LogInformation("[Tenancy] Modalita' database: '{Mode}'.", setting.Value);
            return setting.Value == TenancySettings.Mode_Multi;
        }
    }
}
