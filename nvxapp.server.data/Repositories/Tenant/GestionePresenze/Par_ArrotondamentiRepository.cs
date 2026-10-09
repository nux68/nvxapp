using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Par_ArrotondamentiRepository : Repository<TenantDbContext, Par_Arrotondamenti>, IPar_ArrotondamentiRepository
    {

        public Par_ArrotondamentiRepository(TenantDbContext dbContext,
                                            IServiceProvider provider,
                                            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_ArrotondamentiRepository : IRepository<Par_Arrotondamenti>
    {
    }
}
