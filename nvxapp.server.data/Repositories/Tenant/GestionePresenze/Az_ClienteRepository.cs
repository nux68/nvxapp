using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Az_ClienteRepository : Repository<TenantDbContext, Az_Cliente>, IAz_ClienteRepository
    {
        public Az_ClienteRepository(TenantDbContext dbContext,
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
