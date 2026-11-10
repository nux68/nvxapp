using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Public
{
    public class UserDashboardRepository : Repository<PublicDbContext, UserDashboard>, IUserDashboardRepository
    {
        public UserDashboardRepository(PublicDbContext dbContext,
                                       IServiceProvider provider,
                                       IHttpContextAccessor httpContextAccessor) : base(dbContext, provider, httpContextAccessor)
        {
        }

        public UserDashboard? Dashboard(string idAspNetUsers, int idCompany)
            => FindAll(x => x.IdAspNetUsers == idAspNetUsers && x.IdCompany == idCompany).FirstOrDefault();
    }

    public interface IUserDashboardRepository : IRepository<UserDashboard>
    {
        /// <summary>Dashboard dell'utente nell'azienda (null se non l'ha ancora configurata).</summary>
        UserDashboard? Dashboard(string idAspNetUsers, int idCompany);
    }
}
