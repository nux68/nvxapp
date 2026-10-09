using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Az_SediRepartoUserRepository : Repository<AttendanceTrackingDbContext, Az_SediRepartoUser>, IAz_SediRepartoUserRepository
    {

        public Az_SediRepartoUserRepository(AttendanceTrackingDbContext dbContext,
                                            IServiceProvider provider,
                                            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_SediRepartoUserRepository : IRepository<Az_SediRepartoUser>
    {
    }
}
