using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.DocumentManagement
{
    // tabella del documentale (schema tenant_<IdAzienda>_4)
    public class Doc_CartellaRepository : Repository<DocumentManagementDbContext, Doc_Cartella>, IDoc_CartellaRepository
    {
        public Doc_CartellaRepository(DocumentManagementDbContext dbContext,
                      IServiceProvider provider,
                      Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDoc_CartellaRepository : IRepository<Doc_Cartella>
    {
    }
}
