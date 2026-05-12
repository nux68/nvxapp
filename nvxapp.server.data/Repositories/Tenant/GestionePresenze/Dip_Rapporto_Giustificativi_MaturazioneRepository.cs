using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_Rapporto_Giustificativi_MaturazioneRepository
        : Repository<ApplicationDbContext, Dip_Rapporto_Giustificativi_Maturazione>,
          IDip_Rapporto_Giustificativi_MaturazioneRepository,
          ICurrentTenant
    {
        private readonly IApplicationDbContextFactory _applicationDbContextFactory;

        public Dip_Rapporto_Giustificativi_MaturazioneRepository(ApplicationDbContext dbContext,
                                                                  IServiceProvider provider,
                                                                  IHttpContextAccessor httpContextAccessor,
                                                                  IApplicationDbContextFactory applicationDbContextFactory)
            : base(applicationDbContextFactory.CreateDbContext(null), provider, httpContextAccessor)
        {
            _applicationDbContextFactory = applicationDbContextFactory;
        }
    }

    public interface IDip_Rapporto_Giustificativi_MaturazioneRepository
        : IRepository<Dip_Rapporto_Giustificativi_Maturazione>
    {
    }
}
