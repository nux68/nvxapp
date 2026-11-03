using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.Common
{
    // tabella comune dell'azienda (schema Common): scritture solo da qui
    public class Az_AnagraficaRepository : Repository<CommonDbContext, Az_Anagrafica>, IAz_AnagraficaRepository
    {
        public Az_AnagraficaRepository(CommonDbContext dbContext,
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
