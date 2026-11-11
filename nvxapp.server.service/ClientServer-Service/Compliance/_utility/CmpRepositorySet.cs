using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories.Tenant.Compliance;

namespace nvxapp.server.service.ClientServer_Service.Compliance._utility
{
    /*
     Repository di Compliance su un contesto creato dalla factory per lo schema di un'azienda, fuori da una richiesta HTTP
     (cancellazione fisica): anche li' i dati si leggono e scrivono con i repository.
     Il contesto lo possiede chi lo ha creato (await using).
    */
    public sealed class CmpRepositorySet
    {
        private readonly ComplianceDbContext _db;
        private readonly IServiceProvider _sp;
        private readonly IHttpContextAccessor _http;

        public CmpRepositorySet(ComplianceDbContext db, IServiceProvider sp, IHttpContextAccessor http)
        {
            _db = db;
            _sp = sp;
            _http = http;
        }

        public ICmp_CfgRepository Cmp_Cfg => _Cmp_Cfg ??= new Cmp_CfgRepository(_db, _sp, _http);
        private ICmp_CfgRepository? _Cmp_Cfg;
        public ICmp_Az_AnagraficaReadRepository Az_Anagrafica => _Az_Anagrafica ??= new Cmp_Az_AnagraficaReadRepository(_db, _sp, _http);
        private ICmp_Az_AnagraficaReadRepository? _Az_Anagrafica;
    }
}
