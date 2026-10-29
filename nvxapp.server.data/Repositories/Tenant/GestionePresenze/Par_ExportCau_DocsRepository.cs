using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{





    public class Par_ExportCau_DocsRepository : Repository<AttendanceTrackingDbContext, Par_ExportCau_Docs>, IPar_ExportCau_DocsRepository
    {
        public Par_ExportCau_DocsRepository(AttendanceTrackingDbContext dbContext,
                                               IServiceProvider provider,
                                               Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_ExportCau_DocsRepository : IRepository<Par_ExportCau_Docs>
    {
    }
}
