using nvxapp.server.data.Entities.Tenant.HumanResources;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.HumanResources
{
    // tabella di Risorse Umane (schema tenant_<IdAzienda>_5)
    public class Hr_ChecklistModelloRepository : Repository<HumanResourcesDbContext, Hr_ChecklistModello>, IHr_ChecklistModelloRepository
    {
        public Hr_ChecklistModelloRepository(HumanResourcesDbContext dbContext,
                         IServiceProvider provider,
                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IHr_ChecklistModelloRepository : IRepository<Hr_ChecklistModello>
    {
    }
}
