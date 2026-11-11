using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    // Factory usata SOLO da dotnet ef per Risorse aziendali (vedi Note/Migrazioni.txt).
    // File del ramo Assets (non tocca DesignTimeDbContextFactories.cs di Infrastructure), come per il CRM.
    // Il modello non contiene lo schema: per generare le migration basta "public".
    public class AssetsDbContextDesignTimeFactory : IDesignTimeDbContextFactory<AssetsDbContext>
    {
        public AssetsDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<AssetsDbContext>();
            DataLayerInstaller.ConfigureApplication(options, DesignTimeConfiguration.ConnectionString(), ApplicationType.Assets);
            return new AssetsDbContext(options.Options, new FixedTenantSchemaAccessor(TenantSchemaName.Public));
        }
    }
}
