using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Az_SediRepartoRepository : Repository<AttendanceTrackingDbContext, Az_SediReparto>, IAz_SediRepartoRepository
    {

        public Az_SediRepartoRepository(AttendanceTrackingDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_SediRepartoRepository : IRepository<Az_SediReparto>
    {
    }
}
