using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // registro delle eliminazioni fisiche
    public class DeletionLogRepository : Repository<PublicDbContext, DeletionLog>, IDeletionLogRepository
    {
        public DeletionLogRepository(PublicDbContext dbContext, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDeletionLogRepository : IRepository<DeletionLog>
    {
    }
}
