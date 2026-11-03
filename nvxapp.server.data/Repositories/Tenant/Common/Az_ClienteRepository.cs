using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Common
{
    // tabella comune dell'azienda (schema Common): scritture solo da qui
    public class Az_ClienteRepository : Repository<CommonDbContext, Az_Cliente>, IAz_ClienteRepository
    {
        public Az_ClienteRepository(CommonDbContext dbContext,
                              IServiceProvider provider,
                              Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_ClienteRepository : IRepository<Az_Cliente>
    {
    }
}
