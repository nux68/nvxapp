using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Par_ProfiloFormuleRepository : Repository<AttendanceTrackingDbContext, Par_ProfiloFormule>, IPar_ProfiloFormuleRepository
    {
        public Par_ProfiloFormuleRepository(AttendanceTrackingDbContext dbContext,
                              IServiceProvider provider,
                              Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_ProfiloFormuleRepository : IRepository<Par_ProfiloFormule>
    {
    }
}
