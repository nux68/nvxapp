using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Az_AttivitaRepository : Repository<ApplicationDbContext, Az_Attivita>, IAz_AttivitaRepository, ICurrentTenant
    {
        private readonly IApplicationDbContextFactory _applicationDbContextFactory;

        public Az_AttivitaRepository(ApplicationDbContext dbContext,
                                 IServiceProvider provider,
                                 IHttpContextAccessor httpContextAccessor,
                                 IApplicationDbContextFactory applicationDbContextFactory) : base(applicationDbContextFactory.CreateDbContext(null), 
                                                                                                  provider, 
                                                                                                  httpContextAccessor)
        {
            _applicationDbContextFactory = applicationDbContextFactory;
        }
    }

    public interface IAz_AttivitaRepository : IRepository<Az_Attivita>
    {
    }
}
