using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // licenze sottoscritte da aziende e studi (ANALISI_INFRASTRUCTURE_LICENZE.md §3.2)
    public class Lic_AbbonamentoRepository : Repository<PublicDbContext, Lic_Abbonamento>, ILic_AbbonamentoRepository
    {
        public Lic_AbbonamentoRepository(PublicDbContext dbContext, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ILic_AbbonamentoRepository : IRepository<Lic_Abbonamento>
    {
    }
}
