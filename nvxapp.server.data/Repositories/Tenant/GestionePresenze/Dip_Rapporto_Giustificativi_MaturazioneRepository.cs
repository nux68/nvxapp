using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{
    public class Dip_Rapporto_Giustificativi_MaturazioneRepository
        : Repository<AttendanceTrackingDbContext, Dip_Rapporto_Giustificativi_Maturazione>,
          IDip_Rapporto_Giustificativi_MaturazioneRepository
    {
        public Dip_Rapporto_Giustificativi_MaturazioneRepository(AttendanceTrackingDbContext dbContext,
                                                                 IServiceProvider provider,
                                                                 Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_Rapporto_Giustificativi_MaturazioneRepository
        : IRepository<Dip_Rapporto_Giustificativi_Maturazione>
    {
    }
}
