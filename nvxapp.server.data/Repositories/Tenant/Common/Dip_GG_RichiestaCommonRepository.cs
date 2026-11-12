using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Common
{
    // richieste da approvare di tutti gli applicativi (schema Common, ANALISI_INFRASTRUCTURE_RICHIESTE.md): scritture solo da qui. Nome con "Common" per non confondersi con
    // il repository delle presenze (contesto delle presenze, che la mappa in sola lettura per le navigazioni)
    public class Dip_GG_RichiestaCommonRepository : Repository<CommonDbContext, Dip_GG_Richiesta>, IDip_GG_RichiestaCommonRepository
    {
        public Dip_GG_RichiestaCommonRepository(CommonDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_GG_RichiestaCommonRepository : IRepository<Dip_GG_Richiesta>
    {
    }
}
