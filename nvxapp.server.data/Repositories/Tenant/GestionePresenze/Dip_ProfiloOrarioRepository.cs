using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Dip_ProfiloOrarioRepository : Repository<TenantDbContext, Dip_ProfiloOrario>, IDip_ProfiloOrarioRepository
    {

        public Dip_ProfiloOrarioRepository(TenantDbContext dbContext,
                                           IServiceProvider provider,
                                           Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_ProfiloOrarioRepository : IRepository<Dip_ProfiloOrario>
    {
    }
}
