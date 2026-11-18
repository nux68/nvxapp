using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // registro delle chiamate MCP (ANALISI_INFRASTRUCTURE_MCP.md): vedi Entities/Public/Mcp.cs
    public class McpCallLogRepository : Repository<PublicDbContext, McpCallLog>, IMcpCallLogRepository
    {
        public McpCallLogRepository(PublicDbContext dbContext,
                                    IServiceProvider provider,
                                    IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IMcpCallLogRepository : IRepository<McpCallLog>
    {
    }
}
