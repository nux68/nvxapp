using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Assets
{
    // tabella di Risorse aziendali (schema tenant_<IdAzienda>_7)
    public class Ast_VeicoloRepository : Repository<AssetsDbContext, Ast_Veicolo>, IAst_VeicoloRepository
    {
        public Ast_VeicoloRepository(AssetsDbContext dbContext,
                         IServiceProvider provider,
                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAst_VeicoloRepository : IRepository<Ast_Veicolo>
    {
    }
}
