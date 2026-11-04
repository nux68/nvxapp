using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    public class FinancialAdvisorApplicationRepository : Repository<PublicDbContext, FinancialAdvisorApplication>, IFinancialAdvisorApplicationRepository
    {
        public FinancialAdvisorApplicationRepository(PublicDbContext dbContext,
                                                     IServiceProvider provider,
                                                     IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }

        public List<ApplicationType> ActiveApplications(int idFinancialAdvisor)
            => FindAll(x => x.IdFinancialAdvisor == idFinancialAdvisor && x.Active)
                   .Select(x => x.ApplicationType)
                   .OrderBy(x => x)
                   .ToList();
    }

    public interface IFinancialAdvisorApplicationRepository : IRepository<FinancialAdvisorApplication>
    {
        /// <summary>Applicativi abilitati dello studio.</summary>
        List<ApplicationType> ActiveApplications(int idFinancialAdvisor);
    }
}
