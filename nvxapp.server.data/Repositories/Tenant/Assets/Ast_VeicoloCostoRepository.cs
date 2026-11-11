using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Assets
{
    // tabella di Risorse aziendali (schema tenant_<IdAzienda>_7)
    public class Ast_VeicoloCostoRepository : Repository<AssetsDbContext, Ast_VeicoloCosto>, IAst_VeicoloCostoRepository
    {
        public Ast_VeicoloCostoRepository(AssetsDbContext dbContext,
                         IServiceProvider provider,
                         Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAst_VeicoloCostoRepository : IRepository<Ast_VeicoloCosto>
    {
    }
}
