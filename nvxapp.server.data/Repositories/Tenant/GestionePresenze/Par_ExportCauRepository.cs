using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Par_ExportCauRepository : Repository<TenantDbContext, Par_ExportCau>, IPar_ExportCauRepository
    {
        public Par_ExportCauRepository(TenantDbContext dbContext,
                                       IServiceProvider provider,
                                       Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_ExportCauRepository : IRepository<Par_ExportCau>
    {
    }


 
}
