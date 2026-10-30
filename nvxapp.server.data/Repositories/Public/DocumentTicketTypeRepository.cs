using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    public class DocumentTicketTypeRepository : Repository<PublicDbContext, DocumentTicketType>, IDocumentTicketTypeRepository
    {
        public DocumentTicketTypeRepository(PublicDbContext dbContext,
                              IServiceProvider provider,
                              IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDocumentTicketTypeRepository : IRepository<DocumentTicketType>
    {
    }
}
