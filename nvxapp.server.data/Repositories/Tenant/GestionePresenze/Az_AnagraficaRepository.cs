using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Az_AnagraficaRepository : Repository<TenantDbContext, Az_Anagrafica>, IAz_AnagraficaRepository
    {

        public Az_AnagraficaRepository(TenantDbContext dbContext,
                                       IServiceProvider provider,
                                       Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_AnagraficaRepository : IRepository<Az_Anagrafica>
    {
    }
}
