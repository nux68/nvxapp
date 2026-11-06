using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Crm
{
    // tabella del CRM (schema tenant_<IdAzienda>_3)
    public class Crm_NotaRepository : Repository<CrmDbContext, Crm_Nota>, ICrm_NotaRepository
    {
        public Crm_NotaRepository(CrmDbContext dbContext,
                          IServiceProvider provider,
                          Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICrm_NotaRepository : IRepository<Crm_Nota>
    {
    }
}
