using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Crm
{
    // tabella del CRM (schema tenant_<IdAzienda>_3)
    public class Crm_RichiestaMessaggioRepository : Repository<CrmDbContext, Crm_RichiestaMessaggio>, ICrm_RichiestaMessaggioRepository
    {
        public Crm_RichiestaMessaggioRepository(CrmDbContext dbContext,
                                 IServiceProvider provider,
                                 Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICrm_RichiestaMessaggioRepository : IRepository<Crm_RichiestaMessaggio>
    {
    }
}
