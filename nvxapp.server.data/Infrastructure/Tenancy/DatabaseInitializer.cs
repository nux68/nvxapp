using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Eseguito all'avvio dell'applicazione, prima di accettare richieste:
        1. migra le tabelle condivise (PublicDbContext, schema public)
        2. modalita' multi-tenant:
             - primo avvio: salva in AppSetting il valore di configurazione (DbParameter:MultiTenant)
             - avvii successivi: il valore di configurazione deve coincidere con quello salvato,
               altrimenti l'avvio viene interrotto (la modalita' non si cambia dopo il primo avvio)
        3. migra le tabelle tenant (public oppure tutti gli schemi azienda)
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

            // 1. tabelle condivise
            await publicDbContext.Database.MigrateAsync(cancellationToken);

            // 2. modalita' multi-tenant
            var multiTenant = await ResolveTenancyModeAsync(publicDbContext, configuredMultiTenant, logger, cancellationToken);
            tenancySettings.Initialize(multiTenant);

            // 3. tabelle tenant
            await sp.GetRequiredService<ITenantProvisioningService>().MigrateAllTenantsAsync(cancellationToken);
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
