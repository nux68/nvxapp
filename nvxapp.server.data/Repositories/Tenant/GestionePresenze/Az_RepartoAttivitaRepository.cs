using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{

    /*
     ATTENZIONE !!!
    
     applicationDbContextFactory.CreateDbContext(null) e quello che permette la lettura dei dati
     sul tenat in caso di multi tenant
     
     */

    public class Az_RepartoAttivitaRepository : Repository<ApplicationDbContext, Az_RepartoAttivita>, IAz_RepartoAttivitaRepository, ICurrentTenant
    {

        private readonly IApplicationDbContextFactory _applicationDbContextFactory;



        public Az_RepartoAttivitaRepository(ApplicationDbContext dbContext,
                                 IServiceProvider provider,
                                 IHttpContextAccessor httpContextAccessor,
                                 IApplicationDbContextFactory applicationDbContextFactory) : base(applicationDbContextFactory.CreateDbContext(null), 
                                                                                                  provider, 
                                                                                                  httpContextAccessor)
                                
        {
            _applicationDbContextFactory = applicationDbContextFactory;
        }
    }

    public interface IAz_RepartoAttivitaRepository : IRepository<Az_RepartoAttivita>
    {
    }
}
