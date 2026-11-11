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
        public ICmp_MansioneRepository Cmp_Mansione => _Cmp_Mansione ??= new Cmp_MansioneRepository(_db, _sp, _http);
        private ICmp_MansioneRepository? _Cmp_Mansione;
        public ICmp_MansioneCorsoRepository Cmp_MansioneCorso => _Cmp_MansioneCorso ??= new Cmp_MansioneCorsoRepository(_db, _sp, _http);
        private ICmp_MansioneCorsoRepository? _Cmp_MansioneCorso;
        public ICmp_DipendenteMansioneRepository Cmp_DipendenteMansione => _Cmp_DipendenteMansione ??= new Cmp_DipendenteMansioneRepository(_db, _sp, _http);
        private ICmp_DipendenteMansioneRepository? _Cmp_DipendenteMansione;
        public ICmp_FormazioneRepository Cmp_Formazione => _Cmp_Formazione ??= new Cmp_FormazioneRepository(_db, _sp, _http);
        private ICmp_FormazioneRepository? _Cmp_Formazione;
        public ICmp_VisitaRepository Cmp_Visita => _Cmp_Visita ??= new Cmp_VisitaRepository(_db, _sp, _http);
        private ICmp_VisitaRepository? _Cmp_Visita;
        public ICmp_DpiRepository Cmp_Dpi => _Cmp_Dpi ??= new Cmp_DpiRepository(_db, _sp, _http);
        private ICmp_DpiRepository? _Cmp_Dpi;
        public ICmp_DpiConsegnaRepository Cmp_DpiConsegna => _Cmp_DpiConsegna ??= new Cmp_DpiConsegnaRepository(_db, _sp, _http);
        private ICmp_DpiConsegnaRepository? _Cmp_DpiConsegna;
        public ICmp_NominaRepository Cmp_Nomina => _Cmp_Nomina ??= new Cmp_NominaRepository(_db, _sp, _http);
        private ICmp_NominaRepository? _Cmp_Nomina;
        public ICmp_DocumentoRepository Cmp_Documento => _Cmp_Documento ??= new Cmp_DocumentoRepository(_db, _sp, _http);
        private ICmp_DocumentoRepository? _Cmp_Documento;
        public ICmp_TrattamentoRepository Cmp_Trattamento => _Cmp_Trattamento ??= new Cmp_TrattamentoRepository(_db, _sp, _http);
        private ICmp_TrattamentoRepository? _Cmp_Trattamento;
        public ICmp_ResponsabileRepository Cmp_Responsabile => _Cmp_Responsabile ??= new Cmp_ResponsabileRepository(_db, _sp, _http);
        private ICmp_ResponsabileRepository? _Cmp_Responsabile;
        public ICmp_ViolazioneRepository Cmp_Violazione => _Cmp_Violazione ??= new Cmp_ViolazioneRepository(_db, _sp, _http);
        private ICmp_ViolazioneRepository? _Cmp_Violazione;
        public ICmp_Az_AnagraficaReadRepository Az_Anagrafica => _Az_Anagrafica ??= new Cmp_Az_AnagraficaReadRepository(_db, _sp, _http);
        private ICmp_Az_AnagraficaReadRepository? _Az_Anagrafica;
    }
}
