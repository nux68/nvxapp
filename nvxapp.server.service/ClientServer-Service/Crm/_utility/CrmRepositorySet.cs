using Microsoft.AspNetCore.Http;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories.Tenant.Crm;

namespace nvxapp.server.service.ClientServer_Service.Crm._utility
{
    /*
     Repository del CRM su un contesto creato dalla factory per lo schema di un'azienda, fuori da una richiesta HTTP
     (cancellazione fisica, uso dei clienti comuni, promemoria): anche li' i dati si leggono e scrivono con i repository.
     Il contesto lo possiede chi lo ha creato (await using).
    */
    public sealed class CrmRepositorySet
    {
        private readonly CrmDbContext _db;
        private readonly IServiceProvider _sp;
        private readonly IHttpContextAccessor _http;

        public CrmRepositorySet(CrmDbContext db, IServiceProvider sp, IHttpContextAccessor http)
        {
            _db = db;
            _sp = sp;
            _http = http;
        }

        public ICrm_CfgRepository Crm_Cfg => _Crm_Cfg ??= new Crm_CfgRepository(_db, _sp, _http);
        private ICrm_CfgRepository? _Crm_Cfg;
        public ICrm_Par_FaseRepository Crm_Par_Fase => _Crm_Par_Fase ??= new Crm_Par_FaseRepository(_db, _sp, _http);
        private ICrm_Par_FaseRepository? _Crm_Par_Fase;
        public ICrm_Par_TipoAttivitaRepository Crm_Par_TipoAttivita => _Crm_Par_TipoAttivita ??= new Crm_Par_TipoAttivitaRepository(_db, _sp, _http);
        private ICrm_Par_TipoAttivitaRepository? _Crm_Par_TipoAttivita;
        public ICrm_Par_MotivoPerditaRepository Crm_Par_MotivoPerdita => _Crm_Par_MotivoPerdita ??= new Crm_Par_MotivoPerditaRepository(_db, _sp, _http);
        private ICrm_Par_MotivoPerditaRepository? _Crm_Par_MotivoPerdita;
        public ICrm_Par_FonteRepository Crm_Par_Fonte => _Crm_Par_Fonte ??= new Crm_Par_FonteRepository(_db, _sp, _http);
        private ICrm_Par_FonteRepository? _Crm_Par_Fonte;
        public ICrm_Par_ProdottoRepository Crm_Par_Prodotto => _Crm_Par_Prodotto ??= new Crm_Par_ProdottoRepository(_db, _sp, _http);
        private ICrm_Par_ProdottoRepository? _Crm_Par_Prodotto;
        public ICrm_Par_TagRepository Crm_Par_Tag => _Crm_Par_Tag ??= new Crm_Par_TagRepository(_db, _sp, _http);
        private ICrm_Par_TagRepository? _Crm_Par_Tag;
        public ICrm_ClienteDatiRepository Crm_ClienteDati => _Crm_ClienteDati ??= new Crm_ClienteDatiRepository(_db, _sp, _http);
        private ICrm_ClienteDatiRepository? _Crm_ClienteDati;
        public ICrm_ClienteTagRepository Crm_ClienteTag => _Crm_ClienteTag ??= new Crm_ClienteTagRepository(_db, _sp, _http);
        private ICrm_ClienteTagRepository? _Crm_ClienteTag;
        public ICrm_LeadRepository Crm_Lead => _Crm_Lead ??= new Crm_LeadRepository(_db, _sp, _http);
        private ICrm_LeadRepository? _Crm_Lead;
        public ICrm_OpportunitaRepository Crm_Opportunita => _Crm_Opportunita ??= new Crm_OpportunitaRepository(_db, _sp, _http);
        private ICrm_OpportunitaRepository? _Crm_Opportunita;
        public ICrm_OpportunitaStoricoRepository Crm_OpportunitaStorico => _Crm_OpportunitaStorico ??= new Crm_OpportunitaStoricoRepository(_db, _sp, _http);
        private ICrm_OpportunitaStoricoRepository? _Crm_OpportunitaStorico;
        public ICrm_AttivitaRepository Crm_Attivita => _Crm_Attivita ??= new Crm_AttivitaRepository(_db, _sp, _http);
        private ICrm_AttivitaRepository? _Crm_Attivita;
        public ICrm_OffertaRepository Crm_Offerta => _Crm_Offerta ??= new Crm_OffertaRepository(_db, _sp, _http);
        private ICrm_OffertaRepository? _Crm_Offerta;
        public ICrm_OffertaRigaRepository Crm_OffertaRiga => _Crm_OffertaRiga ??= new Crm_OffertaRigaRepository(_db, _sp, _http);
        private ICrm_OffertaRigaRepository? _Crm_OffertaRiga;
        public ICrm_NotaRepository Crm_Nota => _Crm_Nota ??= new Crm_NotaRepository(_db, _sp, _http);
        private ICrm_NotaRepository? _Crm_Nota;
        public ICrm_AllegatoRepository Crm_Allegato => _Crm_Allegato ??= new Crm_AllegatoRepository(_db, _sp, _http);
        private ICrm_AllegatoRepository? _Crm_Allegato;
        public ICrm_RichiestaRepository Crm_Richiesta => _Crm_Richiesta ??= new Crm_RichiestaRepository(_db, _sp, _http);
        private ICrm_RichiestaRepository? _Crm_Richiesta;
        public ICrm_RichiestaMessaggioRepository Crm_RichiestaMessaggio => _Crm_RichiestaMessaggio ??= new Crm_RichiestaMessaggioRepository(_db, _sp, _http);
        private ICrm_RichiestaMessaggioRepository? _Crm_RichiestaMessaggio;
        public ICrm_Az_AnagraficaReadRepository Az_Anagrafica => _Az_Anagrafica ??= new Crm_Az_AnagraficaReadRepository(_db, _sp, _http);
        private ICrm_Az_AnagraficaReadRepository? _Az_Anagrafica;
        public ICrm_Az_ClienteReadRepository Az_Cliente => _Az_Cliente ??= new Crm_Az_ClienteReadRepository(_db, _sp, _http);
        private ICrm_Az_ClienteReadRepository? _Az_Cliente;
        public ICrm_Az_ClienteContattoReadRepository Az_ClienteContatto => _Az_ClienteContatto ??= new Crm_Az_ClienteContattoReadRepository(_db, _sp, _http);
        private ICrm_Az_ClienteContattoReadRepository? _Az_ClienteContatto;
    }
}
