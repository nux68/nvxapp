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
        public MyTableRepository(ApplicationDbContext dbContext, IServiceProvider provider) : base(dbContext, provider)
        {
        }
    }

    public interface IMyTableRepository : IRepository<MyTable>
    {
    }
}
