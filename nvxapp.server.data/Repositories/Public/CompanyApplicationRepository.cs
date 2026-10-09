using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    public class CompanyApplicationRepository : Repository<PublicDbContext, CompanyApplication>, ICompanyApplicationRepository
    {
        public CompanyApplicationRepository(PublicDbContext dbContext,
                                            IServiceProvider provider,
                                            IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }

        public List<ApplicationType> ActiveApplications(int idCompany)
            => FindAll(x => x.IdCompany == idCompany && x.Active)
                   .Select(x => x.ApplicationType)
                   .OrderBy(x => x)
                   .ToList();
    }

    public interface ICompanyApplicationRepository : IRepository<CompanyApplication>
    {
        /// <summary>Applicativi attivi dell'azienda.</summary>
        List<ApplicationType> ActiveApplications(int idCompany);
    }
}
