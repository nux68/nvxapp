using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Par_CompetenzaRepository : Repository<AttendanceTrackingDbContext, Par_Competenza>, IPar_CompetenzaRepository
    {
        public Par_CompetenzaRepository(AttendanceTrackingDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_CompetenzaRepository : IRepository<Par_Competenza>
    {
    }
}
