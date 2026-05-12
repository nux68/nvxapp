using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_Contatori_RiportoRepository
        : Repository<ApplicationDbContext, Dip_Contatori_Riporto>,
          IDip_Contatori_RiportoRepository,
          ICurrentTenant
    {
        private readonly IApplicationDbContextFactory _applicationDbContextFactory;

        public Dip_Contatori_RiportoRepository(ApplicationDbContext dbContext,
                                                IServiceProvider provider,
                                                IHttpContextAccessor httpContextAccessor,
                                                IApplicationDbContextFactory applicationDbContextFactory)
            : base(applicationDbContextFactory.CreateDbContext(null), provider, httpContextAccessor)
        {
            _applicationDbContextFactory = applicationDbContextFactory;
        }
    }

    public interface IDip_Contatori_RiportoRepository
        : IRepository<Dip_Contatori_Riporto>
    {
    }
}
