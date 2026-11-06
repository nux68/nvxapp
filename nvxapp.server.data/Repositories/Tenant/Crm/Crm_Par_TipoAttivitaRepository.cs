using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Crm
{
    // tabella del CRM (schema tenant_<IdAzienda>_3)
    public class Crm_Par_TipoAttivitaRepository : Repository<CrmDbContext, Crm_Par_TipoAttivita>, ICrm_Par_TipoAttivitaRepository
    {
        public Crm_Par_TipoAttivitaRepository(CrmDbContext dbContext,
                                      IServiceProvider provider,
                                      Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICrm_Par_TipoAttivitaRepository : IRepository<Crm_Par_TipoAttivita>
    {
    }
}
