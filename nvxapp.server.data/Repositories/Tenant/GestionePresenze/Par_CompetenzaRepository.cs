using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Par_CompetenzaRepository : Repository<ApplicationDbContext, Par_Competenza>, IPar_CompetenzaRepository, ICurrentTenant
    {
        private readonly IApplicationDbContextFactory _applicationDbContextFactory;

        public Par_CompetenzaRepository(ApplicationDbContext dbContext,
                                 IServiceProvider provider,
                                 IHttpContextAccessor httpContextAccessor,
                                 IApplicationDbContextFactory applicationDbContextFactory) : base(applicationDbContextFactory.CreateDbContext(null), 
                                                                                                  provider, 
                                                                                                  httpContextAccessor)
        {
            _applicationDbContextFactory = applicationDbContextFactory;
        }
    }

    public interface IPar_CompetenzaRepository : IRepository<Par_Competenza>
    {
    }
}
