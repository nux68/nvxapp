using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Par_CausaliRepository : Repository<AttendanceTrackingDbContext, Par_Causali>, IPar_CausaliRepository
    {

        public Par_CausaliRepository(AttendanceTrackingDbContext dbContext,
                                     IServiceProvider provider,
                                     Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_CausaliRepository : IRepository<Par_Causali>
    {
    }
}
