using Microsoft.EntityFrameworkCore;

namespace nvxapp.server.data.Infrastructure.Tenancy
{
    /*
     Crea un TenantDbContext su uno schema ESPLICITO, indipendente dalla richiesta corrente.
     Da usare per: provisioning/migration delle aziende, inizializzatori di una nuova azienda,
     elaborazioni che operano su un'azienda diversa da quella del token.
     Il contesto restituito va chiuso dal chiamante (using).

     Per il normale accesso ai dati della richiesta si inietta direttamente TenantDbContext.
    */
    public interface ITenantDbContextFactory
    {
        TenantDbContext Create(string schema);
    }

    public sealed class TenantDbContextFactory : ITenantDbContextFactory
    {
        private readonly DbContextOptions<TenantDbContext> _options;

        public TenantDbContextFactory(DbContextOptions<TenantDbContext> options)
        {
            _options = options;
        }

        public TenantDbContext Create(string schema)
            => new TenantDbContext(_options, new FixedTenantSchemaAccessor(schema));
    }
}
