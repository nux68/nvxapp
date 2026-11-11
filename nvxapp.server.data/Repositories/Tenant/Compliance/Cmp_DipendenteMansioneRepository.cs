using nvxapp.server.data.Entities.Tenant.Compliance;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Compliance
{
    // tabella di Compliance (schema tenant_<IdAzienda>_6)
    public class Cmp_DipendenteMansioneRepository : Repository<ComplianceDbContext, Cmp_DipendenteMansione>, ICmp_DipendenteMansioneRepository
    {
        public Cmp_DipendenteMansioneRepository(ComplianceDbContext dbContext,
                         IServiceProvider provider,
                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICmp_DipendenteMansioneRepository : IRepository<Cmp_DipendenteMansione>
    {
    }
}
