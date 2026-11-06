using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Crm
{
    // tabella del CRM (schema tenant_<IdAzienda>_3)
    public class Crm_Par_TagRepository : Repository<CrmDbContext, Crm_Par_Tag>, ICrm_Par_TagRepository
    {
        public Crm_Par_TagRepository(CrmDbContext dbContext,
                             IServiceProvider provider,
                             Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICrm_Par_TagRepository : IRepository<Crm_Par_Tag>
    {
    }
}
