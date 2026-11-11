using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Par_OrarioPausaRepository : Repository<AttendanceTrackingDbContext, Par_OrarioPausa>, IPar_OrarioPausaRepository
    {
        public Par_OrarioPausaRepository(AttendanceTrackingDbContext dbContext,
                                         IServiceProvider provider,
                                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_OrarioPausaRepository : IRepository<Par_OrarioPausa>
    {
    }
}
