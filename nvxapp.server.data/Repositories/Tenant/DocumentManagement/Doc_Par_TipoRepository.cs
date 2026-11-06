using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.DocumentManagement;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Interfaces;

namespace nvxapp.server.data.Repositories.Tenant.DocumentManagement
{
    // tabella del documentale (schema tenant_<IdAzienda>_4)
    public class Doc_Par_TipoRepository : Repository<DocumentManagementDbContext, Doc_Par_Tipo>, IDoc_Par_TipoRepository
    {
        public Doc_Par_TipoRepository(DocumentManagementDbContext dbContext,
                      IServiceProvider provider,
                      Microsoft.AspNetCore.Http.IHttpContextAccessor httpContextAccessor)
            : base(dbContext, provider, httpContextAccessor)
        {
        }
    }

    public interface IDoc_Par_TipoRepository : IRepository<Doc_Par_Tipo>
    {
    }
}
