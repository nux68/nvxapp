using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data;
using nvxapp.server.data.Repositories;
using nvxapp.server.data.Repositories.Tenant;
using nvxapp.server.data.Repositories.Public;
using System;
using nvxapp.server.data.Interfaces;
using nvxapp.server.data.Infrastructure;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public interface IMy_template1Repository : IRepository<My_template1>
    {
    }

    public class My_template1Repository : Repository<ApplicationDbContext, My_template1>, IMy_template1Repository
    {
        public My_template1Repository(ApplicationDbContext context, IServiceProvider serviceProvider, Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(context, serviceProvider, httpContextAccessor)
        {
        }
    }
}
