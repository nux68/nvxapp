using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Assets
{
    // tabella comune (schema Common) in SOLA LETTURA sul contesto di Risorse aziendali, per le query che la uniscono alle tabelle
    // del modulo (una query sta su un solo contesto). Le scritture passano dai repository di Repositories/Tenant/Common.
    public class Ast_Dip_RapportoAutovettureReadRepository : Repository<AssetsDbContext, Dip_RapportoAutovetture>, IAst_Dip_RapportoAutovettureReadRepository
    {
        public Ast_Dip_RapportoAutovettureReadRepository(AssetsDbContext dbContext,
                         IServiceProvider provider,
                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAst_Dip_RapportoAutovettureReadRepository : IRepository<Dip_RapportoAutovetture>
    {
    }
}
