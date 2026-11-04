using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    // Factory usata SOLO da dotnet ef per il documentale (vedi Note/Migrazioni.txt).
    // Il modello non contiene lo schema: per generare le migration basta "public".
    public class DocumentManagementDbContextDesignTimeFactory : IDesignTimeDbContextFactory<DocumentManagementDbContext>
    {
        public DocumentManagementDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<DocumentManagementDbContext>();
            DataLayerInstaller.ConfigureApplication(options, DesignTimeConfiguration.ConnectionString(), ApplicationType.DocumentManagement);
            return new DocumentManagementDbContext(options.Options, new FixedTenantSchemaAccessor(TenantSchemaName.Public));
        }
    }
}
