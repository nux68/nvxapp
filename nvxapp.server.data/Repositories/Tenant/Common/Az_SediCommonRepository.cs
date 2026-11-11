using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Common
{
    // tabella comune dell'azienda (schema Common): scritture solo da qui. Nome con "Common" per non confondersi con
    // il repository delle presenze (Repositories.Tenant.GestionePresenze.Az_SediRepository, sul contesto delle presenze)
    public class Az_SediCommonRepository : Repository<CommonDbContext, Az_Sedi>, IAz_SediCommonRepository
    {
        public Az_SediCommonRepository(CommonDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_SediCommonRepository : IRepository<Az_Sedi>
    {
    }
}
