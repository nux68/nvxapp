using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Az_CfgRepository : Repository<TenantDbContext, Az_Cfg>, IAz_CfgRepository
    {

        public Az_CfgRepository(TenantDbContext dbContext,
                                IServiceProvider provider,
                                Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_CfgRepository : IRepository<Az_Cfg>
    {
    }
}
