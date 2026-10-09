using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Par_GiustificativiRepository : Repository<AttendanceTrackingDbContext, Par_Giustificativi>, IPar_GiustificativiRepository
    {

        public Par_GiustificativiRepository(AttendanceTrackingDbContext dbContext,
                                            IServiceProvider provider,
                                            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_GiustificativiRepository : IRepository<Par_Giustificativi>
    {
    }
}
