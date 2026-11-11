using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_TimbraturaPinRepository : Repository<AttendanceTrackingDbContext, Dip_TimbraturaPin>, IDip_TimbraturaPinRepository
    {
        public Dip_TimbraturaPinRepository(AttendanceTrackingDbContext dbContext,
                             IServiceProvider provider,
                             Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_TimbraturaPinRepository : IRepository<Dip_TimbraturaPin>
    {
    }
}
