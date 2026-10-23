using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    public class TicketTypeLevelRepository : Repository<PublicDbContext, TicketTypeLevel>, ITicketTypeLevelRepository
    {
        public TicketTypeLevelRepository(PublicDbContext dbContext,
                              IServiceProvider provider,
                              IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ITicketTypeLevelRepository : IRepository<TicketTypeLevel>
    {
    }
}
