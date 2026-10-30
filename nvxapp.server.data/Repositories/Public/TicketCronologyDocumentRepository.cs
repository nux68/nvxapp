using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    public class TicketCronologyDocumentRepository : Repository<PublicDbContext, TicketCronologyDocument>, ITicketCronologyDocumentRepository
    {
        public TicketCronologyDocumentRepository(PublicDbContext dbContext,
                              IServiceProvider provider,
                              IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ITicketCronologyDocumentRepository : IRepository<TicketCronologyDocument>
    {
    }
}
