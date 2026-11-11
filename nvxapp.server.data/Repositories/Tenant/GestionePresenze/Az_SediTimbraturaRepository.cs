using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Az_SediTimbraturaRepository : Repository<AttendanceTrackingDbContext, Az_SediTimbratura>, IAz_SediTimbraturaRepository
    {
        public Az_SediTimbraturaRepository(AttendanceTrackingDbContext dbContext,
                             IServiceProvider provider,
                             Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IAz_SediTimbraturaRepository : IRepository<Az_SediTimbratura>
    {
    }
}
