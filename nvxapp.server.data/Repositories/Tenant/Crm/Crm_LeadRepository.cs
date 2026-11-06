using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Crm
{
    // tabella del CRM (schema tenant_<IdAzienda>_3)
    public class Crm_LeadRepository : Repository<CrmDbContext, Crm_Lead>, ICrm_LeadRepository
    {
        public Crm_LeadRepository(CrmDbContext dbContext,
                          IServiceProvider provider,
                          Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICrm_LeadRepository : IRepository<Crm_Lead>
    {
    }
}
