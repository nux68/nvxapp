using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.DocumentManagement
{
    // Az_Anagrafica (tabella comune) nel contesto del documentale: sola lettura, le scritture passano dai repository Common
    public class Doc_Az_AnagraficaReadRepository : Repository<DocumentManagementDbContext, Az_Anagrafica>, IDoc_Az_AnagraficaReadRepository
    {
        public Doc_Az_AnagraficaReadRepository(DocumentManagementDbContext dbContext,
                      IServiceProvider provider,
                      Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDoc_Az_AnagraficaReadRepository : IRepository<Az_Anagrafica>
    {
    }
}
