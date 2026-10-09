using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Dip_AnagraficaRepository : Repository<AttendanceTrackingDbContext, Dip_Anagrafica>, IDip_AnagraficaRepository
    {

        public Dip_AnagraficaRepository(AttendanceTrackingDbContext dbContext,
                                        IServiceProvider provider,
                                        Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_AnagraficaRepository : IRepository<Dip_Anagrafica>
    {
    }
}
