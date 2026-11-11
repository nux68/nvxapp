using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.HumanResources
{
    // tabella comune (schema Common) in SOLA LETTURA sul contesto di Risorse Umane, per le query che la uniscono alle tabelle
    // del modulo (una query sta su un solo contesto). Le scritture passano dai repository di Repositories/Tenant/Common.
    public class Hr_Az_AnagraficaReadRepository : Repository<HumanResourcesDbContext, Az_Anagrafica>, IHr_Az_AnagraficaReadRepository
    {
        public Hr_Az_AnagraficaReadRepository(HumanResourcesDbContext dbContext,
                                       IServiceProvider provider,
                                       Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IHr_Az_AnagraficaReadRepository : IRepository<Az_Anagrafica>
    {
    }
}
