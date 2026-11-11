using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Par_ProfiloFormuleDettRepository : Repository<AttendanceTrackingDbContext, Par_ProfiloFormuleDett>, IPar_ProfiloFormuleDettRepository
    {
        public Par_ProfiloFormuleDettRepository(AttendanceTrackingDbContext dbContext,
                              IServiceProvider provider,
                              Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_ProfiloFormuleDettRepository : IRepository<Par_ProfiloFormuleDett>
    {
    }
}
