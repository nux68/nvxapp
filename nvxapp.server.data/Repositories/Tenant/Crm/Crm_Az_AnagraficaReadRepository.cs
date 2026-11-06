using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Crm
{
    // tabella comune (schema Common) in SOLA LETTURA sul contesto del CRM, per le query che la uniscono alle tabelle
    // del CRM (una query sta su un solo contesto). Le scritture passano dai repository di Repositories/Tenant/Common.
    public class Crm_Az_AnagraficaReadRepository : Repository<CrmDbContext, Az_Anagrafica>, ICrm_Az_AnagraficaReadRepository
    {
        public Crm_Az_AnagraficaReadRepository(CrmDbContext dbContext,
                                       IServiceProvider provider,
                                       Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICrm_Az_AnagraficaReadRepository : IRepository<Az_Anagrafica>
    {
    }
}
