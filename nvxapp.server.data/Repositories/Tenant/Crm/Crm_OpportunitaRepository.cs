using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Crm
{
    // tabella del CRM (schema tenant_<IdAzienda>_3)
    public class Crm_OpportunitaRepository : Repository<CrmDbContext, Crm_Opportunita>, ICrm_OpportunitaRepository
    {
        public Crm_OpportunitaRepository(CrmDbContext dbContext,
                                 IServiceProvider provider,
                                 Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICrm_OpportunitaRepository : IRepository<Crm_Opportunita>
    {
    }
}
