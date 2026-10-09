using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Design;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Infrastructure.Tenancy;

namespace nvxapp.server.data.Infrastructure
{
    /*
     Factory usate SOLO dagli strumenti dotnet ef (migrations add / database update).
     Vedi nvxapp.server/Note/Migrazioni.txt per i comandi.
    */
    internal static class DesignTimeConfiguration
    {
        public static string ConnectionString()
        {
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .Build();

            return configuration.GetConnectionString("nvxappDbContext")
                   ?? throw new InvalidOperationException("Connection string 'nvxappDbContext' non trovata in appsettings.json.");
        }
    }

    public class PublicDbContextDesignTimeFactory : IDesignTimeDbContextFactory<PublicDbContext>
    {
        public PublicDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<PublicDbContext>();
            DataLayerInstaller.ConfigurePublic(options, DesignTimeConfiguration.ConnectionString());
            return new PublicDbContext(options.Options);
        }
    }

    public class TenantDbContextDesignTimeFactory : IDesignTimeDbContextFactory<TenantDbContext>
    {
        public TenantDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<TenantDbContext>();
            DataLayerInstaller.ConfigureTenant(options, DesignTimeConfiguration.ConnectionString());
            // il modello tenant non contiene lo schema: per generare le migration basta "public"
            return new TenantDbContext(options.Options, new FixedTenantSchemaAccessor(TenantSchemaName.Public));
        }
    }
}
