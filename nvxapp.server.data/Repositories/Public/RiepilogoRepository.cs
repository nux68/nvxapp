using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // riepilogo AI periodico (INF-AI): vedi Entities/Public/Riepilogo.cs
    public class RiepilogoRepository : Repository<PublicDbContext, Riepilogo>, IRiepilogoRepository
    {
        public RiepilogoRepository(PublicDbContext dbContext,
                                   IServiceProvider provider,
                                   IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IRiepilogoRepository : IRepository<Riepilogo>
    {
    }
}
