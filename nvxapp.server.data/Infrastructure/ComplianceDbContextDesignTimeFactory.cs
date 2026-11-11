using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    // Factory usata SOLO da dotnet ef per Compliance (vedi Note/Migrazioni.txt).
    // File del ramo Compliance (non tocca DesignTimeDbContextFactories.cs di Infrastructure), come per il CRM.
    // Il modello non contiene lo schema: per generare le migration basta "public".
    public class ComplianceDbContextDesignTimeFactory : IDesignTimeDbContextFactory<ComplianceDbContext>
    {
        public ComplianceDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<ComplianceDbContext>();
            DataLayerInstaller.ConfigureApplication(options, DesignTimeConfiguration.ConnectionString(), ApplicationType.Compliance);
            return new ComplianceDbContext(options.Options, new FixedTenantSchemaAccessor(TenantSchemaName.Public));
        }
    }
}
