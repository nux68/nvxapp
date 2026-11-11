using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Common
{
    // tabella comune dell'azienda (schema Common): scritture solo da qui. Nome con "Common" per non confondersi con
    // il repository delle presenze (Repositories.Tenant.GestionePresenze.Par_CompetenzaRepository, sul contesto delle presenze)
    public class Par_CompetenzaCommonRepository : Repository<CommonDbContext, Par_Competenza>, IPar_CompetenzaCommonRepository
    {
        public Par_CompetenzaCommonRepository(CommonDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPar_CompetenzaCommonRepository : IRepository<Par_Competenza>
    {
    }
}
