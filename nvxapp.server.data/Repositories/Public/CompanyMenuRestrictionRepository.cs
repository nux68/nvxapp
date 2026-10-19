using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    public class CompanyMenuRestrictionRepository : Repository<PublicDbContext, CompanyMenuRestriction>, ICompanyMenuRestrictionRepository
    {
        public CompanyMenuRestrictionRepository(PublicDbContext dbContext,
                                                IServiceProvider provider,
                                                IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }

        public List<CompanyMenuRestriction> Restrictions(int idCompany)
            => FindAll(x => x.IdCompany == idCompany)
                   .OrderBy(x => x.Url).ThenBy(x => x.Role)
                   .ToList();
    }

    public interface ICompanyMenuRestrictionRepository : IRepository<CompanyMenuRestriction>
    {
        /// <summary>Voci di menu nascoste ai ruoli dell'azienda.</summary>
        List<CompanyMenuRestriction> Restrictions(int idCompany);
    }
}
