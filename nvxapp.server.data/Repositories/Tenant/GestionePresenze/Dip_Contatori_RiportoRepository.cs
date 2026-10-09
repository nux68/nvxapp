using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_Contatori_RiportoRepository
        : Repository<TenantDbContext, Dip_Contatori_Riporto>,
          IDip_Contatori_RiportoRepository
    {
        public Dip_Contatori_RiportoRepository(TenantDbContext dbContext,
                                               IServiceProvider provider,
                                               Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_Contatori_RiportoRepository
        : IRepository<Dip_Contatori_Riporto>
    {
    }
}
