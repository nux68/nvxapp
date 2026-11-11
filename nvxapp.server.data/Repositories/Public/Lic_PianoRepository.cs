using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // piani dei listini delle licenze, uno per livello (ANALISI_INFRASTRUCTURE_LICENZE.md §3.1)
    public class Lic_PianoRepository : Repository<PublicDbContext, Lic_Piano>, ILic_PianoRepository
    {
        public Lic_PianoRepository(PublicDbContext dbContext, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ILic_PianoRepository : IRepository<Lic_Piano>
    {
    }
}
