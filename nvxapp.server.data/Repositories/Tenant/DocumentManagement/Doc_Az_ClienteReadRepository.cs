using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.DocumentManagement
{
    // Az_Cliente (tabella comune) nel contesto del documentale: sola lettura, le scritture passano dai repository Common
    public class Doc_Az_ClienteReadRepository : Repository<DocumentManagementDbContext, Az_Cliente>, IDoc_Az_ClienteReadRepository
    {
        public Doc_Az_ClienteReadRepository(DocumentManagementDbContext dbContext,
                      IServiceProvider provider,
                      Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDoc_Az_ClienteReadRepository : IRepository<Az_Cliente>
    {
    }
}
