using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Par_AttivitaCompetenzaRepository : Repository<AttendanceTrackingDbContext, Par_AttivitaCompetenza>, IPar_AttivitaCompetenzaRepository
    {
        public Par_AttivitaCompetenzaRepository(AttendanceTrackingDbContext dbContext,
                                                IServiceProvider provider,
                                                Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_AttivitaCompetenzaRepository : IRepository<Par_AttivitaCompetenza>
    {
    }
}
