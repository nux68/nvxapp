using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_GG_TimbraturaInfoRepository : Repository<AttendanceTrackingDbContext, Dip_GG_TimbraturaInfo>, IDip_GG_TimbraturaInfoRepository
    {
        public Dip_GG_TimbraturaInfoRepository(AttendanceTrackingDbContext dbContext,
                             IServiceProvider provider,
                             Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_GG_TimbraturaInfoRepository : IRepository<Dip_GG_TimbraturaInfo>
    {
    }
}
