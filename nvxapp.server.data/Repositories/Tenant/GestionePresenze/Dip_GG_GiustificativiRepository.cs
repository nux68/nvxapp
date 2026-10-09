using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Dip_GG_GiustificativiRepository : Repository<AttendanceTrackingDbContext, Dip_GG_Giustificativi>, IDip_GG_GiustificativiRepository
    {

        public Dip_GG_GiustificativiRepository(AttendanceTrackingDbContext dbContext,
                                               IServiceProvider provider,
                                               Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_GG_GiustificativiRepository : IRepository<Dip_GG_Giustificativi>
    {
    }
}
