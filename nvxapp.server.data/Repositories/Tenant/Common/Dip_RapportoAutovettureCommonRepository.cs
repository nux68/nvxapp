using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Common
{
    // tabella comune dell'azienda (schema Common): scritture solo da qui. Nome con "Common" per non confondersi con
    // il repository delle presenze (Repositories.Tenant.GestionePresenze.Dip_RapportoAutovettureRepository, sul contesto delle presenze)
    public class Dip_RapportoAutovettureCommonRepository : Repository<CommonDbContext, Dip_RapportoAutovetture>, IDip_RapportoAutovettureCommonRepository
    {
        public Dip_RapportoAutovettureCommonRepository(CommonDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_RapportoAutovettureCommonRepository : IRepository<Dip_RapportoAutovetture>
    {
    }
}
