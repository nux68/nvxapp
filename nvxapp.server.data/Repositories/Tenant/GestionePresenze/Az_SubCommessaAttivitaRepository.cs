using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Az_SubCommessaAttivitaRepository : Repository<TenantDbContext, Az_SubCommessaAttivita>, IAz_SubCommessaAttivitaRepository
    {
        public Az_SubCommessaAttivitaRepository(TenantDbContext dbContext,
                                                IServiceProvider provider,
                                                Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_SubCommessaAttivitaRepository : IRepository<Az_SubCommessaAttivita>
    {
    }
}
