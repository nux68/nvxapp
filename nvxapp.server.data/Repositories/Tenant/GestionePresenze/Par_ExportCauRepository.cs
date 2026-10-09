using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Par_ExportCauRepository : Repository<AttendanceTrackingDbContext, Par_ExportCau>, IPar_ExportCauRepository
    {
        public Par_ExportCauRepository(AttendanceTrackingDbContext dbContext,
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
