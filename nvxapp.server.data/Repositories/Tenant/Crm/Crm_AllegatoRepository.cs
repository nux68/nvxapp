using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Crm
{
    // tabella del CRM (schema tenant_<IdAzienda>_3)
    public class Crm_AllegatoRepository : Repository<CrmDbContext, Crm_Allegato>, ICrm_AllegatoRepository
    {
        public Crm_AllegatoRepository(CrmDbContext dbContext,
                              IServiceProvider provider,
                              Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICrm_AllegatoRepository : IRepository<Crm_Allegato>
    {
    }
}
