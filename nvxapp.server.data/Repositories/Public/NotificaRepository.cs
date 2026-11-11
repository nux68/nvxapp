using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // servizi comuni (INF-SVC): vedi Entities/Public/ServiziComuni.cs
    public class NotificaRepository : Repository<PublicDbContext, Notifica>, INotificaRepository
    {
        public NotificaRepository(PublicDbContext dbContext,
                                  IServiceProvider provider,
                                  IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface INotificaRepository : IRepository<Notifica>
    {
    }
}
