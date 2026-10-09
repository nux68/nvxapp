using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Az_SubCommessaUserRepository : Repository<TenantDbContext, Az_SubCommessaUser>, IAz_SubCommessaUserRepository
    {
        public Az_SubCommessaUserRepository(TenantDbContext dbContext,
                                            IServiceProvider provider,
                                            Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_SubCommessaUserRepository : IRepository<Az_SubCommessaUser>
    {
    }
}