using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Az_SubCommessaRepository : Repository<TenantDbContext, Az_SubCommessa>, IAz_SubCommessaRepository
    {
        public Az_SubCommessaRepository(TenantDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_SubCommessaRepository : IRepository<Az_SubCommessa>
    {
    }
}
