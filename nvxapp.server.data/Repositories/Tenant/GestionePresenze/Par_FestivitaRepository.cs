using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Par_FestivitaRepository : Repository<AttendanceTrackingDbContext, Par_Festivita>, IPar_FestivitaRepository
    {
        public Par_FestivitaRepository(AttendanceTrackingDbContext dbContext,
                                       IServiceProvider provider,
                                       Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_FestivitaRepository : IRepository<Par_Festivita>
    {
    }
}
