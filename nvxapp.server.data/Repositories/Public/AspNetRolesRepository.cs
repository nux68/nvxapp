using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{


    public class AspNetRolesRepository : Repository<ApplicationDbContext, ApplicationRole>, IAspNetRolesRepository
    {
        public AspNetRolesRepository(ApplicationDbContext dbContext,
                                     IServiceProvider provider,
                                     IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }
    }
    public interface IAspNetRolesRepository : IRepository<ApplicationRole>
    {

    }

}
