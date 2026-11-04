using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    // Factory usata SOLO da dotnet ef per il CRM (vedi Note/Migrazioni.txt).
    // Il modello non contiene lo schema: per generare le migration basta "public".
    public class CrmDbContextDesignTimeFactory : IDesignTimeDbContextFactory<CrmDbContext>
    {
        public CrmDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<CrmDbContext>();
            DataLayerInstaller.ConfigureApplication(options, DesignTimeConfiguration.ConnectionString(), ApplicationType.Crm);
            return new CrmDbContext(options.Options, new FixedTenantSchemaAccessor(TenantSchemaName.Public));
        }
    }
}
