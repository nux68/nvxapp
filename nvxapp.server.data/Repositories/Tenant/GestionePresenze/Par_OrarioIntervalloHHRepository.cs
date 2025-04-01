using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{

    /*
     ATTENZIONE !!!
    
     applicationDbContextFactory.CreateDbContext(null) e quello che permette la lettura dei dati
     sul tenat in caso di multi tenant
     
     */

    public class Par_OrarioIntervalloHHRepository : Repository<ApplicationDbContext, Par_OrarioIntervalloHH>, IPar_OrarioIntervalloHHRepository, ICurrentTenant
    {

        private readonly IApplicationDbContextFactory _applicationDbContextFactory;



        public Par_OrarioIntervalloHHRepository(ApplicationDbContext dbContext,
                                 IServiceProvider provider,
                                 IHttpContextAccessor httpContextAccessor,
                                 IApplicationDbContextFactory applicationDbContextFactory) : base(applicationDbContextFactory.CreateDbContext(null), 
                                                                                                  provider, 
                                                                                                  httpContextAccessor)
                                
        {
            _applicationDbContextFactory = applicationDbContextFactory;
        }
    }

    public interface IPar_OrarioIntervalloHHRepository : IRepository<Par_OrarioIntervalloHH>
    {
    }
}
