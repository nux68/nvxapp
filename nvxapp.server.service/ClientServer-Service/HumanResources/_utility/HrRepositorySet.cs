using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories.Tenant.HumanResources;

namespace nvxapp.server.service.ClientServer_Service.HumanResources._utility
{
    /*
     Repository di Risorse Umane su un contesto creato dalla factory per lo schema di un'azienda, fuori da una richiesta HTTP
     (cancellazione fisica): anche li' i dati si leggono e scrivono con i repository.
     Il contesto lo possiede chi lo ha creato (await using).
    */
    public sealed class HrRepositorySet
    {
        private readonly HumanResourcesDbContext _db;
        private readonly IServiceProvider _sp;
        private readonly IHttpContextAccessor _http;

        public HrRepositorySet(HumanResourcesDbContext db, IServiceProvider sp, IHttpContextAccessor http)
        {
            _db = db;
            _sp = sp;
            _http = http;
        }

        public IHr_CfgRepository Hr_Cfg => _Hr_Cfg ??= new Hr_CfgRepository(_db, _sp, _http);
        private IHr_CfgRepository? _Hr_Cfg;
        public IHr_Az_AnagraficaReadRepository Az_Anagrafica => _Az_Anagrafica ??= new Hr_Az_AnagraficaReadRepository(_db, _sp, _http);
        private IHr_Az_AnagraficaReadRepository? _Az_Anagrafica;
    }
}
