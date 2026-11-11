using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories.Tenant.Assets;

namespace nvxapp.server.service.ClientServer_Service.Assets._utility
{
    /*
     Repository di Risorse aziendali su un contesto creato dalla factory per lo schema di un'azienda, fuori da una richiesta HTTP
     (cancellazione fisica, controllo delle cessazioni, feed del calendario): anche li' i dati si leggono e scrivono con i repository.
     Il contesto lo possiede chi lo ha creato (await using).
    */
    public sealed class AstRepositorySet
    {
        private readonly AssetsDbContext _db;
        private readonly IServiceProvider _sp;
        private readonly IHttpContextAccessor _http;

        public AstRepositorySet(AssetsDbContext db, IServiceProvider sp, IHttpContextAccessor http)
        {
            _db = db;
            _sp = sp;
            _http = http;
        }

        public IAst_CfgRepository Ast_Cfg => _Ast_Cfg ??= new Ast_CfgRepository(_db, _sp, _http);
        private IAst_CfgRepository? _Ast_Cfg;
        public IAst_Az_AnagraficaReadRepository Az_Anagrafica => _Az_Anagrafica ??= new Ast_Az_AnagraficaReadRepository(_db, _sp, _http);
        private IAst_Az_AnagraficaReadRepository? _Az_Anagrafica;
        public IAst_Dip_AnagraficaReadRepository Dip_Anagrafica => _Dip_Anagrafica ??= new Ast_Dip_AnagraficaReadRepository(_db, _sp, _http);
        private IAst_Dip_AnagraficaReadRepository? _Dip_Anagrafica;

        public IAst_VeicoloRepository Ast_Veicolo => _Ast_Veicolo ??= new Ast_VeicoloRepository(_db, _sp, _http);
        private IAst_VeicoloRepository? _Ast_Veicolo;
        public IAst_VeicoloAssegnazioneRepository Ast_VeicoloAssegnazione => _Ast_VeicoloAssegnazione ??= new Ast_VeicoloAssegnazioneRepository(_db, _sp, _http);
        private IAst_VeicoloAssegnazioneRepository? _Ast_VeicoloAssegnazione;
    }
}
