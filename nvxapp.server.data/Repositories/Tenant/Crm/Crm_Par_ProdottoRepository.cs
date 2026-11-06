using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Crm
{
    // tabella del CRM (schema tenant_<IdAzienda>_3)
    public class Crm_Par_ProdottoRepository : Repository<CrmDbContext, Crm_Par_Prodotto>, ICrm_Par_ProdottoRepository
    {
        public Crm_Par_ProdottoRepository(CrmDbContext dbContext,
                                  IServiceProvider provider,
                                  Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICrm_Par_ProdottoRepository : IRepository<Crm_Par_Prodotto>
    {
    }
}
