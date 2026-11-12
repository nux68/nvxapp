using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Assets;
using nvxapp.server.service.ClientServer_Service.Assets._utility;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Assets
{
    /*
     Prenotazioni (ANALISI_ASSETS_APPLICATIVO.md, AST-PREN.1-2).
        - risorse prenotabili (postazioni, sale, auto in pool): le gestisce il CompanyPowerAdmin; tutti vedono quelle attive
        - prenotazioni: ogni utente dell'azienda prenota e gestisce le proprie; gli amministratori vedono (con il nome) e
          annullano tutte. Gli altri utenti vedono le prenotazioni degli altri solo come "occupato" (senza nome)
     Con i limiti di Ast_Cfg (durata massima, anticipo massimo). Approvazione facoltativa per risorsa (AST-D4,
     ANALISI_INFRASTRUCTURE_RICHIESTE.md): sulle risorse con RichiedeApprovazione la prenotazione di un utente non
     amministratore nasce InAttesa (anche quando la modifica) con una richiesta in Dip_GG_Richiesta (Common), che decide
     un amministratore nelle richieste comuni (AstRichiestaTipoHandler, motore RichiestaCommonService); le InAttesa
     occupano gia' la fascia. Richiesta, scadenza e motivo del rifiuto: AstApprovazioni.
     Conflitti: Ast_PrenotazioneRepository.SalvaSeLiberaAsync (transazione con lock sulla risorsa, vale anche per richieste concorrenti).
     Le prenotazioni dell'utente vanno nel suo calendario personale (feed .ics) con AstCalendarEventProvider (AST-PREN.3).
    */
    public class Ast_PrenotazioniService : ServiceBase, IAst_PrenotazioniService
    {
        public const string UrlPrenotazioni = "/astbookings";

        private readonly IAst_RisorsaRepository _Ast_Risorsa;
        private readonly IAst_PrenotazioneRepository _Ast_Prenotazione;
        private readonly IAst_VeicoloRepository _Ast_Veicolo;
        private readonly IAst_Az_SediReadRepository _Az_Sedi;
        private readonly IAstUtility _utility;
        private readonly IAstApprovazioni _approvazioni;

        public Ast_PrenotazioniService(IMapper mapper,
                                       UserManager<ApplicationUser> userManager,
                                       IAspNetUsersRepository aspNetUsersRepository,
                                       IOptions<JwtParameter> jwtParameter,
                                       IHttpContextAccessor httpContextAccessor,
                                       IConfiguration configuration,
                                       IAst_RisorsaRepository ast_RisorsaRepository,
                                       IAst_PrenotazioneRepository ast_PrenotazioneRepository,
                                       IAst_VeicoloRepository ast_VeicoloRepository,
                                       IAst_Az_SediReadRepository az_SediRepository,
                                       IAstUtility utility,
                                       IAstApprovazioni approvazioni) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Ast_Risorsa = ast_RisorsaRepository;
            _Ast_Prenotazione = ast_PrenotazioneRepository;
            _Ast_Veicolo = ast_VeicoloRepository;
            _Az_Sedi = az_SediRepository;
            _utility = utility;
            _approvazioni = approvazioni;
        }

        // ------------------------------------------------------------------------------------------------ risorse (AST-PREN.1)

        public virtual async Task<GenericResult<Ast_RisorsaListOutModel>> Ast_RisorsaList(GenericRequest<Ast_RisorsaListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                bool tutte = (model.Data?.Tutte ?? false) && ctx.PowerAdmin;
                var res = new Ast_RisorsaListOutModel
                {
                    Permessi = new Ast_PermessiModel { PowerAdmin = ctx.PowerAdmin, Admin = ctx.Admin },
                    Risorsa = await RisorseAsync(ctx, tutte),
                };
                if (ctx.PowerAdmin)
                {
                    res.Sede = await _utility.SediAsync(ctx);
                    res.VeicoloPool = (await _Ast_Veicolo.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Uso == AstUsoVeicolo.Pool && x.Stato == AstStatoVeicolo.Attivo)
                                                         .Select(x => new { x.Id, x.Marca, x.Modello, x.Targa }).ToListAsync())
                                      .Select(x => new Ast_LookupModel { Id = x.Id, Descrizione = Ast_FlottaService.DescrizioneVeicolo(x.Marca, x.Modello, x.Targa) })
                                      .OrderBy(x => x.Descrizione).ToList();
                }
                return res;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_RisorsaPutOutModel>> Ast_RisorsaPut(GenericRequest<Ast_RisorsaPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.PowerAdmin)
                    throw new UserMessageException(AssetsMessages.Get("NotAllowed"));
                var d = model.Data.Risorsa;
                var tipo = Enum.IsDefined(typeof(AstTipoRisorsa), d.Tipo) ? (AstTipoRisorsa)d.Tipo : AstTipoRisorsa.Postazione;
                var descr = AstUtility.Testo(d.Descrizione, 100) ?? throw new UserMessageException(AssetsMessages.Get("DescriptionRequired"));
                if (await _Ast_Risorsa.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Tipo == tipo && x.Descrizione == descr && x.Id != d.Id).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("ResourceDuplicate", descr));
                if (d.IdAz_Sedi != null && !await _Az_Sedi.FindAll(x => x.Id == d.IdAz_Sedi && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("NotFound"));
                int? veicolo = tipo == AstTipoRisorsa.AutoPool ? d.IdAst_Veicolo : null;
                if (tipo == AstTipoRisorsa.AutoPool
                    && (veicolo == null || !await _Ast_Veicolo.FindAll(x => x.Id == veicolo && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Uso == AstUsoVeicolo.Pool).AnyAsync()))
                    throw new UserMessageException(AssetsMessages.Get("ResourcePoolVehicleRequired"));

                Ast_Risorsa r;
                if (d.Id == 0)
                    r = new Ast_Risorsa { IdAz_Anagrafica = ctx.IdAz_Anagrafica };
                else
                    r = await _Ast_Risorsa.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                r.Tipo = tipo;
                r.Descrizione = descr;
                r.IdAz_Sedi = d.IdAz_Sedi;
                r.Capienza = tipo == AstTipoRisorsa.Sala && d.Capienza is > 0 ? d.Capienza : null;
                r.IdAst_Veicolo = veicolo;
                r.Attiva = d.Attiva;
                r.RichiedeApprovazione = d.RichiedeApprovazione;
                r.Note = AstUtility.Testo(d.Note, 500);
                if (r.Id == 0)
                    await _Ast_Risorsa.CreateAsync(r);
                else
                {
                    r.ModifiedDate = DateTime.Now;
                    r.ChangeUser = ctx.IdUtente;
                    await _Ast_Risorsa.SaveChange();
                }
                return new Ast_RisorsaPutOutModel { Risorsa = (await RisorseAsync(ctx, true)).First(x => x.Id == r.Id) };
            }, isSubProcess);
        }

        // una risorsa con prenotazioni future confermate o in attesa non si cancella (si disattiva)
        public virtual async Task<GenericResult<Ast_DeleteOutModel>> Ast_RisorsaDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                if (!ctx.PowerAdmin)
                    throw new UserMessageException(AssetsMessages.Get("NotAllowed"));
                var id = model.Data.Id;
                var ora = DateTime.Now;
                if (await _Ast_Prenotazione.FindAll(x => x.IdAst_Risorsa == id && (x.Stato == AstStatoPrenotazione.Confermata || x.Stato == AstStatoPrenotazione.InAttesa) && x.Fine > ora).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("ResourceHasBookings"));
                int n = await _Ast_Risorsa.FindAll(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ExecuteDeleteAsync();
                if (n == 0)
                    throw new UserMessageException(AssetsMessages.Get("NotFound"));
                return new Ast_DeleteOutModel { Eliminato = true };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ prenotazioni (AST-PREN.2)

        public virtual async Task<GenericResult<Ast_PrenotazioneListOutModel>> Ast_PrenotazioneList(GenericRequest<Ast_PrenotazioneListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                await _approvazioni.ScadiAsync(ctx);
                var d = model.Data;
                var dal = d.Dal == DateTime.MinValue ? DateTime.Today : d.Dal;
                var al = d.Al <= dal ? dal.AddDays(7) : d.Al;
                if ((al - dal).TotalDays > 62)
                    al = dal.AddDays(62);
                var cfg = await _utility.CfgAsync(ctx);
                // occupano la risorsa le confermate e quelle in attesa di approvazione; nelle proprie anche le rifiutate (con il motivo)
                var q = _Ast_Prenotazione.FindAll(x => x.Ast_RisorsaNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Inizio < al && x.Fine > dal);
                if (d.IdAst_Risorsa != null)
                    q = q.Where(x => x.IdAst_Risorsa == d.IdAst_Risorsa);
                if (d.SoloMie)
                    q = q.Where(x => x.IdAspNetUsers == ctx.IdUtente
                                     && (x.Stato == AstStatoPrenotazione.Confermata || x.Stato == AstStatoPrenotazione.InAttesa || x.Stato == AstStatoPrenotazione.Rifiutata));
                else
                    q = q.Where(x => x.Stato == AstStatoPrenotazione.Confermata || x.Stato == AstStatoPrenotazione.InAttesa);
                var righe = await q.Include(x => x.Ast_RisorsaNavigation).OrderBy(x => x.Inizio).Take(2000).ToListAsync();
                var nomi = await _approvazioni.NomiAsync(ctx, righe.Select(x => x.IdAspNetUsers));
                var motivi = await _approvazioni.MotiviAsync(righe);
                return new Ast_PrenotazioneListOutModel
                {
                    Permessi = new Ast_PermessiModel { PowerAdmin = ctx.PowerAdmin, Admin = ctx.Admin },
                    Prenotazione = righe.Select(x => PrenotazioneModel(ctx, x, nomi, motivi.GetValueOrDefault(x.Id))).ToList(),
                    Risorsa = await RisorseAsync(ctx, false),
                    DurataMaxOre = cfg.PrenotazioneDurataMaxOre,
                    AnticipoMaxGiorni = cfg.PrenotazioneAnticipoMaxGiorni,
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_PrenotazionePutOutModel>> Ast_PrenotazionePut(GenericRequest<Ast_PrenotazionePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                await _approvazioni.ScadiAsync(ctx);
                var d = model.Data.Prenotazione;
                var cfg = await _utility.CfgAsync(ctx);
                var ora = DateTime.Now;

                Ast_Prenotazione p;
                if (d.Id == 0)
                    p = new Ast_Prenotazione { IdAspNetUsers = ctx.IdUtente, Stato = AstStatoPrenotazione.Confermata };
                else
                {
                    p = await _Ast_Prenotazione.FindAll(x => x.Id == d.Id && x.Ast_RisorsaNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                    if (p.IdAspNetUsers != ctx.IdUtente && !ctx.Amministratore)
                        throw new UserMessageException(AssetsMessages.Get("NotAllowed"));
                    if ((p.Stato != AstStatoPrenotazione.Confermata && p.Stato != AstStatoPrenotazione.InAttesa) || p.Fine <= ora)
                        throw new UserMessageException(AssetsMessages.Get("BookingNotEditable"));
                }
                var risorsa = await _Ast_Risorsa.FindAll(x => x.Id == d.IdAst_Risorsa && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Attiva).FirstOrDefaultAsync()
                              ?? throw new UserMessageException(AssetsMessages.Get("ResourceRequired"));
                var inizio = TroncaMinuti(d.Inizio);
                var fine = TroncaMinuti(d.Fine);
                if (inizio == DateTime.MinValue || fine <= inizio)
                    throw new UserMessageException(AssetsMessages.Get("InvalidPeriod"));
                if (fine <= ora)
                    throw new UserMessageException(AssetsMessages.Get("BookingInThePast"));
                if (cfg.PrenotazioneDurataMaxOre > 0 && (fine - inizio).TotalHours > cfg.PrenotazioneDurataMaxOre)
                    throw new UserMessageException(AssetsMessages.Get("BookingTooLong", cfg.PrenotazioneDurataMaxOre));
                if (cfg.PrenotazioneAnticipoMaxGiorni > 0 && inizio > DateTime.Today.AddDays(cfg.PrenotazioneAnticipoMaxGiorni + 1))
                    throw new UserMessageException(AssetsMessages.Get("BookingTooFar", cfg.PrenotazioneAnticipoMaxGiorni));

                // com'era prima della modifica: se la nuova richiesta non si crea, la prenotazione torna cosi'
                var prima = new { p.IdAst_Risorsa, p.Inizio, p.Fine, p.Titolo, p.Note, p.Stato, p.IdDip_GG_Richiesta };

                p.IdAst_Risorsa = risorsa.Id;
                p.Inizio = inizio;
                p.Fine = fine;
                p.Titolo = AstUtility.Testo(d.Titolo, 200);
                p.Note = AstUtility.Testo(d.Note, 500);
                // risorsa con approvazione: la prenotazione (o la modifica) di chi non e' amministratore va approvata;
                // quella di un amministratore e' confermata subito (se modifica una richiesta in attesa, la approva)
                bool daApprovare = risorsa.RichiedeApprovazione && !ctx.Amministratore;
                bool eraInAttesa = p.Stato == AstStatoPrenotazione.InAttesa;
                var richiesta = p.Id == 0 ? null : await _approvazioni.RichiestaAsync(p.IdDip_GG_Richiesta);
                // modifica di una prenotazione ancora in attesa da parte del richiedente: si aggiorna il periodo della stessa
                // richiesta (gli amministratori la vedono gia', niente nuova notifica); se era confermata (o la richiesta non e'
                // piu' aperta) se ne crea una nuova e la vecchia, approvata, si annulla
                bool stessaRichiesta = daApprovare && eraInAttesa && richiesta != null && RichiestaRegole.InAttesa(richiesta);
                bool nuovaRichiesta = daApprovare && !stessaRichiesta;
                p.Stato = daApprovare ? AstStatoPrenotazione.InAttesa : AstStatoPrenotazione.Confermata;
                if (nuovaRichiesta)
                    p.IdDip_GG_Richiesta = null;
                if (!await _Ast_Prenotazione.SalvaSeLiberaAsync(p))
                    throw new UserMessageException(AssetsMessages.Get("BookingConflict", risorsa.Descrizione));
                p.Ast_RisorsaNavigation = risorsa;

                if (stessaRichiesta)
                    await _approvazioni.AggiornaPeriodoAsync(richiesta!, p);
                else if (nuovaRichiesta)
                {
                    Func<Task>? compensa = d.Id == 0 ? null : async () =>
                        await _Ast_Prenotazione.FindAll(x => x.Id == p.Id).ExecuteUpdateAsync(u => u
                            .SetProperty(x => x.IdAst_Risorsa, prima.IdAst_Risorsa).SetProperty(x => x.Inizio, prima.Inizio)
                            .SetProperty(x => x.Fine, prima.Fine).SetProperty(x => x.Titolo, prima.Titolo).SetProperty(x => x.Note, prima.Note)
                            .SetProperty(x => x.Stato, prima.Stato).SetProperty(x => x.IdDip_GG_Richiesta, prima.IdDip_GG_Richiesta));
                    await _approvazioni.CreaRichiestaAsync(ctx, p, risorsa.Descrizione, compensa);
                    if (richiesta != null && richiesta.IdAspNetUsers == ctx.IdUtente && richiesta.RichiestaStato == StatoRichiesta.Approvata)
                        await _approvazioni.AnnullaRichiestaAsync(ctx, new Ast_Prenotazione { IdDip_GG_Richiesta = richiesta.Id, Inizio = prima.Inizio, Fine = prima.Fine }, risorsa.Descrizione);
                }
                else if (eraInAttesa && richiesta != null && RichiestaRegole.InAttesa(richiesta))
                {
                    // confermata da un amministratore (approva la richiesta) o risorsa che non richiede piu' approvazione (si annulla)
                    if (ctx.Amministratore)
                        await _approvazioni.ApprovaRichiestaAsync(ctx, richiesta, p, risorsa.Descrizione);
                    else
                        await _approvazioni.AnnullaRichiestaAsync(ctx, p, risorsa.Descrizione);
                }

                var nomi = await _approvazioni.NomiAsync(ctx, new[] { p.IdAspNetUsers });
                return new Ast_PrenotazionePutOutModel { Prenotazione = PrenotazioneModel(ctx, p, nomi) };
            }, isSubProcess);
        }

        // annulla la propria prenotazione (o, per gli amministratori, quella di un altro: l'interessato riceve la notifica)
        public virtual async Task<GenericResult<Ast_PrenotazionePutOutModel>> Ast_PrenotazioneAnnulla(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var p = await _Ast_Prenotazione.FindAll(x => x.Id == model.Data.Id && x.Ast_RisorsaNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                               .Include(x => x.Ast_RisorsaNavigation).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                if (p.IdAspNetUsers != ctx.IdUtente && !ctx.Amministratore)
                    throw new UserMessageException(AssetsMessages.Get("NotAllowed"));
                if (p.Stato == AstStatoPrenotazione.Confermata || p.Stato == AstStatoPrenotazione.InAttesa)
                {
                    // prima la richiesta collegata (il motore ricontrolla i permessi): il richiedente la annulla; un amministratore
                    // che annulla la prenotazione in attesa di un altro la rifiuta (il richiedente riceve l'esito dal motore)
                    bool rifiutata = false;
                    var richiesta = await _approvazioni.RichiestaAsync(p.IdDip_GG_Richiesta);
                    if (richiesta != null)
                    {
                        if (p.IdAspNetUsers == ctx.IdUtente)
                            await _approvazioni.AnnullaRichiestaAsync(ctx, p, p.Ast_RisorsaNavigation!.Descrizione);
                        else if (ctx.Amministratore && RichiestaRegole.InAttesa(richiesta))
                        {
                            await _approvazioni.RifiutaRichiestaAsync(ctx, richiesta, p, p.Ast_RisorsaNavigation!.Descrizione, null);
                            rifiutata = true;
                        }
                    }
                    p.Stato = rifiutata ? AstStatoPrenotazione.Rifiutata : AstStatoPrenotazione.Annullata;
                    p.DataAnnullamento = DateTime.Now;
                    p.ModifiedDate = DateTime.Now;
                    p.ChangeUser = ctx.IdUtente;
                    await _Ast_Prenotazione.SaveChange();
                    if (p.IdAspNetUsers != ctx.IdUtente && !rifiutata)
                        await _utility.NotificaAsync(ctx, AstNotificationCategoryProvider.Prenotazioni, new[] { p.IdAspNetUsers },
                                                     AssetsMessages.Get("BookingCancelledTitle", p.Ast_RisorsaNavigation!.Descrizione),
                                                     AssetsMessages.Get("BookingCancelledText", p.Ast_RisorsaNavigation.Descrizione, p.Inizio.ToString("dd/MM/yyyy HH:mm"), p.Fine.ToString("HH:mm")),
                                                     UrlPrenotazioni);
                }
                var nomi = await _approvazioni.NomiAsync(ctx, new[] { p.IdAspNetUsers });
                var motivi = await _approvazioni.MotiviAsync(new[] { p });
                return new Ast_PrenotazionePutOutModel { Prenotazione = PrenotazioneModel(ctx, p, nomi, motivi.GetValueOrDefault(p.Id)) };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ supporto

        private static DateTime TroncaMinuti(DateTime d) => new(d.Year, d.Month, d.Day, d.Hour, d.Minute, 0, DateTimeKind.Unspecified);

        private async Task<List<Ast_RisorsaModel>> RisorseAsync(AstContesto ctx, bool tutte)
            => (await _Ast_Risorsa.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && (tutte || x.Attiva))
                                  .Select(x => new
                                  {
                                      x.Id, x.Tipo, x.Descrizione, x.IdAz_Sedi, Sede = x.Az_SediNavigation != null ? x.Az_SediNavigation.Descrizione : null,
                                      x.Capienza, x.IdAst_Veicolo, Targa = x.Ast_VeicoloNavigation != null ? x.Ast_VeicoloNavigation.Targa : null,
                                      Marca = x.Ast_VeicoloNavigation != null ? x.Ast_VeicoloNavigation.Marca : null,
                                      Modello = x.Ast_VeicoloNavigation != null ? x.Ast_VeicoloNavigation.Modello : null, x.Attiva, x.Note,
                                      x.RichiedeApprovazione,
                                  })
                                  .ToListAsync())
               .Select(x => new Ast_RisorsaModel
               {
                   Id = x.Id, Tipo = (int)x.Tipo, Descrizione = x.Descrizione, IdAz_Sedi = x.IdAz_Sedi, Sede = x.Sede, Capienza = x.Capienza,
                   IdAst_Veicolo = x.IdAst_Veicolo, Veicolo = x.Targa == null ? null : Ast_FlottaService.DescrizioneVeicolo(x.Marca, x.Modello, x.Targa),
                   Attiva = x.Attiva, Note = x.Note, RichiedeApprovazione = x.RichiedeApprovazione,
               })
               .OrderBy(x => x.Tipo).ThenBy(x => x.Descrizione).ToList();

        private static Ast_PrenotazioneModel PrenotazioneModel(AstContesto ctx, Ast_Prenotazione p, Dictionary<string, string> nomi, string? motivoRifiuto = null)
        {
            bool mia = p.IdAspNetUsers == ctx.IdUtente;
            return new Ast_PrenotazioneModel
            {
                Id = p.Id,
                IdAst_Risorsa = p.IdAst_Risorsa,
                Risorsa = p.Ast_RisorsaNavigation?.Descrizione,
                TipoRisorsa = (int)(p.Ast_RisorsaNavigation?.Tipo ?? AstTipoRisorsa.Postazione),
                Inizio = p.Inizio,
                Fine = p.Fine,
                // gli altri utenti vedono solo che la risorsa e' occupata
                Titolo = mia || ctx.Amministratore ? p.Titolo : null,
                Note = mia || ctx.Amministratore ? p.Note : null,
                Stato = (int)p.Stato,
                Mia = mia,
                Utente = mia || ctx.Amministratore ? (nomi.TryGetValue(p.IdAspNetUsers, out var n) ? n : null) : null,
                Modificabile = (mia || ctx.Amministratore) && (p.Stato == AstStatoPrenotazione.Confermata || p.Stato == AstStatoPrenotazione.InAttesa)
                               && p.Fine > DateTime.Now,
                MotivoRifiuto = mia || ctx.Amministratore ? motivoRifiuto : null,
            };
        }
    }

    public interface IAst_PrenotazioniService : IServiceBase
    {
        Task<GenericResult<Ast_RisorsaListOutModel>> Ast_RisorsaList(GenericRequest<Ast_RisorsaListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_RisorsaPutOutModel>> Ast_RisorsaPut(GenericRequest<Ast_RisorsaPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_DeleteOutModel>> Ast_RisorsaDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_PrenotazioneListOutModel>> Ast_PrenotazioneList(GenericRequest<Ast_PrenotazioneListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_PrenotazionePutOutModel>> Ast_PrenotazionePut(GenericRequest<Ast_PrenotazionePutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_PrenotazionePutOutModel>> Ast_PrenotazioneAnnulla(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
    }
}
