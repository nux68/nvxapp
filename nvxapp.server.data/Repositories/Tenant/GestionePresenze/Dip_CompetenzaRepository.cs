using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_CompetenzaRepository : Repository<TenantDbContext, Dip_Competenza>, IDip_CompetenzaRepository
    {
        public Dip_CompetenzaRepository(TenantDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_CompetenzaRepository : IRepository<Dip_Competenza>
    {
    }
}
