using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // ripartizione degli incassi ai centri (ANALISI_PAGAMENTI_CENTRI.md)
    public class Pag_InvioEmailRepository : Repository<PublicDbContext, Pag_InvioEmail>, IPag_InvioEmailRepository
    {
        public Pag_InvioEmailRepository(PublicDbContext dbContext, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPag_InvioEmailRepository : IRepository<Pag_InvioEmail>
    {
    }
}
