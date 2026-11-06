using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Crm
{
    // tabella del CRM (schema tenant_<IdAzienda>_3)
    public class Crm_Par_FonteRepository : Repository<CrmDbContext, Crm_Par_Fonte>, ICrm_Par_FonteRepository
    {
        public Crm_Par_FonteRepository(CrmDbContext dbContext,
                               IServiceProvider provider,
                               Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICrm_Par_FonteRepository : IRepository<Crm_Par_Fonte>
    {
    }
}
