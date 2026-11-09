using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // pagamenti delle licenze (ANALISI_LICENZE.md §3.2)
    public class Lic_PagamentoRepository : Repository<PublicDbContext, Lic_Pagamento>, ILic_PagamentoRepository
    {
        public Lic_PagamentoRepository(PublicDbContext dbContext, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ILic_PagamentoRepository : IRepository<Lic_Pagamento>
    {
    }
}
