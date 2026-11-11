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
        public IHr_DocCaricamentoRepository Hr_DocCaricamento => _Hr_DocCaricamento ??= new Hr_DocCaricamentoRepository(_db, _sp, _http);
        private IHr_DocCaricamentoRepository? _Hr_DocCaricamento;
        public IHr_DocDipendenteRepository Hr_DocDipendente => _Hr_DocDipendente ??= new Hr_DocDipendenteRepository(_db, _sp, _http);
        private IHr_DocDipendenteRepository? _Hr_DocDipendente;
        public IHr_DocScartoRepository Hr_DocScarto => _Hr_DocScarto ??= new Hr_DocScartoRepository(_db, _sp, _http);
        private IHr_DocScartoRepository? _Hr_DocScarto;
        public IHr_CorsoRepository Hr_Corso => _Hr_Corso ??= new Hr_CorsoRepository(_db, _sp, _http);
        private IHr_CorsoRepository? _Hr_Corso;
        public IHr_CorsoEdizioneRepository Hr_CorsoEdizione => _Hr_CorsoEdizione ??= new Hr_CorsoEdizioneRepository(_db, _sp, _http);
        private IHr_CorsoEdizioneRepository? _Hr_CorsoEdizione;
        public IHr_CorsoPartecipanteRepository Hr_CorsoPartecipante => _Hr_CorsoPartecipante ??= new Hr_CorsoPartecipanteRepository(_db, _sp, _http);
        private IHr_CorsoPartecipanteRepository? _Hr_CorsoPartecipante;
        public IHr_PianoFormazioneRepository Hr_PianoFormazione => _Hr_PianoFormazione ??= new Hr_PianoFormazioneRepository(_db, _sp, _http);
        private IHr_PianoFormazioneRepository? _Hr_PianoFormazione;
        public IHr_PianoFormazioneRigaRepository Hr_PianoFormazioneRiga => _Hr_PianoFormazioneRiga ??= new Hr_PianoFormazioneRigaRepository(_db, _sp, _http);
        private IHr_PianoFormazioneRigaRepository? _Hr_PianoFormazioneRiga;
        public IHr_ChecklistModelloRepository Hr_ChecklistModello => _Hr_ChecklistModello ??= new Hr_ChecklistModelloRepository(_db, _sp, _http);
        private IHr_ChecklistModelloRepository? _Hr_ChecklistModello;
        public IHr_ChecklistModelloVoceRepository Hr_ChecklistModelloVoce => _Hr_ChecklistModelloVoce ??= new Hr_ChecklistModelloVoceRepository(_db, _sp, _http);
        private IHr_ChecklistModelloVoceRepository? _Hr_ChecklistModelloVoce;
        public IHr_ChecklistRepository Hr_Checklist => _Hr_Checklist ??= new Hr_ChecklistRepository(_db, _sp, _http);
        private IHr_ChecklistRepository? _Hr_Checklist;
        public IHr_ChecklistVoceRepository Hr_ChecklistVoce => _Hr_ChecklistVoce ??= new Hr_ChecklistVoceRepository(_db, _sp, _http);
        private IHr_ChecklistVoceRepository? _Hr_ChecklistVoce;
    }
}
