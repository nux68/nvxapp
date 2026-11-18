using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // token personali MCP (ANALISI_INFRASTRUCTURE_MCP.md): vedi Entities/Public/Mcp.cs
    public class McpAccessTokenRepository : Repository<PublicDbContext, McpAccessToken>, IMcpAccessTokenRepository
    {
        public McpAccessTokenRepository(PublicDbContext dbContext,
                                        IServiceProvider provider,
                                        IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }

        // la richiesta MCP si autentica con il token stesso: UPDATE diretto, senza toccare data e utente della modifica
        public async Task SegnaUsoAsync(int id, DateTime quando)
        {
            await DbContext.Set<McpAccessToken>().Where(x => x.Id == id)
                           .ExecuteUpdateAsync(s => s.SetProperty(x => x.LastUsedAt, quando));
        }
    }

    public interface IMcpAccessTokenRepository : IRepository<McpAccessToken>
    {
        /// <summary>registra l'ultimo uso del token</summary>
        Task SegnaUsoAsync(int id, DateTime quando);
    }
}
