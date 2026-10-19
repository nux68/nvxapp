using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Par_TipoSpesaRepository : Repository<AttendanceTrackingDbContext, Par_TipoSpesa>, IPar_TipoSpesaRepository
    {
        public Par_TipoSpesaRepository(AttendanceTrackingDbContext dbContext,
                              IServiceProvider provider,
                              Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_TipoSpesaRepository : IRepository<Par_TipoSpesa>
    {
    }
}
