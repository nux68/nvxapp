using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Common
{
    // tabella comune dell'azienda (schema Common): scritture solo da qui. Nome con "Common" per non confondersi con
    // il repository delle presenze (Repositories.Tenant.GestionePresenze.Dip_AnagraficaRepository, sul contesto delle presenze)
    public class Dip_AnagraficaCommonRepository : Repository<CommonDbContext, Dip_Anagrafica>, IDip_AnagraficaCommonRepository
    {
        public Dip_AnagraficaCommonRepository(CommonDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_AnagraficaCommonRepository : IRepository<Dip_Anagrafica>
    {
    }
}
