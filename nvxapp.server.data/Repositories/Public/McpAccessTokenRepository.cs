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

        // "lavora come" / "torna indietro": cambia l'utente di lavoro del token, non chi lo possiede
        public async Task ImpostaIdentitaAsync(int id, McpHomeIdentity identita, string? homeIdentity, DateTime? scadenza)
        {
            await DbContext.Set<McpAccessToken>().Where(x => x.Id == id)
                           .ExecuteUpdateAsync(s => s.SetProperty(x => x.IdAspNetUsers, identita.IdAspNetUsers)
                                                     .SetProperty(x => x.IdDealer, identita.IdDealer)
                                                     .SetProperty(x => x.IdFinancialAdvisor, identita.IdFinancialAdvisor)
                                                     .SetProperty(x => x.IdCompany, identita.IdCompany)
                                                     .SetProperty(x => x.Tenant, identita.Tenant)
                                                     .SetProperty(x => x.HomeIdentity, homeIdentity)
                                                     .SetProperty(x => x.ImpersonationExpiresAt, scadenza));
        }
    }

    public interface IMcpAccessTokenRepository : IRepository<McpAccessToken>
    {
        /// <summary>registra l'ultimo uso del token</summary>
        Task SegnaUsoAsync(int id, DateTime quando);
        /// <summary>imposta l'utente di lavoro del token (impersonificazione tramite strumenti MCP); homeIdentity null = nessuna impersonificazione</summary>
        Task ImpostaIdentitaAsync(int id, McpHomeIdentity identita, string? homeIdentity, DateTime? scadenza);
    }
}
