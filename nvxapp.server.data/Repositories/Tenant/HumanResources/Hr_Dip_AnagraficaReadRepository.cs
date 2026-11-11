using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.HumanResources
{
    // tabella comune (schema Common) in SOLA LETTURA sul contesto di Risorse Umane, per le query che la uniscono alle tabelle
    // del modulo (una query sta su un solo contesto). Le scritture passano dai repository di Repositories/Tenant/Common.
    public class Hr_Dip_AnagraficaReadRepository : Repository<HumanResourcesDbContext, Dip_Anagrafica>, IHr_Dip_AnagraficaReadRepository
    {
        public Hr_Dip_AnagraficaReadRepository(HumanResourcesDbContext dbContext,
                                       IServiceProvider provider,
                                       Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IHr_Dip_AnagraficaReadRepository : IRepository<Dip_Anagrafica>
    {
    }
}
