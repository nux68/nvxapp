using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_ProfiloFormuleRepository : Repository<AttendanceTrackingDbContext, Dip_ProfiloFormule>, IDip_ProfiloFormuleRepository
    {
        public Dip_ProfiloFormuleRepository(AttendanceTrackingDbContext dbContext,
                              IServiceProvider provider,
                              Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_ProfiloFormuleRepository : IRepository<Dip_ProfiloFormule>
    {
    }
}
