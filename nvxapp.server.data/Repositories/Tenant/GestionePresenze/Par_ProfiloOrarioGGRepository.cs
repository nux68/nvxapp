using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Par_ProfiloOrarioGGRepository : Repository<TenantDbContext, Par_ProfiloOrarioGG>, IPar_ProfiloOrarioGGRepository
    {

        public Par_ProfiloOrarioGGRepository(TenantDbContext dbContext,
                                             IServiceProvider provider,
                                             Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_ProfiloOrarioGGRepository : IRepository<Par_ProfiloOrarioGG>
    {
    }
}
