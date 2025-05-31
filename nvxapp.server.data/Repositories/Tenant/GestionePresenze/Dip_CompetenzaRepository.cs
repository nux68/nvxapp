using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_CompetenzaRepository : Repository<ApplicationDbContext, Dip_Competenza>, IDip_CompetenzaRepository, ICurrentTenant
    {
        private readonly IApplicationDbContextFactory _applicationDbContextFactory;

        public Dip_CompetenzaRepository(ApplicationDbContext dbContext,
                                 IServiceProvider provider,
                                 IHttpContextAccessor httpContextAccessor,
                                 IApplicationDbContextFactory applicationDbContextFactory) : base(applicationDbContextFactory.CreateDbContext(null), 
                                                                                                  provider, 
                                                                                                  httpContextAccessor)
        {
            _applicationDbContextFactory = applicationDbContextFactory;
        }
    }

    public interface IDip_CompetenzaRepository : IRepository<Dip_Competenza>
    {
    }
}
