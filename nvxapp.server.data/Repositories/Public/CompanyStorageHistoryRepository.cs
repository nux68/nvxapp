using Microsoft.AspNetCore.Http;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    // fotografie mensili dello spazio delle aziende
    public class CompanyStorageHistoryRepository : Repository<PublicDbContext, CompanyStorageHistory>, ICompanyStorageHistoryRepository
    {
        public CompanyStorageHistoryRepository(PublicDbContext dbContext, IServiceProvider provider, IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface ICompanyStorageHistoryRepository : IRepository<CompanyStorageHistory>
    {
    }
}
