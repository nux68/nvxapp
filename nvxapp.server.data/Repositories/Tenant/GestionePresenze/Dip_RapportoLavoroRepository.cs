using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Dip_RapportoLavoroRepository : Repository<AttendanceTrackingDbContext, Dip_RapportoLavoro>, IDip_RapportoLavoroRepository
    {

        public Dip_RapportoLavoroRepository(AttendanceTrackingDbContext dbContext,
                                            IServiceProvider provider,
                                            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_RapportoLavoroRepository : IRepository<Dip_RapportoLavoro>
    {
    }
}
