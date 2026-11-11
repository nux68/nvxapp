using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Compliance;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Compliance;
using nvxapp.server.service.ClientServer_Service.Compliance._utility;
using nvxapp.server.service.ClientServer_Service.Compliance.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Compliance
{
    /*
     Sicurezza sul lavoro, D.Lgs 81/08 (CMP-81, ANALISI_COMPLIANCE_APPLICATIVO.md §4.2):
        - mansioni, corsi richiesti per mansione, mansioni dei dipendenti, corsi svolti e situazione della formazione (CMP-81.1)
        - visite mediche: esito e scadenza agli amministratori, dati sanitari (cifrati) solo agli abilitati (CMP-81.2)
        - catalogo dei DPI e consegne con presa visione del dipendente (fonte cmp.dpi, CMP-81.3)
        - nomine (presa visione del nominato) e documenti DVR (CMP-81.4)
        - ogni scadenza (corso, visita, nomina, documento) registrata nello scadenzario comune a ogni salvataggio e
          tolta alla cancellazione; il corso o la visita piu' recente "rinnova" e chiude le scadenze precedenti (CMP-81.5)
     Permessi: CompanyPowerAdmin e CompanyAdmin tutto; utenti abilitati ai dati sanitari le visite; i dipendenti solo i
     propri dati (Cmp_Mine). Nessun accesso dello studio a quest'area (vede solo le scadenze, vedi Cmp_GdprService).
    */
    public class Cmp_SicurezzaService : ServiceBase, ICmp_SicurezzaService
    {
        private readonly ICmpUtility _utility;
        private readonly ICmpScadenze _scadenze;
        private readonly IAcknowledgementService _ack;
        private readonly IPresaVisioneRepository _presaVisione;
        private readonly ICmp_MansioneRepository _Cmp_Mansione;
        private readonly ICmp_MansioneCorsoRepository _Cmp_MansioneCorso;
        private readonly ICmp_DipendenteMansioneRepository _Cmp_DipendenteMansione;
        private readonly ICmp_FormazioneRepository _Cmp_Formazione;
        private readonly ICmp_VisitaRepository _Cmp_Visita;
        private readonly ICmp_DpiRepository _Cmp_Dpi;
        private readonly ICmp_DpiConsegnaRepository _Cmp_DpiConsegna;
        private readonly ICmp_NominaRepository _Cmp_Nomina;
        private readonly ICmp_DocumentoRepository _Cmp_Documento;
        private readonly ICmp_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly ICmp_Az_SediReadRepository _Az_Sedi;

        public Cmp_SicurezzaService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,
                                    ICmpUtility utility,
                                    ICmpScadenze scadenze,
                                    IAcknowledgementService ack,
                                    IPresaVisioneRepository presaVisione,
                                    ICmp_MansioneRepository cmp_Mansione,
                                    ICmp_MansioneCorsoRepository cmp_MansioneCorso,
                                    ICmp_DipendenteMansioneRepository cmp_DipendenteMansione,
                                    ICmp_FormazioneRepository cmp_Formazione,
                                    ICmp_VisitaRepository cmp_Visita,
                                    ICmp_DpiRepository cmp_Dpi,
                                    ICmp_DpiConsegnaRepository cmp_DpiConsegna,
                                    ICmp_NominaRepository cmp_Nomina,
                                    ICmp_DocumentoRepository cmp_Documento,
                                    ICmp_Dip_AnagraficaReadRepository dip_Anagrafica,
                                    ICmp_Az_SediReadRepository az_Sedi) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _utility = utility;
            _scadenze = scadenze;
            _ack = ack;
            _presaVisione = presaVisione;
            _Cmp_Mansione = cmp_Mansione;
            _Cmp_MansioneCorso = cmp_MansioneCorso;
            _Cmp_DipendenteMansione = cmp_DipendenteMansione;
            _Cmp_Formazione = cmp_Formazione;
            _Cmp_Visita = cmp_Visita;
            _Cmp_Dpi = cmp_Dpi;
            _Cmp_DpiConsegna = cmp_DpiConsegna;
            _Cmp_Nomina = cmp_Nomina;
            _Cmp_Documento = cmp_Documento;
            _Dip_Anagrafica = dip_Anagrafica;
            _Az_Sedi = az_Sedi;
        }

        // =========================================================================================== contesto e tabelle

        private async Task<CmpContesto> AmministratoreAsync()
        {
            var ctx = await _utility.ContestoAsync();
            if (!ctx.Amministratore)
                throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
            return ctx;
        }

        public virtual async Task<GenericResult<Cmp_LookupOutModel>> Cmp_Lookup(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.VedeVisite)
                    throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
                var retVal = new Cmp_LookupOutModel
                {
                    Permessi = new Cmp_PermessiAreeModel { PowerAdmin = ctx.PowerAdmin, Admin = ctx.Admin, Sanitario = ctx.Sanitario },
                    GiorniPreavviso = ctx.GiorniPreavviso,
                    Dipendenti = (await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync())
                                 .Select(x => new Cmp_VoceModel { Id = x.Id, Nome = NomeDip(x.Cognome, x.Nome, x.Matricola), Attivo = x.Attivo })
                                 .OrderBy(x => x.Nome).ToList(),
                };
                if (ctx.Amministratore)
                {
                    retVal.Mansioni = await _Cmp_Mansione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Descrizione)
                                                         .Select(x => new Cmp_VoceModel { Id = x.Id, Nome = x.Descrizione, Attivo = x.Attivo }).ToListAsync();
                    retVal.Dpi = await _Cmp_Dpi.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Descrizione)
                                               .Select(x => new Cmp_VoceModel { Id = x.Id, Nome = x.Descrizione, Attivo = x.Attivo }).ToListAsync();
                    retVal.Sedi = await _Az_Sedi.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Descrizione)
                                                .Select(x => new Cmp_VoceModel { Id = x.Id, Nome = x.Descrizione ?? string.Empty }).ToListAsync();
                }
                return retVal;
            }, isSubProcess);
        }

        // =========================================================================================== mansioni (CMP-81.1)

        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_MansioneModel>>> Cmp_MansioneList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var corsi = await _Cmp_MansioneCorso.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).GroupBy(x => x.IdCmp_Mansione)
                                                    .Select(g => new { g.Key, N = g.Count() }).ToDictionaryAsync(x => x.Key, x => x.N);
                var oggi = DateTime.Today;
                var dip = await _Cmp_DipendenteMansione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && (x.DataFine == null || x.DataFine >= oggi))
                                                       .GroupBy(x => x.IdCmp_Mansione).Select(g => new { g.Key, N = g.Select(y => y.IdDip_Anagrafica).Distinct().Count() })
                                                       .ToDictionaryAsync(x => x.Key, x => x.N);
                var items = await _Cmp_Mansione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Descrizione).ToListAsync();
                return new Cmp_ListOutModel<Cmp_MansioneModel>
                {
                    Items = items.Select(x => new Cmp_MansioneModel
                    {
                        Id = x.Id, Descrizione = x.Descrizione, Rischio = x.Rischio, Note = x.Note, Attivo = x.Attivo,
                        NumeroCorsi = corsi.GetValueOrDefault(x.Id), NumeroDipendenti = dip.GetValueOrDefault(x.Id)
                    }).ToList()
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_PutOutModel<Cmp_MansioneModel>>> Cmp_MansionePut(GenericRequest<Cmp_PutInModel<Cmp_MansioneModel>> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var d = model.Data.Item;
                var riga = d.Id == 0 ? new Cmp_Mansione { IdAz_Anagrafica = ctx.IdAz_Anagrafica }
                           : await _Cmp_Mansione.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                riga.Descrizione = Obbligatorio(d.Descrizione, 100);
                riga.Rischio = Math.Clamp(d.Rischio, 0, 2);
                riga.Note = Testo(d.Note, 500);
                riga.Attivo = d.Attivo;
                riga = d.Id == 0 ? await _Cmp_Mansione.CreateAsync(riga) : await _Cmp_Mansione.UpdateAsync(riga);
                d.Id = riga.Id;
                return new Cmp_PutOutModel<Cmp_MansioneModel> { Item = d };
            }, isSubProcess);
        }

        // cancella anche i corsi richiesti e le assegnazioni ai dipendenti (in cascata); i corsi svolti restano
        public virtual async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_MansioneDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var riga = await _Cmp_Mansione.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                await _Cmp_Mansione.DeleteAsync(riga);
                return new Cmp_DeleteOutModel();
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_MansioneCorsoModel>>> Cmp_MansioneCorsoList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var mansioni = await Mansioni(ctx);
                var items = await _Cmp_MansioneCorso.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync();
                return new Cmp_ListOutModel<Cmp_MansioneCorsoModel>
                {
                    Items = items.Select(x => new Cmp_MansioneCorsoModel
                    {
                        Id = x.Id, IdCmp_Mansione = x.IdCmp_Mansione, Mansione = mansioni.GetValueOrDefault(x.IdCmp_Mansione), TipoCorso = x.TipoCorso,
                        Descrizione = x.Descrizione, ValiditaMesi = x.ValiditaMesi, Ore = x.Ore
                    }).OrderBy(x => x.Mansione).ThenBy(x => x.TipoCorso).ToList()
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_PutOutModel<Cmp_MansioneCorsoModel>>> Cmp_MansioneCorsoPut(GenericRequest<Cmp_PutInModel<Cmp_MansioneCorsoModel>> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var d = model.Data.Item;
                if (!await _Cmp_Mansione.FindAll(x => x.Id == d.IdCmp_Mansione && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    throw new UserMessageException(ComplianceMessages.Get("JobInvalid"));
                var riga = d.Id == 0 ? new Cmp_MansioneCorso { IdAz_Anagrafica = ctx.IdAz_Anagrafica }
                           : await _Cmp_MansioneCorso.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                riga.IdCmp_Mansione = d.IdCmp_Mansione;
                riga.TipoCorso = Math.Clamp(d.TipoCorso, 0, CmpTesti.UltimoTipoCorso);
                riga.Descrizione = Testo(d.Descrizione, 100);
                riga.ValiditaMesi = d.ValiditaMesi is > 0 ? Math.Min(d.ValiditaMesi.Value, 600) : null;
                riga.Ore = d.Ore is > 0 ? d.Ore : null;
                riga = d.Id == 0 ? await _Cmp_MansioneCorso.CreateAsync(riga) : await _Cmp_MansioneCorso.UpdateAsync(riga);
                d.Id = riga.Id;
                return new Cmp_PutOutModel<Cmp_MansioneCorsoModel> { Item = d };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_MansioneCorsoDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var riga = await _Cmp_MansioneCorso.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                await _Cmp_MansioneCorso.DeleteAsync(riga);
                return new Cmp_DeleteOutModel();
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_DipendenteMansioneModel>>> Cmp_DipendenteMansioneList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var dip = await Dipendenti(ctx);
                var mansioni = await Mansioni(ctx);
                var oggi = DateTime.Today;
                var items = await _Cmp_DipendenteMansione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync();
                return new Cmp_ListOutModel<Cmp_DipendenteMansioneModel>
                {
                    Items = items.Select(x => new Cmp_DipendenteMansioneModel
                    {
                        Id = x.Id, IdDip_Anagrafica = x.IdDip_Anagrafica, Dipendente = dip.GetValueOrDefault(x.IdDip_Anagrafica)?.Nome,
                        IdCmp_Mansione = x.IdCmp_Mansione, Mansione = mansioni.GetValueOrDefault(x.IdCmp_Mansione),
                        DataInizio = x.DataInizio, DataFine = x.DataFine, Note = x.Note,
                        Attiva = x.DataInizio <= oggi && (x.DataFine == null || x.DataFine >= oggi)
                    }).OrderBy(x => x.Dipendente).ThenByDescending(x => x.DataInizio).ToList()
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_PutOutModel<Cmp_DipendenteMansioneModel>>> Cmp_DipendenteMansionePut(GenericRequest<Cmp_PutInModel<Cmp_DipendenteMansioneModel>> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var d = model.Data.Item;
                await DipendenteAsync(ctx, d.IdDip_Anagrafica);
                if (!await _Cmp_Mansione.FindAll(x => x.Id == d.IdCmp_Mansione && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    throw new UserMessageException(ComplianceMessages.Get("JobInvalid"));
                Periodo(d.DataInizio, d.DataFine);
                var riga = d.Id == 0 ? new Cmp_DipendenteMansione { IdAz_Anagrafica = ctx.IdAz_Anagrafica }
                           : await _Cmp_DipendenteMansione.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                riga.IdDip_Anagrafica = d.IdDip_Anagrafica;
                riga.IdCmp_Mansione = d.IdCmp_Mansione;
                riga.DataInizio = d.DataInizio.Date;
                riga.DataFine = d.DataFine?.Date;
                riga.Note = Testo(d.Note, 500);
                riga = d.Id == 0 ? await _Cmp_DipendenteMansione.CreateAsync(riga) : await _Cmp_DipendenteMansione.UpdateAsync(riga);
                d.Id = riga.Id;
                return new Cmp_PutOutModel<Cmp_DipendenteMansioneModel> { Item = d };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_DipendenteMansioneDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var riga = await _Cmp_DipendenteMansione.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                await _Cmp_DipendenteMansione.DeleteAsync(riga);
                return new Cmp_DeleteOutModel();
            }, isSubProcess);
        }

        // =========================================================================================== formazione (CMP-81.1, 81.5)

        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_FormazioneModel>>> Cmp_FormazioneList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var dip = await Dipendenti(ctx);
                var items = await _Cmp_Formazione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync();
                return new Cmp_ListOutModel<Cmp_FormazioneModel> { Items = FormazioneModelli(ctx, items, dip) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_PutOutModel<Cmp_FormazioneModel>>> Cmp_FormazionePut(GenericRequest<Cmp_PutInModel<Cmp_FormazioneModel>> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var d = model.Data.Item;
                await DipendenteAsync(ctx, d.IdDip_Anagrafica);
                if (d.DataCorso == default)
                    throw new UserMessageException(ComplianceMessages.Get("DateRequired"));
                var riga = d.Id == 0 ? new Cmp_Formazione { IdAz_Anagrafica = ctx.IdAz_Anagrafica }
                           : await _Cmp_Formazione.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                var prima = (riga.IdDip_Anagrafica, riga.TipoCorso);
                riga.IdDip_Anagrafica = d.IdDip_Anagrafica;
                riga.TipoCorso = Math.Clamp(d.TipoCorso, 0, CmpTesti.UltimoTipoCorso);
                riga.Descrizione = Obbligatorio(d.Descrizione, 200);
                riga.DataCorso = d.DataCorso.Date;
                riga.Ore = d.Ore is > 0 ? Math.Min(d.Ore.Value, 9999) : null;
                riga.Ente = Testo(d.Ente, 100);
                // senza scadenza indicata: validita' del corso richiesta dalle mansioni (la piu' breve), se c'e'
                var validita = d.DataScadenza == null ? await ValiditaAsync(ctx, riga.TipoCorso) : null;
                riga.DataScadenza = d.DataScadenza?.Date ?? (validita != null ? riga.DataCorso.AddMonths(validita.Value) : null);
                if (riga.DataScadenza != null && riga.DataScadenza < riga.DataCorso)
                    throw new UserMessageException(ComplianceMessages.Get("PeriodInvalid"));
                riga.Attestato = Testo(d.Attestato, 200);
                riga.Note = Testo(d.Note, 500);
                riga = d.Id == 0 ? await _Cmp_Formazione.CreateAsync(riga) : await _Cmp_Formazione.UpdateAsync(riga);

                await ScadenzeFormazioneAsync(ctx, riga.IdDip_Anagrafica, riga.TipoCorso);
                if (d.Id != 0 && prima != (riga.IdDip_Anagrafica, riga.TipoCorso))
                    await ScadenzeFormazioneAsync(ctx, prima.IdDip_Anagrafica, prima.TipoCorso);

                d.Id = riga.Id;
                d.DataScadenza = riga.DataScadenza;
                return new Cmp_PutOutModel<Cmp_FormazioneModel> { Item = d };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_FormazioneDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var riga = await _Cmp_Formazione.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                await _Cmp_Formazione.DeleteAsync(riga);
                await _scadenze.RimuoviAsync(ctx, CmpFonti.Formazione, riga.Id);
                // il corso precedente dello stesso tipo torna a essere quello valido
                await ScadenzeFormazioneAsync(ctx, riga.IdDip_Anagrafica, riga.TipoCorso);
                return new Cmp_DeleteOutModel();
            }, isSubProcess);
        }

        // il corso piu' recente di un tipo ha la scadenza aperta, i precedenti l'hanno chiusa (rinnovati)
        private async Task ScadenzeFormazioneAsync(CmpContesto ctx, int idDip, int tipoCorso)
        {
            var corsi = await _Cmp_Formazione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdDip_Anagrafica == idDip && x.TipoCorso == tipoCorso)
                                             .OrderByDescending(x => x.DataCorso).ThenByDescending(x => x.Id).ToListAsync();
            if (corsi.Count == 0)
                return;
            var nome = (await DipendenteAsync(ctx, idDip, false))?.Nome ?? string.Empty;
            for (int i = 0; i < corsi.Count; i++)
                await _scadenze.RegistraAsync(ctx, CmpFonti.Formazione, corsi[i].Id, ComplianceMessages.Get("DeadlineCourse", corsi[i].Descrizione, nome),
                                              corsi[i].DataScadenza, i > 0, CmpUrl.Sicurezza, "formazione");
        }

        private async Task<int?> ValiditaAsync(CmpContesto ctx, int tipoCorso)
            => await _Cmp_MansioneCorso.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.TipoCorso == tipoCorso && x.ValiditaMesi != null)
                                       .MinAsync(x => x.ValiditaMesi);

        private static List<Cmp_FormazioneModel> FormazioneModelli(CmpContesto ctx, List<Cmp_Formazione> items, Dictionary<int, Dip> dip)
        {
            var oggi = DateTime.Today;
            var ultimi = items.GroupBy(x => (x.IdDip_Anagrafica, x.TipoCorso))
                              .Select(g => g.OrderByDescending(x => x.DataCorso).ThenByDescending(x => x.Id).First().Id).ToHashSet();
            return items.Select(x => new Cmp_FormazioneModel
            {
                Id = x.Id, IdDip_Anagrafica = x.IdDip_Anagrafica, Dipendente = dip.GetValueOrDefault(x.IdDip_Anagrafica)?.Nome, TipoCorso = x.TipoCorso,
                Descrizione = x.Descrizione, DataCorso = x.DataCorso, Ore = x.Ore, Ente = x.Ente, DataScadenza = x.DataScadenza, Attestato = x.Attestato, Note = x.Note,
                Rinnovata = !ultimi.Contains(x.Id),
                Scaduta = ultimi.Contains(x.Id) && x.DataScadenza != null && x.DataScadenza < oggi,
                InScadenza = ultimi.Contains(x.Id) && x.DataScadenza != null && x.DataScadenza >= oggi && x.DataScadenza <= oggi.AddDays(ctx.GiorniPreavviso),
            }).OrderByDescending(x => x.DataCorso).ToList();
        }

        // situazione della formazione obbligatoria: per ogni dipendente con mansioni attive, i corsi richiesti e l'ultimo svolto
        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_FormazioneStatoModel>>> Cmp_FormazioneStato(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var oggi = DateTime.Today;
                var dip = await Dipendenti(ctx);
                var mansioni = await Mansioni(ctx);
                var assegnate = await _Cmp_DipendenteMansione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.DataInizio <= oggi && (x.DataFine == null || x.DataFine >= oggi))
                                                             .Select(x => new { x.IdDip_Anagrafica, x.IdCmp_Mansione }).ToListAsync();
                var richiesti = await _Cmp_MansioneCorso.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync();
                var corsi = await _Cmp_Formazione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync();

                var retVal = new Cmp_ListOutModel<Cmp_FormazioneStatoModel>();
                foreach (var g in assegnate.GroupBy(x => x.IdDip_Anagrafica))
                {
                    if (!dip.TryGetValue(g.Key, out var d) || !d.Attivo)
                        continue;
                    var idMansioni = g.Select(x => x.IdCmp_Mansione).Distinct().ToList();
                    var nomiMansioni = string.Join(", ", idMansioni.Select(m => mansioni.GetValueOrDefault(m)).Where(m => m != null));
                    foreach (var tipo in richiesti.Where(r => idMansioni.Contains(r.IdCmp_Mansione)).GroupBy(r => r.TipoCorso))
                    {
                        var req = tipo.OrderBy(r => r.ValiditaMesi ?? int.MaxValue).First();
                        var ultimo = corsi.Where(c => c.IdDip_Anagrafica == g.Key && c.TipoCorso == tipo.Key).OrderByDescending(c => c.DataCorso).ThenByDescending(c => c.Id).FirstOrDefault();
                        var scadenza = ultimo?.DataScadenza ?? (ultimo != null && req.ValiditaMesi != null ? ultimo.DataCorso.AddMonths(req.ValiditaMesi.Value) : null);
                        retVal.Items.Add(new Cmp_FormazioneStatoModel
                        {
                            Id = g.Key + "-" + tipo.Key,
                            IdDip_Anagrafica = g.Key, Dipendente = d.Nome, Mansioni = nomiMansioni,
                            TipoCorso = tipo.Key, Corso = req.Descrizione, ValiditaMesi = req.ValiditaMesi,
                            IdCmp_Formazione = ultimo?.Id, DataCorso = ultimo?.DataCorso, DataScadenza = scadenza,
                            Stato = ultimo == null ? 0 : scadenza == null ? 1 : scadenza < oggi ? 3 : scadenza <= oggi.AddDays(ctx.GiorniPreavviso) ? 2 : 1,
                        });
                    }
                }
                retVal.Items = retVal.Items.OrderBy(x => x.Dipendente).ThenBy(x => x.TipoCorso).ToList();
                return retVal;
            }, isSubProcess);
        }

        // =========================================================================================== visite (CMP-81.2)

        private async Task<CmpContesto> VisiteAsync()
        {
            var ctx = await _utility.ContestoAsync();
            if (!ctx.VedeVisite)
                throw new UserMessageException(ComplianceMessages.Get("NotAllowed"));
            return ctx;
        }

        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_VisitaModel>>> Cmp_VisitaList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await VisiteAsync();
                var dip = await Dipendenti(ctx);
                var items = await _Cmp_Visita.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync();
                return new Cmp_ListOutModel<Cmp_VisitaModel> { Items = VisitaModelli(ctx, items, dip, ctx.Sanitario) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_PutOutModel<Cmp_VisitaModel>>> Cmp_VisitaPut(GenericRequest<Cmp_PutInModel<Cmp_VisitaModel>> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await VisiteAsync();
                var d = model.Data.Item;
                await DipendenteAsync(ctx, d.IdDip_Anagrafica);
                if (d.DataVisita == default)
                    throw new UserMessageException(ComplianceMessages.Get("DateRequired"));
                if (d.DataScadenza != null && d.DataScadenza < d.DataVisita)
                    throw new UserMessageException(ComplianceMessages.Get("PeriodInvalid"));
                var riga = d.Id == 0 ? new Cmp_Visita { IdAz_Anagrafica = ctx.IdAz_Anagrafica }
                           : await _Cmp_Visita.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                var dipPrima = riga.IdDip_Anagrafica;
                riga.IdDip_Anagrafica = d.IdDip_Anagrafica;
                riga.Tipo = Math.Clamp(d.Tipo, 0, 5);
                riga.DataVisita = d.DataVisita.Date;
                riga.Esito = Math.Clamp(d.Esito, 0, 3);
                riga.Limitazioni = Testo(d.Limitazioni, 500);
                riga.DataScadenza = d.DataScadenza?.Date;
                riga.Medico = Testo(d.Medico, 100);
                // i dati sanitari li scrive solo chi e' abilitato; per gli altri restano quelli salvati
                if (ctx.Sanitario)
                    riga.DatiSanitari = _utility.Cifra(Testo(d.DatiSanitari, 4000));
                riga = d.Id == 0 ? await _Cmp_Visita.CreateAsync(riga) : await _Cmp_Visita.UpdateAsync(riga);

                await ScadenzeVisitaAsync(ctx, riga.IdDip_Anagrafica);
                if (d.Id != 0 && dipPrima != riga.IdDip_Anagrafica)
                    await ScadenzeVisitaAsync(ctx, dipPrima);

                d.Id = riga.Id;
                if (!ctx.Sanitario)
                    d.DatiSanitari = null;
                return new Cmp_PutOutModel<Cmp_VisitaModel> { Item = d };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_VisitaDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await VisiteAsync();
                var riga = await _Cmp_Visita.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                await _Cmp_Visita.DeleteAsync(riga);
                await _scadenze.RimuoviAsync(ctx, CmpFonti.Visita, riga.Id);
                await ScadenzeVisitaAsync(ctx, riga.IdDip_Anagrafica);
                return new Cmp_DeleteOutModel();
            }, isSubProcess);
        }

        // l'ultima visita del dipendente ha la scadenza aperta (prossima visita), le precedenti l'hanno chiusa
        private async Task ScadenzeVisitaAsync(CmpContesto ctx, int idDip)
        {
            var visite = await _Cmp_Visita.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdDip_Anagrafica == idDip)
                                          .OrderByDescending(x => x.DataVisita).ThenByDescending(x => x.Id)
                                          .Select(x => new { x.Id, x.DataScadenza }).ToListAsync();
            if (visite.Count == 0)
                return;
            var nome = (await DipendenteAsync(ctx, idDip, false))?.Nome ?? string.Empty;
            for (int i = 0; i < visite.Count; i++)
                await _scadenze.RegistraAsync(ctx, CmpFonti.Visita, visite[i].Id, ComplianceMessages.Get("DeadlineVisit", nome), visite[i].DataScadenza, i > 0,
                                              CmpUrl.Sicurezza, "visite");
        }

        private List<Cmp_VisitaModel> VisitaModelli(CmpContesto ctx, List<Cmp_Visita> items, Dictionary<int, Dip> dip, bool sanitario)
        {
            var oggi = DateTime.Today;
            var ultime = items.GroupBy(x => x.IdDip_Anagrafica).Select(g => g.OrderByDescending(x => x.DataVisita).ThenByDescending(x => x.Id).First().Id).ToHashSet();
            return items.Select(x => new Cmp_VisitaModel
            {
                Id = x.Id, IdDip_Anagrafica = x.IdDip_Anagrafica, Dipendente = dip.GetValueOrDefault(x.IdDip_Anagrafica)?.Nome, Tipo = x.Tipo,
                DataVisita = x.DataVisita, Esito = x.Esito, Limitazioni = x.Limitazioni, DataScadenza = x.DataScadenza, Medico = x.Medico,
                DatiSanitari = sanitario ? _utility.Decifra(x.DatiSanitari) : null,
                Rinnovata = !ultime.Contains(x.Id),
                Scaduta = ultime.Contains(x.Id) && x.DataScadenza != null && x.DataScadenza < oggi,
                InScadenza = ultime.Contains(x.Id) && x.DataScadenza != null && x.DataScadenza >= oggi && x.DataScadenza <= oggi.AddDays(ctx.GiorniPreavviso),
            }).OrderByDescending(x => x.DataVisita).ToList();
        }

        // =========================================================================================== DPI (CMP-81.3)

        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_DpiModel>>> Cmp_DpiList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var items = await _Cmp_Dpi.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Descrizione).ToListAsync();
                return new Cmp_ListOutModel<Cmp_DpiModel>
                {
                    Items = items.Select(x => new Cmp_DpiModel { Id = x.Id, Descrizione = x.Descrizione, Categoria = x.Categoria, Norma = x.Norma, Note = x.Note, Attivo = x.Attivo }).ToList()
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_PutOutModel<Cmp_DpiModel>>> Cmp_DpiPut(GenericRequest<Cmp_PutInModel<Cmp_DpiModel>> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var d = model.Data.Item;
                var riga = d.Id == 0 ? new Cmp_Dpi { IdAz_Anagrafica = ctx.IdAz_Anagrafica }
                           : await _Cmp_Dpi.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                riga.Descrizione = Obbligatorio(d.Descrizione, 100);
                riga.Categoria = Math.Clamp(d.Categoria, 1, 3);
                riga.Norma = Testo(d.Norma, 50);
                riga.Note = Testo(d.Note, 500);
                riga.Attivo = d.Attivo;
                riga = d.Id == 0 ? await _Cmp_Dpi.CreateAsync(riga) : await _Cmp_Dpi.UpdateAsync(riga);
                d.Id = riga.Id;
                return new Cmp_PutOutModel<Cmp_DpiModel> { Item = d };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_DpiDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var riga = await _Cmp_Dpi.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                if (await _Cmp_DpiConsegna.FindAll(x => x.IdCmp_Dpi == riga.Id).AnyAsync())
                    throw new UserMessageException(ComplianceMessages.Get("DpiInUse"));
                await _Cmp_Dpi.DeleteAsync(riga);
                return new Cmp_DeleteOutModel();
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_DpiConsegnaModel>>> Cmp_DpiConsegnaList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var items = await _Cmp_DpiConsegna.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync();
                return new Cmp_ListOutModel<Cmp_DpiConsegnaModel> { Items = await ConsegnaModelli(ctx, items) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_PutOutModel<Cmp_DpiConsegnaModel>>> Cmp_DpiConsegnaPut(GenericRequest<Cmp_PutInModel<Cmp_DpiConsegnaModel>> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var d = model.Data.Item;
                var dipendente = (await DipendenteAsync(ctx, d.IdDip_Anagrafica))!;
                var dpi = await _Cmp_Dpi.FindAll(x => x.Id == d.IdCmp_Dpi && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                          ?? throw new UserMessageException(ComplianceMessages.Get("DpiInvalid"));
                if (d.DataConsegna == default)
                    throw new UserMessageException(ComplianceMessages.Get("DateRequired"));
                var riga = d.Id == 0 ? new Cmp_DpiConsegna { IdAz_Anagrafica = ctx.IdAz_Anagrafica }
                           : await _Cmp_DpiConsegna.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                var prima = (riga.IdDip_Anagrafica, riga.IdCmp_Dpi, riga.DataConsegna, riga.Quantita, riga.Taglia);
                riga.IdDip_Anagrafica = d.IdDip_Anagrafica;
                riga.IdCmp_Dpi = d.IdCmp_Dpi;
                riga.DataConsegna = d.DataConsegna.Date;
                riga.Quantita = Math.Clamp(d.Quantita, 1, 9999);
                riga.Taglia = Testo(d.Taglia, 20);
                riga.Note = Testo(d.Note, 500);
                riga = d.Id == 0 ? await _Cmp_DpiConsegna.CreateAsync(riga) : await _Cmp_DpiConsegna.UpdateAsync(riga);

                // verbale di consegna: presa visione del dipendente (se ha un utente). Cambiando il dipendente si ritira la richiesta
                // al precedente; cambiando cosa e' stato consegnato si chiede di nuovo la conferma
                bool cambiata = d.Id != 0 && prima != (riga.IdDip_Anagrafica, riga.IdCmp_Dpi, riga.DataConsegna, riga.Quantita, riga.Taglia);
                if (d.Id != 0 && prima.IdDip_Anagrafica != riga.IdDip_Anagrafica)
                    await _ack.CancelAsync(ctx.IdCompany, ApplicationType.Compliance, CmpFonti.Dpi, riga.Id);
                if (!string.IsNullOrEmpty(dipendente.IdAspNetUsers))
                    await _ack.RequestAsync(new AcknowledgementRequest
                    {
                        IdCompany = ctx.IdCompany,
                        Application = ApplicationType.Compliance,
                        Fonte = CmpFonti.Dpi,
                        IdRiferimento = riga.Id,
                        UserIds = new[] { dipendente.IdAspNetUsers! },
                        Titolo = ComplianceMessages.Get("AckDpiTitle", dpi.Descrizione),
                        Testo = ComplianceMessages.Get("AckDpiText", dipendente.Nome, riga.DataConsegna.ToString("dd/MM/yyyy"), dpi.Descrizione, riga.Quantita,
                                                       string.IsNullOrEmpty(riga.Taglia) ? string.Empty : ComplianceMessages.Get("AckDpiSize", riga.Taglia),
                                                       string.IsNullOrEmpty(dpi.Norma) ? string.Empty : " (" + dpi.Norma + ")"),
                        Url = CmpUrl.Miei,
                        UrlState = new { sezione = "dpi", id = riga.Id },
                        RichiediDiNuovo = cambiata,
                        Notifica = ctx.NotificheAttive,
                    });

                return new Cmp_PutOutModel<Cmp_DpiConsegnaModel> { Item = (await ConsegnaModelli(ctx, new List<Cmp_DpiConsegna> { riga })).First() };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_DpiConsegnaDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var riga = await _Cmp_DpiConsegna.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                await _Cmp_DpiConsegna.DeleteAsync(riga);
                await _ack.CancelAsync(ctx.IdCompany, ApplicationType.Compliance, CmpFonti.Dpi, riga.Id);
                return new Cmp_DeleteOutModel();
            }, isSubProcess);
        }

        private async Task<List<Cmp_DpiConsegnaModel>> ConsegnaModelli(CmpContesto ctx, List<Cmp_DpiConsegna> items)
        {
            var dip = await Dipendenti(ctx);
            var dpi = await _Cmp_Dpi.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToDictionaryAsync(x => x.Id, x => x.Descrizione);
            var prese = await PreseVisioneAsync(ctx, CmpFonti.Dpi, items.Select(x => x.Id).ToList());
            return items.Select(x => new Cmp_DpiConsegnaModel
            {
                Id = x.Id, IdDip_Anagrafica = x.IdDip_Anagrafica, Dipendente = dip.GetValueOrDefault(x.IdDip_Anagrafica)?.Nome, IdCmp_Dpi = x.IdCmp_Dpi,
                Dpi = dpi.GetValueOrDefault(x.IdCmp_Dpi), DataConsegna = x.DataConsegna, Quantita = x.Quantita, Taglia = x.Taglia, Note = x.Note,
                PresaVisioneRichiesta = prese.ContainsKey(x.Id), DataPresaVisione = prese.GetValueOrDefault(x.Id),
            }).OrderByDescending(x => x.DataConsegna).ThenBy(x => x.Dipendente).ToList();
        }

        // prese visione (non annullate) degli oggetti: id -> data di conferma (null = da confermare)
        private async Task<Dictionary<int, DateTime?>> PreseVisioneAsync(CmpContesto ctx, string fonte, List<int> ids)
            => (await _presaVisione.FindAll(x => x.IdCompany == ctx.IdCompany && x.Application == (int)ApplicationType.Compliance && x.Fonte == fonte
                                                 && x.IdRiferimento != null && ids.Contains(x.IdRiferimento.Value) && !x.Annullata)
                                   .Select(x => new { Id = x.IdRiferimento!.Value, x.DataConferma }).ToListAsync())
               .GroupBy(x => x.Id).ToDictionary(g => g.Key, g => g.Max(x => x.DataConferma));

        // =========================================================================================== nomine e documenti (CMP-81.4)

        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_NominaModel>>> Cmp_NominaList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var items = await _Cmp_Nomina.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToListAsync();
                return new Cmp_ListOutModel<Cmp_NominaModel> { Items = await NominaModelli(ctx, items) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_PutOutModel<Cmp_NominaModel>>> Cmp_NominaPut(GenericRequest<Cmp_PutInModel<Cmp_NominaModel>> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var d = model.Data.Item;
                Dip? dipendente = d.IdDip_Anagrafica is > 0 ? await DipendenteAsync(ctx, d.IdDip_Anagrafica.Value) : null;
                var esterno = Testo(d.NominativoEsterno, 100);
                if (dipendente == null && esterno == null)
                    throw new UserMessageException(ComplianceMessages.Get("AppointmentPersonRequired"));
                if (d.IdAz_Sedi is > 0 && !await _Az_Sedi.FindAll(x => x.Id == d.IdAz_Sedi && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    throw new UserMessageException(ComplianceMessages.Get("SiteInvalid"));
                Periodo(d.DataInizio, d.DataFine);
                var riga = d.Id == 0 ? new Cmp_Nomina { IdAz_Anagrafica = ctx.IdAz_Anagrafica }
                           : await _Cmp_Nomina.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                var dipPrima = riga.IdDip_Anagrafica;
                riga.Tipo = Math.Clamp(d.Tipo, 0, CmpTesti.UltimoTipoNomina);
                riga.IdDip_Anagrafica = dipendente?.Id;
                riga.NominativoEsterno = dipendente == null ? esterno : null;
                riga.IdAz_Sedi = d.IdAz_Sedi is > 0 ? d.IdAz_Sedi : null;
                riga.DataInizio = d.DataInizio.Date;
                riga.DataFine = d.DataFine?.Date;
                riga.Documento = Testo(d.Documento, 200);
                riga.Note = Testo(d.Note, 500);
                riga = d.Id == 0 ? await _Cmp_Nomina.CreateAsync(riga) : await _Cmp_Nomina.UpdateAsync(riga);

                var tipo = CmpTesti.TipoNomina(riga.Tipo);
                var persona = dipendente?.Nome ?? riga.NominativoEsterno ?? string.Empty;
                await _scadenze.RegistraAsync(ctx, CmpFonti.Nomina, riga.Id, ComplianceMessages.Get("DeadlineAppointment", tipo, persona), riga.DataFine, false,
                                              CmpUrl.Sicurezza, "nomine");

                // presa visione della nomina da parte del dipendente nominato
                if (d.Id != 0 && dipPrima != riga.IdDip_Anagrafica)
                    await _ack.CancelAsync(ctx.IdCompany, ApplicationType.Compliance, CmpFonti.Nomina, riga.Id);
                if (!string.IsNullOrEmpty(dipendente?.IdAspNetUsers))
                    await _ack.RequestAsync(new AcknowledgementRequest
                    {
                        IdCompany = ctx.IdCompany,
                        Application = ApplicationType.Compliance,
                        Fonte = CmpFonti.Nomina,
                        IdRiferimento = riga.Id,
                        UserIds = new[] { dipendente.IdAspNetUsers! },
                        Titolo = ComplianceMessages.Get("AckAppointmentTitle", tipo),
                        Testo = ComplianceMessages.Get("AckAppointmentText", dipendente.Nome, tipo, riga.DataInizio.ToString("dd/MM/yyyy"),
                                                       riga.DataFine == null ? string.Empty : ComplianceMessages.Get("AckAppointmentUntil", riga.DataFine.Value.ToString("dd/MM/yyyy"))),
                        Url = CmpUrl.Miei,
                        UrlState = new { sezione = "nomine", id = riga.Id },
                        Notifica = ctx.NotificheAttive,
                    });

                return new Cmp_PutOutModel<Cmp_NominaModel> { Item = (await NominaModelli(ctx, new List<Cmp_Nomina> { riga })).First() };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_NominaDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var riga = await _Cmp_Nomina.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                await _Cmp_Nomina.DeleteAsync(riga);
                await _scadenze.RimuoviAsync(ctx, CmpFonti.Nomina, riga.Id);
                await _ack.CancelAsync(ctx.IdCompany, ApplicationType.Compliance, CmpFonti.Nomina, riga.Id);
                return new Cmp_DeleteOutModel();
            }, isSubProcess);
        }

        private async Task<List<Cmp_NominaModel>> NominaModelli(CmpContesto ctx, List<Cmp_Nomina> items)
        {
            var oggi = DateTime.Today;
            var dip = await Dipendenti(ctx);
            var sedi = await _Az_Sedi.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToDictionaryAsync(x => x.Id, x => x.Descrizione);
            var prese = await PreseVisioneAsync(ctx, CmpFonti.Nomina, items.Select(x => x.Id).ToList());
            return items.Select(x => new Cmp_NominaModel
            {
                Id = x.Id, Tipo = x.Tipo, IdDip_Anagrafica = x.IdDip_Anagrafica, NominativoEsterno = x.NominativoEsterno,
                Persona = x.IdDip_Anagrafica != null ? dip.GetValueOrDefault(x.IdDip_Anagrafica.Value)?.Nome : x.NominativoEsterno,
                IdAz_Sedi = x.IdAz_Sedi, Sede = x.IdAz_Sedi != null ? sedi.GetValueOrDefault(x.IdAz_Sedi.Value) : null,
                DataInizio = x.DataInizio, DataFine = x.DataFine, Documento = x.Documento, Note = x.Note,
                Scaduta = x.DataFine != null && x.DataFine < oggi,
                InScadenza = x.DataFine != null && x.DataFine >= oggi && x.DataFine <= oggi.AddDays(ctx.GiorniPreavviso),
                PresaVisioneRichiesta = prese.ContainsKey(x.Id), DataPresaVisione = prese.GetValueOrDefault(x.Id),
            }).OrderBy(x => x.Tipo).ThenBy(x => x.Persona).ToList();
        }

        public virtual async Task<GenericResult<Cmp_ListOutModel<Cmp_DocumentoModel>>> Cmp_DocumentoList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var oggi = DateTime.Today;
                var items = await _Cmp_Documento.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Tipo).ThenByDescending(x => x.DataRevisione).ToListAsync();
                return new Cmp_ListOutModel<Cmp_DocumentoModel>
                {
                    Items = items.Select(x => new Cmp_DocumentoModel
                    {
                        Id = x.Id, Tipo = x.Tipo, Titolo = x.Titolo, Versione = x.Versione, DataRevisione = x.DataRevisione, DataScadenza = x.DataScadenza,
                        Riferimento = x.Riferimento, Note = x.Note,
                        Scaduta = x.DataScadenza != null && x.DataScadenza < oggi,
                        InScadenza = x.DataScadenza != null && x.DataScadenza >= oggi && x.DataScadenza <= oggi.AddDays(ctx.GiorniPreavviso),
                    }).ToList()
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_PutOutModel<Cmp_DocumentoModel>>> Cmp_DocumentoPut(GenericRequest<Cmp_PutInModel<Cmp_DocumentoModel>> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var d = model.Data.Item;
                if (d.DataRevisione == default)
                    throw new UserMessageException(ComplianceMessages.Get("DateRequired"));
                if (d.DataScadenza != null && d.DataScadenza < d.DataRevisione)
                    throw new UserMessageException(ComplianceMessages.Get("PeriodInvalid"));
                var riga = d.Id == 0 ? new Cmp_Documento { IdAz_Anagrafica = ctx.IdAz_Anagrafica }
                           : await _Cmp_Documento.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                riga.Tipo = Math.Clamp(d.Tipo, 0, 4);
                riga.Titolo = Obbligatorio(d.Titolo, 200);
                riga.Versione = Testo(d.Versione, 20);
                riga.DataRevisione = d.DataRevisione.Date;
                riga.DataScadenza = d.DataScadenza?.Date;
                riga.Riferimento = Testo(d.Riferimento, 200);
                riga.Note = Testo(d.Note, 500);
                riga = d.Id == 0 ? await _Cmp_Documento.CreateAsync(riga) : await _Cmp_Documento.UpdateAsync(riga);
                await _scadenze.RegistraAsync(ctx, CmpFonti.Documento, riga.Id, ComplianceMessages.Get("DeadlineDocument", riga.Titolo), riga.DataScadenza, false,
                                              CmpUrl.Sicurezza, "documenti");
                d.Id = riga.Id;
                return new Cmp_PutOutModel<Cmp_DocumentoModel> { Item = d };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Cmp_DeleteOutModel>> Cmp_DocumentoDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await AmministratoreAsync();
                var riga = await _Cmp_Documento.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync() ?? throw NonTrovato();
                await _Cmp_Documento.DeleteAsync(riga);
                await _scadenze.RimuoviAsync(ctx, CmpFonti.Documento, riga.Id);
                return new Cmp_DeleteOutModel();
            }, isSubProcess);
        }

        // =========================================================================================== dipendente (User)

        public virtual async Task<GenericResult<Cmp_MineOutModel>> Cmp_Mine(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var retVal = new Cmp_MineOutModel();
                if (ctx.Studio || ctx.IdDip_Anagrafica == null)
                    return retVal;
                int idDip = ctx.IdDip_Anagrafica.Value;
                retVal.Dipendente = true;
                var dip = await Dipendenti(ctx);
                retVal.Formazione = FormazioneModelli(ctx, await _Cmp_Formazione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdDip_Anagrafica == idDip).ToListAsync(), dip);
                // visite: solo tipo, data, esito e scadenza (niente limitazioni in dettaglio, medico o dati sanitari)
                retVal.Visite = VisitaModelli(ctx, await _Cmp_Visita.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdDip_Anagrafica == idDip).ToListAsync(), dip, false);
                retVal.Visite.ForEach(v => { v.Limitazioni = null; v.Medico = null; });
                retVal.Consegne = await ConsegnaModelli(ctx, await _Cmp_DpiConsegna.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdDip_Anagrafica == idDip).ToListAsync());
                retVal.Nomine = await NominaModelli(ctx, await _Cmp_Nomina.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.IdDip_Anagrafica == idDip).ToListAsync());
                return retVal;
            }, isSubProcess);
        }

        // =========================================================================================== utilita'

        private sealed record Dip(int Id, string Nome, string? IdAspNetUsers, bool Attivo);

        private async Task<Dictionary<int, Dip>> Dipendenti(CmpContesto ctx)
            => (await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                     .Select(x => new { x.Id, x.Cognome, x.Nome, x.Matricola, x.IdAspNetUsers, x.Attivo }).ToListAsync())
               .ToDictionary(x => x.Id, x => new Dip(x.Id, NomeDip(x.Cognome, x.Nome, x.Matricola), x.IdAspNetUsers, x.Attivo));

        private async Task<Dictionary<int, string>> Mansioni(CmpContesto ctx)
            => await _Cmp_Mansione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToDictionaryAsync(x => x.Id, x => x.Descrizione);

        private async Task<Dip?> DipendenteAsync(CmpContesto ctx, int idDip, bool obbligatorio = true)
        {
            var x = await _Dip_Anagrafica.FindAll(d => d.Id == idDip && d.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                         .Select(d => new { d.Id, d.Cognome, d.Nome, d.Matricola, d.IdAspNetUsers, d.Attivo }).FirstOrDefaultAsync();
            if (x == null && obbligatorio)
                throw new UserMessageException(ComplianceMessages.Get("EmployeeInvalid"));
            return x == null ? null : new Dip(x.Id, NomeDip(x.Cognome, x.Nome, x.Matricola), x.IdAspNetUsers, x.Attivo);
        }

        internal static string NomeDip(string? cognome, string? nome, string? matricola)
        {
            var s = ((cognome ?? string.Empty) + " " + (nome ?? string.Empty)).Trim();
            return s.Length > 0 ? s : matricola ?? string.Empty;
        }

        internal static string Obbligatorio(string? s, int max)
        {
            var t = Testo(s, max);
            return t ?? throw new UserMessageException(ComplianceMessages.Get("DescriptionRequired"));
        }

        internal static string? Testo(string? s, int max)
        {
            if (string.IsNullOrWhiteSpace(s))
                return null;
            s = s.Trim();
            return s.Length > max ? s[..max] : s;
        }

        private static void Periodo(DateTime inizio, DateTime? fine)
        {
            if (inizio == default)
                throw new UserMessageException(ComplianceMessages.Get("DateRequired"));
            if (fine != null && fine.Value.Date < inizio.Date)
                throw new UserMessageException(ComplianceMessages.Get("PeriodInvalid"));
        }

        internal static UserMessageException NonTrovato() => new(ComplianceMessages.Get("NotFound"));
    }

    public interface ICmp_SicurezzaService : IServiceBase
    {
        Task<GenericResult<Cmp_LookupOutModel>> Cmp_Lookup(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);

        Task<GenericResult<Cmp_ListOutModel<Cmp_MansioneModel>>> Cmp_MansioneList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_PutOutModel<Cmp_MansioneModel>>> Cmp_MansionePut(GenericRequest<Cmp_PutInModel<Cmp_MansioneModel>> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_DeleteOutModel>> Cmp_MansioneDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_ListOutModel<Cmp_MansioneCorsoModel>>> Cmp_MansioneCorsoList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_PutOutModel<Cmp_MansioneCorsoModel>>> Cmp_MansioneCorsoPut(GenericRequest<Cmp_PutInModel<Cmp_MansioneCorsoModel>> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_DeleteOutModel>> Cmp_MansioneCorsoDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_ListOutModel<Cmp_DipendenteMansioneModel>>> Cmp_DipendenteMansioneList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_PutOutModel<Cmp_DipendenteMansioneModel>>> Cmp_DipendenteMansionePut(GenericRequest<Cmp_PutInModel<Cmp_DipendenteMansioneModel>> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_DeleteOutModel>> Cmp_DipendenteMansioneDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess);

        Task<GenericResult<Cmp_ListOutModel<Cmp_FormazioneModel>>> Cmp_FormazioneList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_PutOutModel<Cmp_FormazioneModel>>> Cmp_FormazionePut(GenericRequest<Cmp_PutInModel<Cmp_FormazioneModel>> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_DeleteOutModel>> Cmp_FormazioneDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_ListOutModel<Cmp_FormazioneStatoModel>>> Cmp_FormazioneStato(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);

        Task<GenericResult<Cmp_ListOutModel<Cmp_VisitaModel>>> Cmp_VisitaList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_PutOutModel<Cmp_VisitaModel>>> Cmp_VisitaPut(GenericRequest<Cmp_PutInModel<Cmp_VisitaModel>> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_DeleteOutModel>> Cmp_VisitaDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess);

        Task<GenericResult<Cmp_ListOutModel<Cmp_DpiModel>>> Cmp_DpiList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_PutOutModel<Cmp_DpiModel>>> Cmp_DpiPut(GenericRequest<Cmp_PutInModel<Cmp_DpiModel>> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_DeleteOutModel>> Cmp_DpiDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_ListOutModel<Cmp_DpiConsegnaModel>>> Cmp_DpiConsegnaList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_PutOutModel<Cmp_DpiConsegnaModel>>> Cmp_DpiConsegnaPut(GenericRequest<Cmp_PutInModel<Cmp_DpiConsegnaModel>> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_DeleteOutModel>> Cmp_DpiConsegnaDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess);

        Task<GenericResult<Cmp_ListOutModel<Cmp_NominaModel>>> Cmp_NominaList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_PutOutModel<Cmp_NominaModel>>> Cmp_NominaPut(GenericRequest<Cmp_PutInModel<Cmp_NominaModel>> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_DeleteOutModel>> Cmp_NominaDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_ListOutModel<Cmp_DocumentoModel>>> Cmp_DocumentoList(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_PutOutModel<Cmp_DocumentoModel>>> Cmp_DocumentoPut(GenericRequest<Cmp_PutInModel<Cmp_DocumentoModel>> model, Boolean isSubProcess);
        Task<GenericResult<Cmp_DeleteOutModel>> Cmp_DocumentoDelete(GenericRequest<Cmp_DeleteInModel> model, Boolean isSubProcess);

        Task<GenericResult<Cmp_MineOutModel>> Cmp_Mine(GenericRequest<Cmp_ListInModel> model, Boolean isSubProcess);
    }
}
