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

    public class Dip_RapportoLavoroRepository : Repository<ApplicationDbContext, Dip_RapportoLavoro>, IDip_RapportoLavoroRepository, ICurrentTenant
    {

        private readonly IApplicationDbContextFactory _applicationDbContextFactory;



        public Dip_RapportoLavoroRepository(ApplicationDbContext dbContext,
                                 IServiceProvider provider,
                                 IHttpContextAccessor httpContextAccessor,
                                 IApplicationDbContextFactory applicationDbContextFactory) : base(applicationDbContextFactory.CreateDbContext(null), 
                                                                                                  provider, 
                                                                                                  httpContextAccessor)
                                
        {
            _applicationDbContextFactory = applicationDbContextFactory;
        }
    }

    public interface IDip_RapportoLavoroRepository : IRepository<Dip_RapportoLavoro>
    {
    }
}
