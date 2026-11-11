using nvxapp.server.data.Entities.Tenant.HumanResources;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.HumanResources
{
    // tabella di Risorse Umane (schema tenant_<IdAzienda>_5)
    public class Hr_ChecklistModelloVoceRepository : Repository<HumanResourcesDbContext, Hr_ChecklistModelloVoce>, IHr_ChecklistModelloVoceRepository
    {
        public Hr_ChecklistModelloVoceRepository(HumanResourcesDbContext dbContext,
                         IServiceProvider provider,
                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IHr_ChecklistModelloVoceRepository : IRepository<Hr_ChecklistModelloVoce>
    {
    }
}
