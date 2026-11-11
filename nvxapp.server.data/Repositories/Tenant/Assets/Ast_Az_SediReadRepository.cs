using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Assets
{
    // tabella comune (schema Common) in SOLA LETTURA sul contesto di Risorse aziendali, per le query che la uniscono alle tabelle
    // del modulo (una query sta su un solo contesto). Le scritture passano dai repository di Repositories/Tenant/Common.
    public class Ast_Az_SediReadRepository : Repository<AssetsDbContext, Az_Sedi>, IAst_Az_SediReadRepository
    {
        public Ast_Az_SediReadRepository(AssetsDbContext dbContext,
                         IServiceProvider provider,
                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAst_Az_SediReadRepository : IRepository<Az_Sedi>
    {
    }
}
