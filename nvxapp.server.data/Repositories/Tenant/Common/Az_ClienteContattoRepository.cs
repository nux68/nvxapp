using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Common
{
    // tabella comune dell'azienda (schema Common): scritture solo da qui
    public class Az_ClienteContattoRepository : Repository<CommonDbContext, Az_ClienteContatto>, IAz_ClienteContattoRepository
    {
        public Az_ClienteContattoRepository(CommonDbContext dbContext,
                              IServiceProvider provider,
                              Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_ClienteContattoRepository : IRepository<Az_ClienteContatto>
    {
    }
}
