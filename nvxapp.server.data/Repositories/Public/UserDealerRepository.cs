using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Repositories.Public
{


    public class UserDealerRepository : Repository<ApplicationDbContext, UserDealer>, IUserDealerRepository
    {
        public UserDealerRepository(ApplicationDbContext dbContext,
                                     IServiceProvider provider,
                                     IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }
    }
    public interface IUserDealerRepository : IRepository<UserDealer>
    {

    }

}
