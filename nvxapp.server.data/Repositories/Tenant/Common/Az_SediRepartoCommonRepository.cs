using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Common
{
    // tabella comune dell'azienda (schema Common): scritture solo da qui. Nome con "Common" per non confondersi con
    // il repository delle presenze (Repositories.Tenant.GestionePresenze.Az_SediRepartoRepository, sul contesto delle presenze)
    public class Az_SediRepartoCommonRepository : Repository<CommonDbContext, Az_SediReparto>, IAz_SediRepartoCommonRepository
    {
        public Az_SediRepartoCommonRepository(CommonDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_SediRepartoCommonRepository : IRepository<Az_SediReparto>
    {
    }
}
