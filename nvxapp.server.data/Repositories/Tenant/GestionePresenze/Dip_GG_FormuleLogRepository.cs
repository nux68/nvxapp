using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_GG_FormuleLogRepository : Repository<AttendanceTrackingDbContext, Dip_GG_FormuleLog>, IDip_GG_FormuleLogRepository
    {
        public Dip_GG_FormuleLogRepository(AttendanceTrackingDbContext dbContext,
                              IServiceProvider provider,
                              Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_GG_FormuleLogRepository : IRepository<Dip_GG_FormuleLog>
    {
    }
}
