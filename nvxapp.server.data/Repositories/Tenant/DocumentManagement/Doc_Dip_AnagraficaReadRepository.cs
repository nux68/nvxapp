using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.DocumentManagement
{
    // Dip_Anagrafica (tabella comune) nel contesto del documentale: sola lettura, le scritture passano dai repository Common
    public class Doc_Dip_AnagraficaReadRepository : Repository<DocumentManagementDbContext, Dip_Anagrafica>, IDoc_Dip_AnagraficaReadRepository
    {
        public Doc_Dip_AnagraficaReadRepository(DocumentManagementDbContext dbContext,
                      IServiceProvider provider,
                      Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDoc_Dip_AnagraficaReadRepository : IRepository<Dip_Anagrafica>
    {
    }
}
