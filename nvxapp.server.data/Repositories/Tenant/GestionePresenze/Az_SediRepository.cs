using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Az_SediRepository : Repository<TenantDbContext, Az_Sedi>, IAz_SediRepository
    {

        public Az_SediRepository(TenantDbContext dbContext,
                                 IServiceProvider provider,
                                 Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_SediRepository : IRepository<Az_Sedi>
    {
    }
}
