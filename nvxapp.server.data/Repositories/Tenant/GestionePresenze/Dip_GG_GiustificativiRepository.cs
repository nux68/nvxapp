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

    public class Dip_GG_GiustificativiRepository : Repository<ApplicationDbContext, Dip_GG_Giustificativi>, IDip_GG_GiustificativiRepository, ICurrentTenant
    {

        private readonly IApplicationDbContextFactory _applicationDbContextFactory;



        public Dip_GG_GiustificativiRepository(ApplicationDbContext dbContext,
                                 IServiceProvider provider,
                                 IHttpContextAccessor httpContextAccessor,
                                 IApplicationDbContextFactory applicationDbContextFactory) : base(applicationDbContextFactory.CreateDbContext(null), 
                                                                                                  provider, 
                                                                                                  httpContextAccessor)
                                
        {
            _applicationDbContextFactory = applicationDbContextFactory;
        }
    }

    public interface IDip_GG_GiustificativiRepository : IRepository<Dip_GG_Giustificativi>
    {
    }
}
