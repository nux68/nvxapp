using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Crm;
using nvxapp.server.service.ClientServer_Service.Crm._utility;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Crm
{
    /*
     Opportunita' (trattative) e pipeline (ANALISI_CRM.md §6.1). Ogni cambio di fase o di valore finisce nello storico
     (Crm_OpportunitaStorico), base dei report. Una fase "vinta" o "persa" chiude l'opportunita' (data di chiusura);
     tornando a una fase aperta la data si svuota. Il motivo di perdita serve solo nelle fasi "perse".
    */
    public class Crm_OpportunitaService : ServiceBase, ICrm_OpportunitaService
    {
        private readonly ICrm_OpportunitaRepository _Crm_Opportunita;
        private readonly ICrm_OpportunitaStoricoRepository _Crm_OpportunitaStorico;
        private readonly ICrm_Az_ClienteReadRepository _Az_Cliente;
        private readonly ICrm_Az_ClienteContattoReadRepository _Az_ClienteContatto;
        private readonly ICrm_AllegatoRepository _Crm_Allegato;
        private readonly ICrm_AttivitaRepository _Crm_Attivita;
        private readonly ICrm_NotaRepository _Crm_Nota;
        private readonly ICrm_OffertaRepository _Crm_Offerta;
        private readonly ICrm_Par_FaseRepository _Crm_Par_Fase;
        private readonly ICrm_Par_FonteRepository _Crm_Par_Fonte;
        private readonly ICrm_Par_MotivoPerditaRepository _Crm_Par_MotivoPerdita;
        private readonly ICrmUtility _utility;

        public Crm_OpportunitaService(IMapper mapper,
                                      UserManager<ApplicationUser> userManager,
                                      IAspNetUsersRepository aspNetUsersRepository,
                                      IOptions<JwtParameter> jwtParameter,
                                      IHttpContextAccessor httpContextAccessor,
                                      IConfiguration configuration,
                                      ICrm_OpportunitaRepository crm_OpportunitaRepository,
                                      ICrm_OpportunitaStoricoRepository crm_OpportunitaStoricoRepository,
                                      ICrm_Az_ClienteReadRepository az_ClienteRepository,
                                      ICrm_Az_ClienteContattoReadRepository az_ClienteContattoRepository,
                                      ICrm_AllegatoRepository crm_AllegatoRepository,
                                      ICrm_AttivitaRepository crm_AttivitaRepository,
                                      ICrm_NotaRepository crm_NotaRepository,
                                      ICrm_OffertaRepository crm_OffertaRepository,
                                      ICrm_Par_FaseRepository crm_Par_FaseRepository,
                                      ICrm_Par_FonteRepository crm_Par_FonteRepository,
                                      ICrm_Par_MotivoPerditaRepository crm_Par_MotivoPerditaRepository,
                                      ICrmUtility utility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Crm_Opportunita = crm_OpportunitaRepository;
            _Crm_OpportunitaStorico = crm_OpportunitaStoricoRepository;
            _Az_Cliente = az_ClienteRepository;
            _Az_ClienteContatto = az_ClienteContattoRepository;
            _Crm_Allegato = crm_AllegatoRepository;
            _Crm_Attivita = crm_AttivitaRepository;
            _Crm_Nota = crm_NotaRepository;
            _Crm_Offerta = crm_OffertaRepository;
            _Crm_Par_Fase = crm_Par_FaseRepository;
            _Crm_Par_Fonte = crm_Par_FonteRepository;
            _Crm_Par_MotivoPerdita = crm_Par_MotivoPerditaRepository;
            _utility = utility;
        }

        public virtual async Task<GenericResult<Crm_OpportunitaListOutModel>> Crm_OpportunitaList(GenericRequest<Crm_OpportunitaListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var f = model.Data;
                var q = _utility.OpportunitaVisibili(ctx);
                if (f.IdAz_Cliente.HasValue)
                    q = q.Where(o => o.IdAz_Cliente == f.IdAz_Cliente);
                if (!string.IsNullOrEmpty(f.IdResponsabile))
                    q = q.Where(o => o.IdAspNetUsersResponsabile == f.IdResponsabile);
                // senza "anche chiuse": aperte e chiuse negli ultimi 30 giorni (le colonne vinte/perse della pipeline)
                if (!f.AncheChiuse)
                {
                    var limite = DateTime.Today.AddDays(-30);
                    q = q.Where(o => o.Crm_Par_FaseNavigation!.Tipo == Crm_FaseTipo.Aperta || o.DataChiusura == null || o.DataChiusura >= limite);
                }
                return new Crm_OpportunitaListOutModel { Opportunita = await CrmLists.OpportunitaAsync(q, _utility) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_OpportunitaGetOutModel>> Crm_OpportunitaGet(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var o = await Visibile(ctx, model.Data.Id, false);
                var cfg = await _utility.CfgAsync(ctx);
                var fasi = await _Crm_Par_Fase.FindAll(_ => true).Where(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ToDictionaryAsync(x => x.Id, x => x.Descrizione);
                var storico = await _Crm_OpportunitaStorico.FindAll(_ => true).Where(x => x.IdCrm_Opportunita == o.Id).OrderByDescending(x => x.Data).ToListAsync();
                var nomi = await _utility.NomiAsync(storico.Select(s => s.IdAspNetUsers));
                return new Crm_OpportunitaGetOutModel
                {
                    Opportunita = Modello(o),
                    Cliente = await _Az_Cliente.FindAll(c => c.Id == o.IdAz_Cliente).Select(c => c.Descrizione).FirstAsync(),
                    Contatti = await CrmLists.ContattiAsync(_Az_ClienteContatto, o.IdAz_Cliente),
                    Storico = storico.Select(s => new Crm_StoricoModel
                    {
                        Data = s.Data, Utente = s.IdAspNetUsers != null && nomi.TryGetValue(s.IdAspNetUsers, out var n) ? n : null,
                        FaseDa = s.IdCrm_Par_FaseDa is int da && fasi.TryGetValue(da, out var fd) ? fd : null,
                        FaseA = fasi.TryGetValue(s.IdCrm_Par_FaseA, out var fa) ? fa : string.Empty, ValoreDa = s.ValoreDa, ValoreA = s.ValoreA
                    }).ToList(),
                    Attivita = (await CrmLists.AttivitaAsync(_Crm_Attivita.FindAll(a => a.IdCrm_Opportunita == o.Id), _utility)).OrderByDescending(a => a.Inizio).ToList(),
                    Offerte = await CrmLists.OfferteAsync(_Crm_Offerta.FindAll(x => x.IdCrm_Opportunita == o.Id), cfg),
                    Note = await CrmLists.NoteAsync(_Crm_Nota.FindAll(n => n.IdCrm_Opportunita == o.Id), ctx, _utility),
                    Allegati = await CrmLists.AllegatiAsync(_Crm_Allegato.FindAll(a => a.IdCrm_Opportunita == o.Id), _utility),
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_OpportunitaPutOutModel>> Crm_OpportunitaPut(GenericRequest<Crm_OpportunitaPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var d = model.Data.Opportunita;
                if (string.IsNullOrWhiteSpace(d.Titolo))
                    throw new UserMessageException(CrmMessages.Get("OpportunityTitleRequired"));
                await _utility.ClienteAsync(ctx, d.IdAz_Cliente);
                var fase = await Fase(ctx, d.IdCrm_Par_Fase);

                Crm_Opportunita o;
                int? faseDa = null;
                decimal? valoreDa = null;
                if (d.Id != 0)
                {
                    o = await Visibile(ctx, d.Id, true);
                    faseDa = o.IdCrm_Par_Fase;
                    valoreDa = o.Valore;
                }
                else
                {
                    o = new Crm_Opportunita { IdAz_Anagrafica = ctx.IdAz_Anagrafica, CreationDate = DateTime.Now, IdAspNetUsersResponsabile = ctx.Amministratore ? null : ctx.IdUtente };
                    await _Crm_Opportunita.CreateAsync(o, false);
                }

                var responsabile = string.IsNullOrEmpty(d.IdResponsabile) ? null : d.IdResponsabile;
                if (responsabile != o.IdAspNetUsersResponsabile && (d.Id != 0 || ctx.Amministratore))
                {
                    if (responsabile != null && !(await _utility.UtentiAsync(ctx)).Any(u => u.Id == responsabile))
                        throw new UserMessageException(CrmMessages.Get("UserInvalid"));
                    if (!ctx.Amministratore && !(responsabile == null || responsabile == ctx.IdUtente))
                        throw new UserMessageException(CrmMessages.Get("ResponsibleChangeNotAllowed"));
                    o.IdAspNetUsersResponsabile = responsabile;
                }

                o.Titolo = d.Titolo.Trim();
                o.IdAz_Cliente = d.IdAz_Cliente;
                o.IdAz_ClienteContatto = d.IdAz_ClienteContatto is int c && await _Az_ClienteContatto.FindAll(_ => true).AnyAsync(x => x.Id == c && x.IdAz_Cliente == d.IdAz_Cliente) ? c : null;
                o.Valore = Math.Max(0, d.Valore);
                o.Probabilita = Math.Clamp(d.Probabilita, 0, 100);
                o.DataChiusuraPrevista = d.DataChiusuraPrevista;
                o.IdCrm_Par_Fonte = d.IdCrm_Par_Fonte is int f && await _Crm_Par_Fonte.FindAll(_ => true).AnyAsync(x => x.Id == f && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica) ? f : null;
                o.Note = string.IsNullOrWhiteSpace(d.Note) ? null : d.Note.Trim();
                ApplicaFase(o, fase, d.IdCrm_Par_MotivoPerdita, faseDa == null || faseDa != fase.Id ? null : d.DataChiusura);
                if (fase.Tipo == Crm_FaseTipo.Persa)
                    o.IdCrm_Par_MotivoPerdita = d.IdCrm_Par_MotivoPerdita is int m && await _Crm_Par_MotivoPerdita.FindAll(_ => true).AnyAsync(x => x.Id == m && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica) ? m : null;
                o.ModifiedDate = DateTime.Now;
                o.ChangeUser = ctx.IdUtente;
                await _Crm_Opportunita.SaveChange();

                if (faseDa != o.IdCrm_Par_Fase || valoreDa != o.Valore)
                {
                    await _Crm_OpportunitaStorico.CreateAsync(new Crm_OpportunitaStorico
                    {
                        IdCrm_Opportunita = o.Id, Data = DateTime.Now, IdAspNetUsers = ctx.IdUtente, IdCrm_Par_FaseDa = faseDa,
                        IdCrm_Par_FaseA = o.IdCrm_Par_Fase, ValoreDa = valoreDa, ValoreA = o.Valore, CreationDate = DateTime.Now
                    }, false);
                    await _Crm_Opportunita.SaveChange();
                }
                return new Crm_OpportunitaPutOutModel { Opportunita = Modello(o) };
            }, isSubProcess);
        }

        // cambio di fase dalla pipeline (trascinamento)
        public virtual async Task<GenericResult<Crm_OpportunitaPutOutModel>> Crm_OpportunitaCambiaFase(GenericRequest<Crm_CambiaFaseInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var o = await Visibile(ctx, model.Data.Id, true);
                var fase = await Fase(ctx, model.Data.IdCrm_Par_Fase);
                if (o.IdCrm_Par_Fase == fase.Id)
                    return new Crm_OpportunitaPutOutModel { Opportunita = Modello(o) };

                int faseDa = o.IdCrm_Par_Fase;
                ApplicaFase(o, fase, model.Data.IdCrm_Par_MotivoPerdita, null);
                o.Probabilita = fase.Probabilita;
                if (fase.Tipo == Crm_FaseTipo.Persa)
                    o.IdCrm_Par_MotivoPerdita = model.Data.IdCrm_Par_MotivoPerdita is int m && await _Crm_Par_MotivoPerdita.FindAll(_ => true).AnyAsync(x => x.Id == m && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica) ? m : null;
                o.ModifiedDate = DateTime.Now;
                o.ChangeUser = ctx.IdUtente;
                await _Crm_OpportunitaStorico.CreateAsync(new Crm_OpportunitaStorico
                {
                    IdCrm_Opportunita = o.Id, Data = DateTime.Now, IdAspNetUsers = ctx.IdUtente, IdCrm_Par_FaseDa = faseDa,
                    IdCrm_Par_FaseA = fase.Id, ValoreDa = o.Valore, ValoreA = o.Valore, CreationDate = DateTime.Now
                }, false);
                await _Crm_Opportunita.SaveChange();
                return new Crm_OpportunitaPutOutModel { Opportunita = Modello(o) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Crm_IdOutModel>> Crm_OpportunitaDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var o = await Visibile(ctx, model.Data.Id, true);
                if (await _Crm_Allegato.FindAll(_ => true).AnyAsync(a => a.IdCrm_Opportunita == o.Id))
                    throw new UserMessageException(CrmMessages.Get("OpportunityHasAttachments"));
                // storico, attivita' e note si cancellano con l'opportunita'; le offerte restano senza collegamento
                await _Crm_Opportunita.DeleteAsync(o);
                await _Crm_Opportunita.SaveChange();
                return new Crm_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ utilita'
        private async Task<Crm_Opportunita> Visibile(CrmContesto ctx, int id, bool tracking)
        {
            var q = _utility.OpportunitaVisibili(ctx);
            q = tracking ? q.AsTracking() : q.AsNoTracking();
            return await q.FirstOrDefaultAsync(o => o.Id == id) ?? throw new UserMessageException(CrmMessages.Get("OpportunityNotFound"));
        }

        private async Task<Crm_Par_Fase> Fase(CrmContesto ctx, int id)
            => await _Crm_Par_Fase.FindAll(_ => true).FirstOrDefaultAsync(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
               ?? throw new UserMessageException(CrmMessages.Get("PhaseInvalid"));

        private static void ApplicaFase(Crm_Opportunita o, Crm_Par_Fase fase, int? idMotivo, DateTime? dataChiusura)
        {
            o.IdCrm_Par_Fase = fase.Id;
            if (fase.Tipo == Crm_FaseTipo.Aperta)
            {
                o.DataChiusura = null;
                o.IdCrm_Par_MotivoPerdita = null;
            }
            else
            {
                o.DataChiusura = dataChiusura ?? o.DataChiusura ?? DateTime.Today;
                if (fase.Tipo == Crm_FaseTipo.Vinta)
                {
                    o.IdCrm_Par_MotivoPerdita = null;
                    o.Probabilita = 100;
                }
                else
                    o.Probabilita = 0;
            }
        }

        private static Crm_OpportunitaModel Modello(Crm_Opportunita o) => new()
        {
            Id = o.Id, Titolo = o.Titolo, IdAz_Cliente = o.IdAz_Cliente, IdAz_ClienteContatto = o.IdAz_ClienteContatto, IdCrm_Par_Fase = o.IdCrm_Par_Fase,
            Valore = o.Valore, Probabilita = o.Probabilita, DataChiusuraPrevista = o.DataChiusuraPrevista, DataChiusura = o.DataChiusura,
            IdResponsabile = o.IdAspNetUsersResponsabile, IdCrm_Par_MotivoPerdita = o.IdCrm_Par_MotivoPerdita, IdCrm_Par_Fonte = o.IdCrm_Par_Fonte,
            Note = o.Note, IdCommessaPresenze = o.IdCommessaPresenze
        };
    }

    public interface ICrm_OpportunitaService : IServiceBase
    {
        Task<GenericResult<Crm_OpportunitaListOutModel>> Crm_OpportunitaList(GenericRequest<Crm_OpportunitaListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_OpportunitaGetOutModel>> Crm_OpportunitaGet(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_OpportunitaPutOutModel>> Crm_OpportunitaPut(GenericRequest<Crm_OpportunitaPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_OpportunitaPutOutModel>> Crm_OpportunitaCambiaFase(GenericRequest<Crm_CambiaFaseInModel> model, Boolean isSubProcess);
        Task<GenericResult<Crm_IdOutModel>> Crm_OpportunitaDelete(GenericRequest<Crm_IdInModel> model, Boolean isSubProcess);
    }
}
