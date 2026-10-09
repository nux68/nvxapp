using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;


namespace nvxapp.server.data.Repositories.Tenant.GestionePresenze
{


    public class Dip_GG_NotaSpesaRepository : Repository<AttendanceTrackingDbContext, Dip_GG_NotaSpesa>, IDip_GG_NotaSpesaRepository
    {

        public Dip_GG_NotaSpesaRepository(AttendanceTrackingDbContext dbContext,
                                          IServiceProvider provider,
                                          Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDip_GG_NotaSpesaRepository : IRepository<Dip_GG_NotaSpesa>
    {
    }
}
