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


    public class UserCompanyRepository : Repository<PublicDbContext, UserCompany>, IUserCompanyRepository
    {
        public UserCompanyRepository(PublicDbContext dbContext,
                                     IServiceProvider provider,
                                     IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }
    }
    public interface IUserCompanyRepository : IRepository<UserCompany>
    {

    }

}
