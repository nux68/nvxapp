using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // accesso AI dell'azienda (ANALISI_INFRASTRUCTURE_MCP.md): vedi Entities/Public/Mcp.cs
    public class McpCompanySettingRepository : Repository<PublicDbContext, McpCompanySetting>, IMcpCompanySettingRepository
    {
        public McpCompanySettingRepository(PublicDbContext dbContext,
                                           IServiceProvider provider,
                                           IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }

        public Task<McpCompanySetting?> OfCompanyAsync(int idCompany)
            => FindAll(x => x.IdCompany == idCompany).FirstOrDefaultAsync();
    }

    public interface IMcpCompanySettingRepository : IRepository<McpCompanySetting>
    {
        /// <summary>impostazione dell'azienda; null = mai impostata (accesso AI spento)</summary>
        Task<McpCompanySetting?> OfCompanyAsync(int idCompany);
    }
}
