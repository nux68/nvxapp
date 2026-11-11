using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Az_SediRepartoTurnoRepository : Repository<AttendanceTrackingDbContext, Az_SediRepartoTurno>, IAz_SediRepartoTurnoRepository
    {
        public Az_SediRepartoTurnoRepository(AttendanceTrackingDbContext dbContext,
                             IServiceProvider provider,
                             Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_SediRepartoTurnoRepository : IRepository<Az_SediRepartoTurno>
    {
    }
}
