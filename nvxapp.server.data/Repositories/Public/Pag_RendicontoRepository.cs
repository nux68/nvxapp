using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // ripartizione degli incassi ai centri (ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md)
    public class Pag_RendicontoRepository : Repository<PublicDbContext, Pag_Rendiconto>, IPag_RendicontoRepository
    {
        public Pag_RendicontoRepository(PublicDbContext dbContext, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPag_RendicontoRepository : IRepository<Pag_Rendiconto>
    {
    }
}
