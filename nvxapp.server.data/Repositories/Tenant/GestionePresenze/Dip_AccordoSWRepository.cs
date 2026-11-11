using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_AccordoSWRepository : Repository<AttendanceTrackingDbContext, Dip_AccordoSW>, IDip_AccordoSWRepository
    {
        public Dip_AccordoSWRepository(AttendanceTrackingDbContext dbContext,
                             IServiceProvider provider,
                             Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_AccordoSWRepository : IRepository<Dip_AccordoSW>
    {
    }
}
