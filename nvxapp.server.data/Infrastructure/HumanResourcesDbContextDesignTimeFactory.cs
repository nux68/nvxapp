using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    // Factory usata SOLO da dotnet ef per Risorse Umane (vedi Note/Migrazioni.txt).
    // File del ramo HumanResources (non tocca DesignTimeDbContextFactories.cs di Infrastructure), come per il CRM.
    // Il modello non contiene lo schema: per generare le migration basta "public".
    public class HumanResourcesDbContextDesignTimeFactory : IDesignTimeDbContextFactory<HumanResourcesDbContext>
    {
        public HumanResourcesDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<HumanResourcesDbContext>();
            DataLayerInstaller.ConfigureApplication(options, DesignTimeConfiguration.ConnectionString(), ApplicationType.HumanResources);
            return new HumanResourcesDbContext(options.Options, new FixedTenantSchemaAccessor(TenantSchemaName.Public));
        }
    }
}
