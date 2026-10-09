using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data;
using nvxapp.server.data.Repositories;
using nvxapp.server.data.Interfaces;
using nvxapp.server.data.Infrastructure;
using System;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public interface IAz_SubCommessaSediRepartoRepository : IRepository<Az_SubCommessaSediReparto>
    {
    }

    public class Az_SubCommessaSediRepartoRepository : Repository<TenantDbContext, Az_SubCommessaSediReparto>, IAz_SubCommessaSediRepartoRepository
    {
        public Az_SubCommessaSediRepartoRepository(TenantDbContext dbContext,
                                                   IServiceProvider provider,
                                                   Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }
}
