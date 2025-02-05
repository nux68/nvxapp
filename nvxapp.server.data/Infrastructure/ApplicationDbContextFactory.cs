using Microsoft.EntityFrameworkCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace nvxapp.server.data.Infrastructure
{
  

    public class ApplicationDbContextFactory : IApplicationDbContextFactory
    {
        private readonly DbContextOptions<ApplicationDbContext> _options;

        public ApplicationDbContextFactory(DbContextOptions<ApplicationDbContext> options)
        {
            _options = options;
        }

        public ApplicationDbContext CreateDbContext(string tenant)
        {
            return new ApplicationDbContext(_options, tenant);
        }
    }

    public interface IApplicationDbContextFactory
    {
        ApplicationDbContext CreateDbContext(string tenant);
    }

}
