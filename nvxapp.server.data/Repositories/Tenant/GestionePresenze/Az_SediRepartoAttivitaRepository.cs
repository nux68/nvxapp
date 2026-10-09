using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Az_SediRepartoAttivitaRepository : Repository<TenantDbContext, Az_SediRepartoAttivita>, IAz_SediRepartoAttivitaRepository
    {

        public Az_SediRepartoAttivitaRepository(TenantDbContext dbContext,
                                                IServiceProvider provider,
                                                Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_SediRepartoAttivitaRepository : IRepository<Az_SediRepartoAttivita>
    {
    }
}
