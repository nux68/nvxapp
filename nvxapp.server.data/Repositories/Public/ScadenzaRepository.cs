using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // servizi comuni (INF-SVC): vedi Entities/Public/ServiziComuni.cs
    public class ScadenzaRepository : Repository<PublicDbContext, Scadenza>, IScadenzaRepository
    {
        public ScadenzaRepository(PublicDbContext dbContext,
                                  IServiceProvider provider,
                                  IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IScadenzaRepository : IRepository<Scadenza>
    {
    }
}
