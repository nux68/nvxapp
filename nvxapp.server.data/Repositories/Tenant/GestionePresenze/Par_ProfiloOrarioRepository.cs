using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Par_ProfiloOrarioRepository : Repository<AttendanceTrackingDbContext, Par_ProfiloOrario>, IPar_ProfiloOrarioRepository
    {

        public Par_ProfiloOrarioRepository(AttendanceTrackingDbContext dbContext,
                                           IServiceProvider provider,
                                           Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_ProfiloOrarioRepository : IRepository<Par_ProfiloOrario>
    {
    }
}
