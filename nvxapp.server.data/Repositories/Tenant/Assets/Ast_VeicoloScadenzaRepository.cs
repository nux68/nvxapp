using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Assets
{
    // tabella di Risorse aziendali (schema tenant_<IdAzienda>_7)
    public class Ast_VeicoloScadenzaRepository : Repository<AssetsDbContext, Ast_VeicoloScadenza>, IAst_VeicoloScadenzaRepository
    {
        public Ast_VeicoloScadenzaRepository(AssetsDbContext dbContext,
                         IServiceProvider provider,
                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAst_VeicoloScadenzaRepository : IRepository<Ast_VeicoloScadenza>
    {
    }
}
