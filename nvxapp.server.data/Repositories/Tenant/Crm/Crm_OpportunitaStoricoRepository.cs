using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Crm
{
    // tabella del CRM (schema tenant_<IdAzienda>_3)
    public class Crm_OpportunitaStoricoRepository : Repository<CrmDbContext, Crm_OpportunitaStorico>, ICrm_OpportunitaStoricoRepository
    {
        public Crm_OpportunitaStoricoRepository(CrmDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICrm_OpportunitaStoricoRepository : IRepository<Crm_OpportunitaStorico>
    {
    }
}
