using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Par_AttivitaRepository : Repository<TenantDbContext, Par_Attivita>, IPar_AttivitaRepository
    {
        public Par_AttivitaRepository(TenantDbContext dbContext,
                                      IServiceProvider provider,
                                      Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_AttivitaRepository : IRepository<Par_Attivita>
    {
    }
}
