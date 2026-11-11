using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Compliance
{
    // tabella comune (schema Common) in SOLA LETTURA sul contesto di Compliance, per le query che la uniscono alle tabelle
    // del modulo (una query sta su un solo contesto). Le scritture passano dai repository di Repositories/Tenant/Common.
    public class Cmp_Dip_AnagraficaReadRepository : Repository<ComplianceDbContext, Dip_Anagrafica>, ICmp_Dip_AnagraficaReadRepository
    {
        public Cmp_Dip_AnagraficaReadRepository(ComplianceDbContext dbContext,
                                       IServiceProvider provider,
                                       Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICmp_Dip_AnagraficaReadRepository : IRepository<Dip_Anagrafica>
    {
    }
}
