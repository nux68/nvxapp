using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Az_SediAttivitaRepository : Repository<TenantDbContext, Az_SediAttivita>, IAz_SediAttivitaRepository
    {
        public Az_SediAttivitaRepository(TenantDbContext dbContext,
                                         IServiceProvider provider,
                                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_SediAttivitaRepository : IRepository<Az_SediAttivita>
    {
    }
}
