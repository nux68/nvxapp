using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    public class CompanyRegistrationRequestRepository : Repository<PublicDbContext, CompanyRegistrationRequest>, ICompanyRegistrationRequestRepository
    {
        public CompanyRegistrationRequestRepository(PublicDbContext dbContext,
                                                    IServiceProvider provider,
                                                    IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }

        public CompanyRegistrationRequest? FindByTokenHash(string tokenHash)
            => FindAll(x => x.TokenHash == tokenHash).FirstOrDefault();
    }

    public interface ICompanyRegistrationRequestRepository : IRepository<CompanyRegistrationRequest>
    {
        /// <summary>Richiesta a cui appartiene il token del link (per hash).</summary>
        CompanyRegistrationRequest? FindByTokenHash(string tokenHash);
    }
}
