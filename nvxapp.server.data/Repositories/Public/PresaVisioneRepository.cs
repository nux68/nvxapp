using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // servizi comuni (INF-SVC): vedi Entities/Public/ServiziComuni.cs
    public class PresaVisioneRepository : Repository<PublicDbContext, PresaVisione>, IPresaVisioneRepository
    {
        public PresaVisioneRepository(PublicDbContext dbContext,
                                      IServiceProvider provider,
                                      IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IPresaVisioneRepository : IRepository<PresaVisione>
    {
    }
}
