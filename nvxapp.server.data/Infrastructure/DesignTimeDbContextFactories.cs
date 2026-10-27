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
    public static class DesignTimeConfiguration
    {
        public static string ConnectionString()
        {
            // la connessione (con la password) e' un segreto: variabile d'ambiente NVXAPP_ConnectionStrings__nvxappDbContext,
            // come a runtime (vedi Installers.InstallSettings)
            var configuration = new ConfigurationBuilder()
                .SetBasePath(Directory.GetCurrentDirectory())
                .AddJsonFile("appsettings.json")
                .AddEnvironmentVariables("NVXAPP_")
                .Build();

            string? connectionString = configuration.GetConnectionString("nvxappDbContext");
            if (string.IsNullOrWhiteSpace(connectionString))
                throw new InvalidOperationException("Connection string 'nvxappDbContext' non configurata: impostare la variabile d'ambiente NVXAPP_ConnectionStrings__nvxappDbContext.");
            return connectionString;
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

    /*
     Contesti degli applicativi: il modello non contiene lo schema, per generare le migration
     basta "public". Ogni modulo aggiunge la factory del proprio contesto.
    */
    public class MokeDbContextDesignTimeFactory : IDesignTimeDbContextFactory<MokeDbContext>
    {
        public MokeDbContext CreateDbContext(string[] args)
        {
            var options = new DbContextOptionsBuilder<MokeDbContext>();
            DataLayerInstaller.ConfigureApplication(options, DesignTimeConfiguration.ConnectionString(), ApplicationType.Moke);
            return new MokeDbContext(options.Options, new FixedTenantSchemaAccessor(TenantSchemaName.Public));
        }
    }
}
