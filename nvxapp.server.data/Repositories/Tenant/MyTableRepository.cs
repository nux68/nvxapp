using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant
{



    public class MyTableRepository : Repository<ApplicationDbContext, MyTable>, IMyTableRepository
    {

        private readonly IApplicationDbContextFactory _applicationDbContextFactory;

        public MyTableRepository(ApplicationDbContext dbContext,
                                 IServiceProvider provider,
                                 IHttpContextAccessor httpContextAccessor,
                                 IApplicationDbContextFactory applicationDbContextFactory) : base(applicationDbContextFactory.CreateDbContext(null), 
                                                                                                  provider, 
                                                                                                  httpContextAccessor)
                                
        {
            _applicationDbContextFactory = applicationDbContextFactory;

            var schema = CurrentTenat;
           
        }
    }

    public interface IMyTableRepository : IRepository<MyTable>
    {
    }
}
