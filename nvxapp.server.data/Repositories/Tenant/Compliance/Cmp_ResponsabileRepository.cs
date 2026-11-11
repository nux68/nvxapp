using nvxapp.server.data.Entities.Tenant.Compliance;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Compliance
{
    // tabella di Compliance (schema tenant_<IdAzienda>_6)
    public class Cmp_ResponsabileRepository : Repository<ComplianceDbContext, Cmp_Responsabile>, ICmp_ResponsabileRepository
    {
        public Cmp_ResponsabileRepository(ComplianceDbContext dbContext,
                         IServiceProvider provider,
                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICmp_ResponsabileRepository : IRepository<Cmp_Responsabile>
    {
    }
}
