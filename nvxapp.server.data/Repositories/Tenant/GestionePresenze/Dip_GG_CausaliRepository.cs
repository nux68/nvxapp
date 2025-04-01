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

    public class Dip_GG_CausaliRepository : Repository<ApplicationDbContext, Dip_GG_Causali>, IDip_GG_CausaliRepository, ICurrentTenant
    {

        private readonly IApplicationDbContextFactory _applicationDbContextFactory;



        public Dip_GG_CausaliRepository(ApplicationDbContext dbContext,
                                 IServiceProvider provider,
                                 IHttpContextAccessor httpContextAccessor,
                                 IApplicationDbContextFactory applicationDbContextFactory) : base(applicationDbContextFactory.CreateDbContext(null), 
                                                                                                  provider, 
                                                                                                  httpContextAccessor)
                                
        {
            _applicationDbContextFactory = applicationDbContextFactory;
        }
    }

    public interface IDip_GG_CausaliRepository : IRepository<Dip_GG_Causali>
    {
    }
}
