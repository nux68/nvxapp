using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Repositories
{

   

    public class MyTableRepository : Repository<ApplicationDbContext, MyTable>, IMyTableRepository
    {

        private readonly IApplicationDbContextFactory _applicationDbContextFactory;

        public MyTableRepository(ApplicationDbContext dbContext, 
                                 IServiceProvider provider,
                                 IHttpContextAccessor httpContextAccessor,
                                 IApplicationDbContextFactory applicationDbContextFactory) : base(dbContext, provider, httpContextAccessor)
        {
            _applicationDbContextFactory = applicationDbContextFactory;

            var schema = this.CurrentTenat;

            this.DbContext = applicationDbContextFactory.CreateDbContext(schema);
            //this.DbContext.ChangeSchema();
        }
    }

    public interface IMyTableRepository : IRepository<MyTable>
    {
    }
}
