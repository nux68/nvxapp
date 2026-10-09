using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Dip_GG_RichiestaRepository : Repository<TenantDbContext, Dip_GG_Richiesta>, IDip_GG_RichiestaRepository
    {

        public Dip_GG_RichiestaRepository(TenantDbContext dbContext,
                                          IServiceProvider provider,
                                          Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_GG_RichiestaRepository : IRepository<Dip_GG_Richiesta>
    {
    }
}
