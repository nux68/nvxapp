using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // ripartizione degli incassi ai centri (ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md)
    public class Pag_DealerConfigRepository : Repository<PublicDbContext, Pag_DealerConfig>, IPag_DealerConfigRepository
    {
        public Pag_DealerConfigRepository(PublicDbContext dbContext, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPag_DealerConfigRepository : IRepository<Pag_DealerConfig>
    {
    }
}
