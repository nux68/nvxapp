using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{





    public class Par_ExportCau_CausaliRepository : Repository<TenantDbContext, Par_ExportCau_Causali>, IPar_ExportCau_CausaliRepository
    {
        public Par_ExportCau_CausaliRepository(TenantDbContext dbContext,
                                               IServiceProvider provider,
                                               Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_ExportCau_CausaliRepository : IRepository<Par_ExportCau_Causali>
    {
    }
}
