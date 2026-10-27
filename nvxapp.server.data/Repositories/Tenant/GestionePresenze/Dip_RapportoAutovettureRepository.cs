using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_RapportoAutovettureRepository : Repository<AttendanceTrackingDbContext, Dip_RapportoAutovetture>, IDip_RapportoAutovettureRepository
    {
        public Dip_RapportoAutovettureRepository(AttendanceTrackingDbContext dbContext,
                              IServiceProvider provider,
                              Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_RapportoAutovettureRepository : IRepository<Dip_RapportoAutovetture>
    {
    }
}
