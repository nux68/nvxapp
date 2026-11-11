using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_TurnoScambioRepository : Repository<AttendanceTrackingDbContext, Dip_TurnoScambio>, IDip_TurnoScambioRepository
    {
        public Dip_TurnoScambioRepository(AttendanceTrackingDbContext dbContext,
                             IServiceProvider provider,
                             Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_TurnoScambioRepository : IRepository<Dip_TurnoScambio>
    {
    }
}
