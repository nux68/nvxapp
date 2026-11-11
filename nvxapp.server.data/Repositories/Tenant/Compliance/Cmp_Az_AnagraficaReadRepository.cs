using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Compliance
{
    // tabella comune (schema Common) in SOLA LETTURA sul contesto di Compliance, per le query che la uniscono alle tabelle
    // del modulo (una query sta su un solo contesto). Le scritture passano dai repository di Repositories/Tenant/Common.
    public class Cmp_Az_AnagraficaReadRepository : Repository<ComplianceDbContext, Az_Anagrafica>, ICmp_Az_AnagraficaReadRepository
    {
        public Cmp_Az_AnagraficaReadRepository(ComplianceDbContext dbContext,
                                       IServiceProvider provider,
                                       Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICmp_Az_AnagraficaReadRepository : IRepository<Az_Anagrafica>
    {
    }
}
