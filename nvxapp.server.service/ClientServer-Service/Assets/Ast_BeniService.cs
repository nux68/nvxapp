using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Assets;
using nvxapp.server.service.ClientServer_Service.Assets._utility;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Assets
{
    /*
     Beni assegnati (ANALISI_ASSETS_APPLICATIVO.md, AST-BENI.1-3).
        - amministratori: categorie, beni, consegne e restituzioni, elenco dei beni da restituire alla cessazione
        - utente: i beni che ha in mano e lo stato dei verbali (Ast_MieiBeniGet)
     Verbali: alla consegna e alla restituzione si chiede la presa visione al dipendente (IAcknowledgementService, fonti
     "ast.beni.consegna" e "ast.beni.restituzione", riferimento = assegnazione); con il documentale attivo la fonte fa generare
     il verbale di consegna dei beni da firmare. Un dipendente senza utente non riceve il verbale (si registra solo la consegna).
     Garanzia del bene nello scadenzario comune (fonte "ast.bene.garanzia").
    */
    public class Ast_BeniService : ServiceBase, IAst_BeniService
    {
        public const string FonteConsegna = "ast.beni.consegna";
        public const string FonteRestituzione = "ast.beni.restituzione";
        public const string FonteGaranzia = "ast.bene.garanzia";
        public const string UrlBene = "/astgoodedit";

        private readonly IAst_CategoriaRepository _Ast_Categoria;
        private readonly IAst_BeneRepository _Ast_Bene;
        private readonly IAst_BeneAssegnazioneRepository _Ast_BeneAssegnazione;
        private readonly IAst_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly IAst_Az_SediReadRepository _Az_Sedi;
        private readonly IPresaVisioneRepository _presaVisioneRepository;
        private readonly IAcknowledgementService _acknowledgementService;
        private readonly IDeadlineRegistry _deadlineRegistry;
        private readonly IAstRestituzioni _restituzioni;
        private readonly IAstUtility _utility;

        public Ast_BeniService(IMapper mapper,
                               UserManager<ApplicationUser> userManager,
                               IAspNetUsersRepository aspNetUsersRepository,
                               IOptions<JwtParameter> jwtParameter,
                               IHttpContextAccessor httpContextAccessor,
                               IConfiguration configuration,
                               IAst_CategoriaRepository ast_CategoriaRepository,
                               IAst_BeneRepository ast_BeneRepository,
                               IAst_BeneAssegnazioneRepository ast_BeneAssegnazioneRepository,
                               IAst_Dip_AnagraficaReadRepository dip_AnagraficaRepository,
                               IAst_Az_SediReadRepository az_SediRepository,
                               IPresaVisioneRepository presaVisioneRepository,
                               IAcknowledgementService acknowledgementService,
                               IDeadlineRegistry deadlineRegistry,
                               IAstRestituzioni restituzioni,
                               IAstUtility utility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Ast_Categoria = ast_CategoriaRepository;
            _Ast_Bene = ast_BeneRepository;
            _Ast_BeneAssegnazione = ast_BeneAssegnazioneRepository;
            _Dip_Anagrafica = dip_AnagraficaRepository;
            _Az_Sedi = az_SediRepository;
            _presaVisioneRepository = presaVisioneRepository;
            _acknowledgementService = acknowledgementService;
            _deadlineRegistry = deadlineRegistry;
            _restituzioni = restituzioni;
            _utility = utility;
        }

        // ------------------------------------------------------------------------------------------------ categorie

        public virtual async Task<GenericResult<Ast_CategoriaListOutModel>> Ast_CategoriaList(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                return new Ast_CategoriaListOutModel { Categoria = await CategorieAsync(ctx) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_CategoriaPutOutModel>> Ast_CategoriaPut(GenericRequest<Ast_CategoriaPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var d = model.Data.Categoria;
                var descr = AstUtility.Testo(d.Descrizione, 100) ?? throw new UserMessageException(AssetsMessages.Get("DescriptionRequired"));
                if (await _Ast_Categoria.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Descrizione == descr && x.Id != d.Id).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("CategoryDuplicate", descr));
                Ast_Categoria c;
                if (d.Id == 0)
                    c = new Ast_Categoria { IdAz_Anagrafica = ctx.IdAz_Anagrafica };
                else
                    c = await _Ast_Categoria.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                c.Descrizione = descr;
                c.Attivo = d.Attivo;
                c.Ordine = d.Ordine;
                if (c.Id == 0)
                    await _Ast_Categoria.CreateAsync(c);
                else
                {
                    c.ModifiedDate = DateTime.Now;
                    c.ChangeUser = ctx.IdUtente;
                    await _Ast_Categoria.SaveChange();
                }
                return new Ast_CategoriaPutOutModel { Categoria = new Ast_CategoriaModel { Id = c.Id, Descrizione = c.Descrizione, Attivo = c.Attivo, Ordine = c.Ordine } };
            }, isSubProcess);
        }

        // una categoria usata dai beni non si cancella (si disattiva)
        public virtual async Task<GenericResult<Ast_DeleteOutModel>> Ast_CategoriaDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var id = model.Data.Id;
                if (await _Ast_Bene.FindAll(x => x.IdAst_Categoria == id).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("CategoryInUse"));
                int n = await _Ast_Categoria.FindAll(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ExecuteDeleteAsync();
                if (n == 0)
                    throw new UserMessageException(AssetsMessages.Get("NotFound"));
                return new Ast_DeleteOutModel { Eliminato = true };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ beni (AST-BENI.1)

        public virtual async Task<GenericResult<Ast_BeneListOutModel>> Ast_BeneList(GenericRequest<Ast_BeneListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                bool dismessi = model.Data?.Dismessi ?? false;
                var righe = await _Ast_Bene.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && (dismessi || x.Stato != AstStatoBene.Dismesso))
                                           .Select(x => new
                                           {
                                               x.Id, x.CodiceInventario, x.Descrizione, Categoria = x.Ast_CategoriaNavigation!.Descrizione, x.NumeroSerie,
                                               Sede = x.Az_SediNavigation != null ? x.Az_SediNavigation.Descrizione : null, x.Stato, x.ScadenzaGaranzia,
                                               Aperta = x.Ast_BeneAssegnazione!.Where(a => a.DataRestituzione == null)
                                                                               .Select(a => new { a.DataConsegna, a.Dip_AnagraficaNavigation!.Cognome, a.Dip_AnagraficaNavigation.Nome })
                                                                               .FirstOrDefault(),
                                           })
                                           .ToListAsync();
                return new Ast_BeneListOutModel
                {
                    Permessi = new Ast_PermessiModel { PowerAdmin = ctx.PowerAdmin, Admin = ctx.Admin },
                    Bene = righe.Select(x => new Ast_BeneListModel
                    {
                        Id = x.Id, CodiceInventario = x.CodiceInventario, Descrizione = x.Descrizione, Categoria = x.Categoria, NumeroSerie = x.NumeroSerie,
                        Sede = x.Sede, Stato = (int)x.Stato, ScadenzaGaranzia = x.ScadenzaGaranzia,
                        Assegnatario = x.Aperta == null ? null : AstUtility.Nominativo(x.Aperta.Cognome, x.Aperta.Nome),
                        DataConsegna = x.Aperta?.DataConsegna,
                    }).OrderBy(x => x.CodiceInventario).ToList(),
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_BeneGetOutModel>> Ast_BeneGet(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var res = new Ast_BeneGetOutModel
                {
                    Categoria = await CategorieAsync(ctx),
                    Sede = await _utility.SediAsync(ctx),
                    Dipendente = await _utility.DipendentiAsync(ctx),
                };
                int id = model.Data?.Id ?? 0;
                if (id == 0)
                    return res;
                var b = await BeneAsync(ctx, id);
                res.Bene = BeneModel(b);
                var assegnazioni = await _Ast_BeneAssegnazione.FindAll(x => x.IdAst_Bene == id).OrderByDescending(x => x.DataConsegna).ToListAsync();
                var nomi = res.Dipendente.ToDictionary(x => x.Id, x => x.Nominativo);
                res.Assegnazione = await AssegnazioniModelAsync(ctx, assegnazioni, new Dictionary<int, Ast_Bene> { { b.Id, b } }, nomi);
                return res;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_BenePutOutModel>> Ast_BenePut(GenericRequest<Ast_BenePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var d = model.Data.Bene;
                var codice = AstUtility.Testo(d.CodiceInventario, 30)?.ToUpperInvariant() ?? throw new UserMessageException(AssetsMessages.Get("InventoryCodeRequired"));
                var descr = AstUtility.Testo(d.Descrizione, 200) ?? throw new UserMessageException(AssetsMessages.Get("DescriptionRequired"));
                if (await _Ast_Bene.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.CodiceInventario == codice && x.Id != d.Id).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("InventoryCodeDuplicate", codice));
                if (!await _Ast_Categoria.FindAll(x => x.Id == d.IdAst_Categoria && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("CategoryRequired"));
                if (d.IdAz_Sedi != null && !await _Az_Sedi.FindAll(x => x.Id == d.IdAz_Sedi && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("NotFound"));

                Ast_Bene b;
                if (d.Id == 0)
                    b = new Ast_Bene { IdAz_Anagrafica = ctx.IdAz_Anagrafica, Stato = AstStatoBene.Disponibile };
                else
                    b = await _Ast_Bene.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                bool assegnato = d.Id != 0 && await _Ast_BeneAssegnazione.FindAll(x => x.IdAst_Bene == b.Id && x.DataRestituzione == null).AnyAsync();
                b.CodiceInventario = codice;
                b.IdAst_Categoria = d.IdAst_Categoria;
                b.Descrizione = descr;
                b.MarcaModello = AstUtility.Testo(d.MarcaModello, 100);
                b.NumeroSerie = AstUtility.Testo(d.NumeroSerie, 100);
                b.DataAcquisto = d.DataAcquisto?.Date;
                b.Valore = d.Valore is > 0 ? Math.Round(d.Valore.Value, 2) : null;
                b.ScadenzaGaranzia = d.ScadenzaGaranzia?.Date;
                b.IdAz_Sedi = d.IdAz_Sedi;
                // "assegnato" lo decidono consegna e restituzione; gli altri stati si scelgono solo con il bene in magazzino
                if (assegnato)
                    b.Stato = AstStatoBene.Assegnato;
                else
                    b.Stato = d.Stato == (int)AstStatoBene.InRiparazione ? AstStatoBene.InRiparazione
                              : d.Stato == (int)AstStatoBene.Dismesso ? AstStatoBene.Dismesso : AstStatoBene.Disponibile;
                b.Note = AstUtility.Testo(d.Note, 2000);
                if (b.Id == 0)
                    await _Ast_Bene.CreateAsync(b);
                else
                {
                    b.ModifiedDate = DateTime.Now;
                    b.ChangeUser = ctx.IdUtente;
                    await _Ast_Bene.SaveChange();
                }
                await RegistraGaranziaAsync(ctx, b);
                return new Ast_BenePutOutModel { Bene = BeneModel(b) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_DeleteOutModel>> Ast_BeneDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var b = await BeneAsync(ctx, model.Data.Id);
                if (await _Ast_BeneAssegnazione.FindAll(x => x.IdAst_Bene == b.Id && x.DataRestituzione == null).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("GoodAssignedNoDelete"));
                var assegnazioni = await _Ast_BeneAssegnazione.FindAll(x => x.IdAst_Bene == b.Id).Select(x => x.Id).ToListAsync();
                await _Ast_Bene.FindAll(x => x.Id == b.Id).ExecuteDeleteAsync();
                await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.Assets, FonteGaranzia, b.Id);
                foreach (var id in assegnazioni)
                {
                    await _acknowledgementService.CancelAsync(ctx.IdCompany, ApplicationType.Assets, FonteConsegna, id);
                    await _acknowledgementService.CancelAsync(ctx.IdCompany, ApplicationType.Assets, FonteRestituzione, id);
                    await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.Assets, AstRestituzioni.FonteBene, id);
                }
                return new Ast_DeleteOutModel { Eliminato = true };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ consegna e restituzione (AST-BENI.2)

        public virtual async Task<GenericResult<Ast_BeneAssegnazionePutOutModel>> Ast_ConsegnaPut(GenericRequest<Ast_ConsegnaPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var d = model.Data;
                var b = await BeneAsync(ctx, d.IdAst_Bene);
                if (b.Stato != AstStatoBene.Disponibile || await _Ast_BeneAssegnazione.FindAll(x => x.IdAst_Bene == b.Id && x.DataRestituzione == null).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("GoodNotAvailable"));
                var dip = await _Dip_Anagrafica.FindAll(x => x.Id == d.IdDip_Anagrafica && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                          ?? throw new UserMessageException(AssetsMessages.Get("EmployeeRequired"));
                if (d.DataConsegna == DateTime.MinValue)
                    throw new UserMessageException(AssetsMessages.Get("DateRequired"));

                var a = new Ast_BeneAssegnazione
                {
                    IdAst_Bene = b.Id,
                    IdDip_Anagrafica = dip.Id,
                    DataConsegna = d.DataConsegna.Date,
                    CondizioniConsegna = AstUtility.Testo(d.CondizioniConsegna, 500),
                    Note = AstUtility.Testo(d.Note, 500),
                };
                try
                {
                    await _Ast_BeneAssegnazione.CreateAsync(a);
                }
                catch (DbUpdateException)
                {
                    // consegna concorrente dello stesso bene: l'indice unico delle assegnazioni aperte la rifiuta
                    throw new UserMessageException(AssetsMessages.Get("GoodNotAvailable"));
                }
                await _Ast_Bene.FindAll(x => x.Id == b.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.Stato, AstStatoBene.Assegnato));
                b.Stato = AstStatoBene.Assegnato;

                var nominativo = AstUtility.Nominativo(dip.Cognome, dip.Nome);
                if (!string.IsNullOrEmpty(dip.IdAspNetUsers))
                    await _acknowledgementService.RequestAsync(new AcknowledgementRequest
                    {
                        IdCompany = ctx.IdCompany,
                        Application = ApplicationType.Assets,
                        Fonte = FonteConsegna,
                        IdRiferimento = a.Id,
                        UserIds = new[] { dip.IdAspNetUsers },
                        Titolo = AssetsMessages.Get("DeliveryReportTitle", b.CodiceInventario, b.Descrizione),
                        Testo = TestoVerbale(b, await CategoriaAsync(b.IdAst_Categoria), nominativo, AssetsMessages.Get("DeliveryReportText", a.DataConsegna.ToString("dd/MM/yyyy")), a.CondizioniConsegna),
                        Url = "/astmine",
                    });
                var res = await AssegnazioniModelAsync(ctx, new List<Ast_BeneAssegnazione> { a }, new Dictionary<int, Ast_Bene> { { b.Id, b } },
                                                       new Dictionary<int, string> { { dip.Id, nominativo } });
                return new Ast_BeneAssegnazionePutOutModel { Assegnazione = res[0] };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_BeneAssegnazionePutOutModel>> Ast_RestituzionePut(GenericRequest<Ast_RestituzionePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var d = model.Data;
                var a = await _Ast_BeneAssegnazione.FindAll(x => x.Id == d.IdAst_BeneAssegnazione && x.Ast_BeneNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                                   .AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                if (a.DataRestituzione != null)
                    throw new UserMessageException(AssetsMessages.Get("GoodAlreadyReturned"));
                if (d.DataRestituzione == DateTime.MinValue || d.DataRestituzione.Date < a.DataConsegna)
                    throw new UserMessageException(AssetsMessages.Get("InvalidPeriod"));
                var b = await BeneAsync(ctx, a.IdAst_Bene);
                var dip = await _Dip_Anagrafica.FindAll(x => x.Id == a.IdDip_Anagrafica).FirstAsync();

                a.DataRestituzione = d.DataRestituzione.Date;
                a.CondizioniRestituzione = AstUtility.Testo(d.CondizioniRestituzione, 500);
                a.ModifiedDate = DateTime.Now;
                a.ChangeUser = ctx.IdUtente;
                await _Ast_BeneAssegnazione.SaveChange();
                var stato = d.StatoBene == (int)AstStatoBene.InRiparazione ? AstStatoBene.InRiparazione
                            : d.StatoBene == (int)AstStatoBene.Dismesso ? AstStatoBene.Dismesso : AstStatoBene.Disponibile;
                await _Ast_Bene.FindAll(x => x.Id == b.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.Stato, stato));
                b.Stato = stato;
                // restituito: la scadenza della restituzione alla cessazione si chiude
                await _deadlineRegistry.CloseAsync(ctx.IdCompany, ApplicationType.Assets, AstRestituzioni.FonteBene, a.Id);

                var nominativo = AstUtility.Nominativo(dip.Cognome, dip.Nome);
                if (!string.IsNullOrEmpty(dip.IdAspNetUsers))
                    await _acknowledgementService.RequestAsync(new AcknowledgementRequest
                    {
                        IdCompany = ctx.IdCompany,
                        Application = ApplicationType.Assets,
                        Fonte = FonteRestituzione,
                        IdRiferimento = a.Id,
                        UserIds = new[] { dip.IdAspNetUsers },
                        Titolo = AssetsMessages.Get("ReturnReportTitle", b.CodiceInventario, b.Descrizione),
                        Testo = TestoVerbale(b, await CategoriaAsync(b.IdAst_Categoria), nominativo, AssetsMessages.Get("ReturnReportText", a.DataRestituzione.Value.ToString("dd/MM/yyyy")), a.CondizioniRestituzione),
                        Url = "/astmine",
                    });
                var res = await AssegnazioniModelAsync(ctx, new List<Ast_BeneAssegnazione> { a }, new Dictionary<int, Ast_Bene> { { b.Id, b } },
                                                       new Dictionary<int, string> { { dip.Id, nominativo } });
                return new Ast_BeneAssegnazionePutOutModel { Assegnazione = res[0] };
            }, isSubProcess);
        }

        // annulla una consegna registrata per errore: ritira i verbali non confermati e rimette il bene disponibile se era aperta
        public virtual async Task<GenericResult<Ast_DeleteOutModel>> Ast_BeneAssegnazioneDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var a = await _Ast_BeneAssegnazione.FindAll(x => x.Id == model.Data.Id && x.Ast_BeneNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                await _Ast_BeneAssegnazione.FindAll(x => x.Id == a.Id).ExecuteDeleteAsync();
                if (a.DataRestituzione == null)
                    await _Ast_Bene.FindAll(x => x.Id == a.IdAst_Bene && x.Stato == AstStatoBene.Assegnato).ExecuteUpdateAsync(u => u.SetProperty(x => x.Stato, AstStatoBene.Disponibile));
                await _acknowledgementService.CancelAsync(ctx.IdCompany, ApplicationType.Assets, FonteConsegna, a.Id);
                await _acknowledgementService.CancelAsync(ctx.IdCompany, ApplicationType.Assets, FonteRestituzione, a.Id);
                await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.Assets, AstRestituzioni.FonteBene, a.Id);
                return new Ast_DeleteOutModel { Eliminato = true };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ restituzioni alla cessazione (AST-BENI.3)

        public virtual async Task<GenericResult<Ast_DaRestituireListOutModel>> Ast_DaRestituireList(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                return new Ast_DaRestituireListOutModel { DaRestituire = await _restituzioni.ElencoAsync(ctx.IdCompany) };
            }, isSubProcess);
        }

        // la verifica gira ogni notte; dalla pagina si puo' lanciare subito (es. dopo aver registrato una cessazione)
        public virtual async Task<GenericResult<Ast_DaRestituireVerificaOutModel>> Ast_DaRestituireVerifica(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                int avvisate = await _restituzioni.VerificaAsync(ctx.IdCompany);
                return new Ast_DaRestituireVerificaOutModel { Avvisate = avvisate, DaRestituire = await _restituzioni.ElencoAsync(ctx.IdCompany) };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ utente

        public virtual async Task<GenericResult<Ast_MieiBeniOutModel>> Ast_MieiBeniGet(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var res = new Ast_MieiBeniOutModel();
                var io = await _utility.MioDipendenteAsync(ctx);
                if (io == null)
                    return res;
                // in mano ora, e le restituzioni dell'ultimo anno (per i verbali)
                var dal = DateTime.Today.AddYears(-1);
                var assegnazioni = await _Ast_BeneAssegnazione.FindAll(x => x.IdDip_Anagrafica == io && (x.DataRestituzione == null || x.DataRestituzione >= dal))
                                                              .Include(x => x.Ast_BeneNavigation)
                                                              .OrderBy(x => x.DataRestituzione != null).ThenByDescending(x => x.DataConsegna).ToListAsync();
                var beni = assegnazioni.Select(x => x.Ast_BeneNavigation!).DistinctBy(x => x.Id).ToDictionary(x => x.Id);
                var nomi = (await _Dip_Anagrafica.FindAll(x => x.Id == io).Select(x => new { x.Id, x.Cognome, x.Nome }).ToListAsync())
                           .ToDictionary(x => x.Id, x => AstUtility.Nominativo(x.Cognome, x.Nome));
                res.Assegnazione = await AssegnazioniModelAsync(ctx, assegnazioni, beni, nomi);
                return res;
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ supporto

        private async Task<Ast_Bene> BeneAsync(AstContesto ctx, int id)
            => await _Ast_Bene.FindAll(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
               ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));

        private async Task<string?> CategoriaAsync(int id) => await _Ast_Categoria.FindAll(x => x.Id == id).Select(x => x.Descrizione).FirstOrDefaultAsync();

        private async Task<List<Ast_CategoriaModel>> CategorieAsync(AstContesto ctx)
        {
            await _utility.EnsureCategorieAsync(ctx.IdAz_Anagrafica);
            var numeri = await _Ast_Bene.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                        .GroupBy(x => x.IdAst_Categoria).Select(g => new { g.Key, N = g.Count() })
                                        .ToDictionaryAsync(x => x.Key, x => x.N);
            var categorie = await _Ast_Categoria.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                                .OrderBy(x => x.Ordine).ThenBy(x => x.Descrizione)
                                                .Select(x => new Ast_CategoriaModel { Id = x.Id, Descrizione = x.Descrizione, Attivo = x.Attivo, Ordine = x.Ordine })
                                                .ToListAsync();
            foreach (var c in categorie)
                c.NumeroBeni = numeri.TryGetValue(c.Id, out var n) ? n : 0;
            return categorie;
        }

        // testo del verbale (lo legge il dipendente; con il documentale finisce nel PDF da firmare)
        private static string TestoVerbale(Ast_Bene b, string? categoria, string dipendente, string apertura, string? condizioni)
        {
            var righe = new List<string>
            {
                apertura,
                AssetsMessages.Get("ReportLineEmployee", dipendente),
                AssetsMessages.Get("ReportLineGood", b.CodiceInventario, b.Descrizione, categoria ?? "-"),
            };
            if (!string.IsNullOrEmpty(b.MarcaModello))
                righe.Add(AssetsMessages.Get("ReportLineModel", b.MarcaModello));
            if (!string.IsNullOrEmpty(b.NumeroSerie))
                righe.Add(AssetsMessages.Get("ReportLineSerial", b.NumeroSerie));
            if (!string.IsNullOrEmpty(condizioni))
                righe.Add(AssetsMessages.Get("ReportLineConditions", condizioni));
            return string.Join("\n", righe);
        }

        private async Task RegistraGaranziaAsync(AstContesto ctx, Ast_Bene b)
        {
            if (b.ScadenzaGaranzia == null)
            {
                await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.Assets, FonteGaranzia, b.Id);
                return;
            }
            var cfg = await _utility.CfgAsync(ctx);
            await _deadlineRegistry.UpsertAsync(new DeadlineRegistration
            {
                IdCompany = ctx.IdCompany,
                Application = ApplicationType.Assets,
                Fonte = FonteGaranzia,
                IdRiferimento = b.Id,
                Descrizione = AssetsMessages.Get("WarrantyDeadlineDescription", b.CodiceInventario, b.Descrizione),
                DataScadenza = b.ScadenzaGaranzia.Value,
                GiorniPreavviso = cfg.GiorniPreavviso,
                Url = UrlBene,
                UrlState = new { id = b.Id },
            });
            if (b.Stato == AstStatoBene.Dismesso)
                await _deadlineRegistry.CloseAsync(ctx.IdCompany, ApplicationType.Assets, FonteGaranzia, b.Id);
        }

        // assegnazioni con lo stato dei verbali (prese visione dei destinatari)
        private async Task<List<Ast_BeneAssegnazioneModel>> AssegnazioniModelAsync(AstContesto ctx, List<Ast_BeneAssegnazione> assegnazioni,
                                                                                   Dictionary<int, Ast_Bene> beni, Dictionary<int, string> nomi)
        {
            var ids = assegnazioni.Select(x => (int?)x.Id).ToList();
            var verbali = await _presaVisioneRepository.FindAll(x => x.IdCompany == ctx.IdCompany && x.Application == (int)ApplicationType.Assets
                                                                   && (x.Fonte == FonteConsegna || x.Fonte == FonteRestituzione)
                                                                   && ids.Contains(x.IdRiferimento) && !x.Annullata)
                                                       .Select(x => new { x.Fonte, x.IdRiferimento, x.DataConferma, x.Canale })
                                                       .ToListAsync();
            Ast_VerbaleModel Verbale(string fonte, int id)
            {
                var v = verbali.FirstOrDefault(x => x.Fonte == fonte && x.IdRiferimento == id);
                return v == null ? new Ast_VerbaleModel() : new Ast_VerbaleModel { Richiesto = true, DataConferma = v.DataConferma, Canale = v.Canale };
            }
            return assegnazioni.Select(a => new Ast_BeneAssegnazioneModel
            {
                Id = a.Id,
                IdAst_Bene = a.IdAst_Bene,
                Bene = beni.TryGetValue(a.IdAst_Bene, out var b) ? b.Descrizione : null,
                CodiceInventario = b?.CodiceInventario,
                IdDip_Anagrafica = a.IdDip_Anagrafica,
                Dipendente = nomi.TryGetValue(a.IdDip_Anagrafica, out var n) ? n : null,
                DataConsegna = a.DataConsegna,
                CondizioniConsegna = a.CondizioniConsegna,
                DataRestituzione = a.DataRestituzione,
                CondizioniRestituzione = a.CondizioniRestituzione,
                Note = a.Note,
                VerbaleConsegna = Verbale(FonteConsegna, a.Id),
                VerbaleRestituzione = Verbale(FonteRestituzione, a.Id),
            }).ToList();
        }

        private static Ast_BeneModel BeneModel(Ast_Bene b) => new()
        {
            Id = b.Id, CodiceInventario = b.CodiceInventario, IdAst_Categoria = b.IdAst_Categoria, Descrizione = b.Descrizione, MarcaModello = b.MarcaModello,
            NumeroSerie = b.NumeroSerie, DataAcquisto = b.DataAcquisto, Valore = b.Valore, ScadenzaGaranzia = b.ScadenzaGaranzia, IdAz_Sedi = b.IdAz_Sedi,
            Stato = (int)b.Stato, Note = b.Note,
        };
    }

    public interface IAst_BeniService : IServiceBase
    {
        Task<GenericResult<Ast_CategoriaListOutModel>> Ast_CategoriaList(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_CategoriaPutOutModel>> Ast_CategoriaPut(GenericRequest<Ast_CategoriaPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_DeleteOutModel>> Ast_CategoriaDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_BeneListOutModel>> Ast_BeneList(GenericRequest<Ast_BeneListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_BeneGetOutModel>> Ast_BeneGet(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_BenePutOutModel>> Ast_BenePut(GenericRequest<Ast_BenePutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_DeleteOutModel>> Ast_BeneDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_BeneAssegnazionePutOutModel>> Ast_ConsegnaPut(GenericRequest<Ast_ConsegnaPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_BeneAssegnazionePutOutModel>> Ast_RestituzionePut(GenericRequest<Ast_RestituzionePutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_DeleteOutModel>> Ast_BeneAssegnazioneDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_DaRestituireListOutModel>> Ast_DaRestituireList(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_DaRestituireVerificaOutModel>> Ast_DaRestituireVerifica(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_MieiBeniOutModel>> Ast_MieiBeniGet(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess);
    }
}
