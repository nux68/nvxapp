using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // listini delle licenze (ANALISI_INFRASTRUCTURE_LICENZE.md §3.1)
    public class Lic_ListinoRepository : Repository<PublicDbContext, Lic_Listino>, ILic_ListinoRepository
    {
        public Lic_ListinoRepository(PublicDbContext dbContext, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ILic_ListinoRepository : IRepository<Lic_Listino>
    {
    }
}
