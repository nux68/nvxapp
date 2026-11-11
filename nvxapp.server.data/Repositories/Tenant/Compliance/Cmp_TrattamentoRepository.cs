using nvxapp.server.data.Entities.Tenant.Compliance;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Compliance
{
    // tabella di Compliance (schema tenant_<IdAzienda>_6)
    public class Cmp_TrattamentoRepository : Repository<ComplianceDbContext, Cmp_Trattamento>, ICmp_TrattamentoRepository
    {
        public Cmp_TrattamentoRepository(ComplianceDbContext dbContext,
                         IServiceProvider provider,
                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICmp_TrattamentoRepository : IRepository<Cmp_Trattamento>
    {
    }
}
