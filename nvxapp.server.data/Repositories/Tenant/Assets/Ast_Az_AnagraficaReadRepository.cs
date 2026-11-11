using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Assets
{
    // tabella comune (schema Common) in SOLA LETTURA sul contesto di Risorse aziendali, per le query che la uniscono alle tabelle
    // del modulo (una query sta su un solo contesto). Le scritture passano dai repository di Repositories/Tenant/Common.
    public class Ast_Az_AnagraficaReadRepository : Repository<AssetsDbContext, Az_Anagrafica>, IAst_Az_AnagraficaReadRepository
    {
        public Ast_Az_AnagraficaReadRepository(AssetsDbContext dbContext,
                                       IServiceProvider provider,
                                       Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAst_Az_AnagraficaReadRepository : IRepository<Az_Anagrafica>
    {
    }
}
