using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Dip_GG_CausaliRepository : Repository<AttendanceTrackingDbContext, Dip_GG_Causali>, IDip_GG_CausaliRepository
    {

        public Dip_GG_CausaliRepository(AttendanceTrackingDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_GG_CausaliRepository : IRepository<Dip_GG_Causali>
    {
    }
}
