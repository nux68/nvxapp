using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Par_TurnoRepository : Repository<AttendanceTrackingDbContext, Par_Turno>, IPar_TurnoRepository
    {
        public Par_TurnoRepository(AttendanceTrackingDbContext dbContext,
                             IServiceProvider provider,
                             Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_TurnoRepository : IRepository<Par_Turno>
    {
    }
}
