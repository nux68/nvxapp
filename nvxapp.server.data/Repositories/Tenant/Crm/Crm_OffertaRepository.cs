using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Crm
{
    // tabella del CRM (schema tenant_<IdAzienda>_3)
    public class Crm_OffertaRepository : Repository<CrmDbContext, Crm_Offerta>, ICrm_OffertaRepository
    {
        public Crm_OffertaRepository(CrmDbContext dbContext,
                             IServiceProvider provider,
                             Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICrm_OffertaRepository : IRepository<Crm_Offerta>
    {
    }
}
