using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_GG_NotaSpesa_AllegatoRepository : Repository<AttendanceTrackingDbContext, Dip_GG_NotaSpesa_Allegato>, IDip_GG_NotaSpesa_AllegatoRepository
    {
        public Dip_GG_NotaSpesa_AllegatoRepository(AttendanceTrackingDbContext dbContext,
                              IServiceProvider provider,
                              Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_GG_NotaSpesa_AllegatoRepository : IRepository<Dip_GG_NotaSpesa_Allegato>
    {
    }
}
