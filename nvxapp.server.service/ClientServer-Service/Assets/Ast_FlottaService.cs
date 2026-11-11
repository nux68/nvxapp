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
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Assets;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.service.ClientServer_Service.Assets._utility;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.Assets
{
    /*
     Flotta aziendale (ANALISI_ASSETS_APPLICATIVO.md, AST-FLO.1-3): veicoli, assegnazioni e fringe benefit, scadenze, costi e multe.
        - amministratori dell'azienda (CompanyPowerAdmin, CompanyAdmin): tutto
        - utente: il proprio veicolo (con il fringe benefit) e le proprie multe gia' notificate (Ast_MieiVeicoliGet)
     Scadenzario comune: le scadenze del veicolo (fonte "ast.veicolo.scadenza") e del contratto di noleggio/leasing
     ("ast.veicolo.contratto"), senza responsabile (avvisa gli amministratori), preavviso da Ast_Cfg.GiorniPreavviso.
     Assegnazione e Common (AST-D3, scelta provvisoria): a ogni assegnazione Assets crea o aggiorna l'autovettura del dipendente
     (Dip_RapportoAutovetture, repository di Common) usata dalle note spese delle Presenze; a fine assegnazione o cancellazione
     l'autovettura resta (puo' essere usata da note spese gia' registrate).
     Fringe benefit (AST-D2, scelta provvisoria): Assets tiene costo ACI, percentuale e periodi e calcola il valore a titolo
     informativo (15.000 km x costo ACI x percentuale); l'export paghe resta nelle Presenze.
    */
    public class Ast_FlottaService : ServiceBase, IAst_FlottaService
    {
        public const string FonteScadenza = "ast.veicolo.scadenza";
        public const string FonteContratto = "ast.veicolo.contratto";
        public const string UrlVeicolo = "/astvehicleedit";
        private const decimal KmConvenzionali = 15000m;

        private readonly IAst_VeicoloRepository _Ast_Veicolo;
        private readonly IAst_VeicoloAssegnazioneRepository _Ast_VeicoloAssegnazione;
        private readonly IAst_VeicoloScadenzaRepository _Ast_VeicoloScadenza;
        private readonly IAst_VeicoloCostoRepository _Ast_VeicoloCosto;
        private readonly IAst_MultaRepository _Ast_Multa;
        private readonly IAst_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly IAst_Az_SediReadRepository _Az_Sedi;
        private readonly IDip_RapportoAutovettureCommonRepository _Dip_RapportoAutovetture;
        private readonly IAst_Dip_RapportoAutovettureReadRepository _Dip_RapportoAutovettureRead;
        private readonly IDeadlineRegistry _deadlineRegistry;
        private readonly IAstUtility _utility;

        public Ast_FlottaService(IMapper mapper,
                                 UserManager<ApplicationUser> userManager,
                                 IAspNetUsersRepository aspNetUsersRepository,
                                 IOptions<JwtParameter> jwtParameter,
                                 IHttpContextAccessor httpContextAccessor,
                                 IConfiguration configuration,
                                 IAst_VeicoloRepository ast_VeicoloRepository,
                                 IAst_VeicoloAssegnazioneRepository ast_VeicoloAssegnazioneRepository,
                                 IAst_VeicoloScadenzaRepository ast_VeicoloScadenzaRepository,
                                 IAst_VeicoloCostoRepository ast_VeicoloCostoRepository,
                                 IAst_MultaRepository ast_MultaRepository,
                                 IAst_Dip_AnagraficaReadRepository dip_AnagraficaRepository,
                                 IAst_Az_SediReadRepository az_SediRepository,
                                 IDip_RapportoAutovettureCommonRepository dip_RapportoAutovettureRepository,
                                 IAst_Dip_RapportoAutovettureReadRepository dip_RapportoAutovettureReadRepository,
                                 IDeadlineRegistry deadlineRegistry,
                                 IAstUtility utility) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Ast_Veicolo = ast_VeicoloRepository;
            _Ast_VeicoloAssegnazione = ast_VeicoloAssegnazioneRepository;
            _Ast_VeicoloScadenza = ast_VeicoloScadenzaRepository;
            _Ast_VeicoloCosto = ast_VeicoloCostoRepository;
            _Ast_Multa = ast_MultaRepository;
            _Dip_Anagrafica = dip_AnagraficaRepository;
            _Az_Sedi = az_SediRepository;
            _Dip_RapportoAutovetture = dip_RapportoAutovettureRepository;
            _Dip_RapportoAutovettureRead = dip_RapportoAutovettureReadRepository;
            _deadlineRegistry = deadlineRegistry;
            _utility = utility;
        }

        // ------------------------------------------------------------------------------------------------ veicoli

        public virtual async Task<GenericResult<Ast_VeicoloListOutModel>> Ast_VeicoloList(GenericRequest<Ast_VeicoloListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                bool dismessi = model.Data?.Dismessi ?? false;
                var oggi = DateTime.Today;
                var righe = await _Ast_Veicolo.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && (dismessi || x.Stato == AstStatoVeicolo.Attivo))
                                              .Select(x => new
                                              {
                                                  x.Id, x.Targa, x.Marca, x.Modello, x.Uso, x.Stato,
                                                  Sede = x.Az_SediNavigation != null ? x.Az_SediNavigation.Descrizione : null,
                                                  Assegnatario = x.Ast_VeicoloAssegnazione!.Where(a => a.DataInizio <= oggi && (a.DataFine == null || a.DataFine >= oggi))
                                                                                          .Select(a => new { a.Dip_AnagraficaNavigation!.Cognome, a.Dip_AnagraficaNavigation.Nome })
                                                                                          .FirstOrDefault(),
                                                  Scadenza = x.Ast_VeicoloScadenza!.Where(s => s.DataEsecuzione == null).OrderBy(s => s.DataScadenza)
                                                                                    .Select(s => new { s.DataScadenza, s.Tipo, s.Descrizione }).FirstOrDefault(),
                                              })
                                              .ToListAsync();
                return new Ast_VeicoloListOutModel
                {
                    Permessi = new Ast_PermessiModel { PowerAdmin = ctx.PowerAdmin, Admin = ctx.Admin },
                    Veicolo = righe.Select(x => new Ast_VeicoloListModel
                    {
                        Id = x.Id,
                        Targa = x.Targa,
                        Descrizione = DescrizioneVeicolo(x.Marca, x.Modello, x.Targa),
                        Uso = (int)x.Uso,
                        Stato = (int)x.Stato,
                        Sede = x.Sede,
                        Assegnatario = x.Assegnatario == null ? null : AstUtility.Nominativo(x.Assegnatario.Cognome, x.Assegnatario.Nome),
                        ProssimaScadenza = x.Scadenza?.DataScadenza,
                        ProssimaScadenzaDescrizione = x.Scadenza == null ? null : DescrizioneScadenza(x.Scadenza.Tipo, x.Scadenza.Descrizione),
                    }).OrderBy(x => x.Targa).ToList(),
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_VeicoloGetOutModel>> Ast_VeicoloGet(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var res = new Ast_VeicoloGetOutModel
                {
                    Sede = await _utility.SediAsync(ctx),
                    Dipendente = await _utility.DipendentiAsync(ctx),
                };
                int id = model.Data?.Id ?? 0;
                if (id == 0)
                    return res;

                var v = await VeicoloAsync(ctx, id);
                res.Veicolo = VeicoloModel(v);
                var nomi = res.Dipendente.ToDictionary(x => x.Id, x => x.Nominativo);
                res.Assegnazione = (await _Ast_VeicoloAssegnazione.FindAll(x => x.IdAst_Veicolo == id).OrderByDescending(x => x.DataInizio).ToListAsync())
                                   .Select(x => AssegnazioneModel(x, v, nomi)).ToList();
                // rimborso km dell'autovettura del dipendente (Common)
                var auto = res.Assegnazione.Where(x => x.IdDip_RapportoAutovetture != null).Select(x => x.IdDip_RapportoAutovetture!.Value).ToList();
                var rimborsi = await _Dip_RapportoAutovettureRead.FindAll(x => auto.Contains(x.Id)).ToDictionaryAsync(x => x.Id, x => x.RimborsoKM);
                foreach (var a in res.Assegnazione)
                    a.RimborsoKm = a.IdDip_RapportoAutovetture != null && rimborsi.TryGetValue(a.IdDip_RapportoAutovetture.Value, out var r) ? r : 0;
                res.Scadenza = (await _Ast_VeicoloScadenza.FindAll(x => x.IdAst_Veicolo == id).OrderBy(x => x.DataEsecuzione != null).ThenBy(x => x.DataScadenza).ToListAsync())
                               .Select(ScadenzaModel).ToList();
                res.Costo = (await _Ast_VeicoloCosto.FindAll(x => x.IdAst_Veicolo == id).OrderByDescending(x => x.Data).Take(500).ToListAsync())
                            .Select(CostoModel).ToList();
                var inizioAnno = new DateTime(DateTime.Today.Year, 1, 1);
                res.TotaleCostiAnno = await _Ast_VeicoloCosto.FindAll(x => x.IdAst_Veicolo == id && x.Data >= inizioAnno).SumAsync(x => x.Importo);
                res.Multa = (await _Ast_Multa.FindAll(x => x.IdAst_Veicolo == id).OrderByDescending(x => x.DataInfrazione).ToListAsync())
                            .Select(x => MultaModel(x, v.Targa, nomi)).ToList();
                return res;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_VeicoloPutOutModel>> Ast_VeicoloPut(GenericRequest<Ast_VeicoloPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var d = model.Data.Veicolo;
                var targa = NormalizzaTarga(d.Targa);
                if (string.IsNullOrEmpty(targa))
                    throw new UserMessageException(AssetsMessages.Get("VehiclePlateRequired"));
                if (await _Ast_Veicolo.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Targa == targa && x.Id != d.Id).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("VehiclePlateDuplicate", targa));
                if (d.IdAz_Sedi != null && !await _Az_Sedi.FindAll(x => x.Id == d.IdAz_Sedi && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("NotFound"));

                Ast_Veicolo v;
                if (d.Id == 0)
                    v = new Ast_Veicolo { IdAz_Anagrafica = ctx.IdAz_Anagrafica };
                else
                    v = await _Ast_Veicolo.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                v.Targa = targa;
                v.Marca = AstUtility.Testo(d.Marca, 50);
                v.Modello = AstUtility.Testo(d.Modello, 50);
                v.Alimentazione = Enum.IsDefined(typeof(AstAlimentazione), d.Alimentazione) ? (AstAlimentazione)d.Alimentazione : AstAlimentazione.Altro;
                v.DataImmatricolazione = d.DataImmatricolazione?.Date;
                v.Possesso = Enum.IsDefined(typeof(AstPossesso), d.Possesso) ? (AstPossesso)d.Possesso : AstPossesso.Proprieta;
                v.Fornitore = AstUtility.Testo(d.Fornitore, 100);
                v.NumeroContratto = AstUtility.Testo(d.NumeroContratto, 50);
                v.ScadenzaContratto = v.Possesso == AstPossesso.Proprieta ? null : d.ScadenzaContratto?.Date;
                v.KmInclusi = d.KmInclusi is > 0 ? d.KmInclusi : null;
                v.CanoneMensile = d.CanoneMensile is > 0 ? Math.Round(d.CanoneMensile.Value, 2) : null;
                v.IdAz_Sedi = d.IdAz_Sedi;
                v.Uso = d.Uso == (int)AstUsoVeicolo.Pool ? AstUsoVeicolo.Pool : AstUsoVeicolo.Assegnato;
                v.CostoKmAci = Math.Clamp(Math.Round(d.CostoKmAci, 4), 0, 99);
                v.EmissioniCO2 = d.EmissioniCO2 is > 0 ? d.EmissioniCO2 : null;
                v.KmAttuali = d.KmAttuali is >= 0 ? d.KmAttuali : null;
                v.Stato = d.Stato == (int)AstStatoVeicolo.Dismesso ? AstStatoVeicolo.Dismesso : AstStatoVeicolo.Attivo;
                v.Note = AstUtility.Testo(d.Note, 2000);
                if (d.Id == 0)
                    await _Ast_Veicolo.CreateAsync(v);
                else
                {
                    v.ModifiedDate = DateTime.Now;
                    v.ChangeUser = ctx.IdUtente;
                    await _Ast_Veicolo.SaveChange();
                }

                await RegistraContrattoAsync(ctx, v);
                // le scadenze portano la targa nella descrizione: si aggiornano (e si chiudono se il veicolo e' dismesso)
                foreach (var s in await _Ast_VeicoloScadenza.FindAll(x => x.IdAst_Veicolo == v.Id).ToListAsync())
                    await RegistraScadenzaAsync(ctx, v, s);
                return new Ast_VeicoloPutOutModel { Veicolo = VeicoloModel(v) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_DeleteOutModel>> Ast_VeicoloDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var v = await VeicoloAsync(ctx, model.Data.Id);
                var scadenze = await _Ast_VeicoloScadenza.FindAll(x => x.IdAst_Veicolo == v.Id).Select(x => x.Id).ToListAsync();
                await _Ast_Veicolo.FindAll(x => x.Id == v.Id).ExecuteDeleteAsync();
                foreach (var id in scadenze)
                    await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.Assets, FonteScadenza, id);
                await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.Assets, FonteContratto, v.Id);
                return new Ast_DeleteOutModel { Eliminato = true };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ assegnazioni (AST-FLO.1)

        public virtual async Task<GenericResult<Ast_AssegnazionePutOutModel>> Ast_AssegnazionePut(GenericRequest<Ast_AssegnazionePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var d = model.Data.Assegnazione;
                Ast_VeicoloAssegnazione a;
                if (d.Id == 0)
                    a = new Ast_VeicoloAssegnazione { IdAst_Veicolo = d.IdAst_Veicolo };
                else
                    a = await _Ast_VeicoloAssegnazione.FindAll(x => x.Id == d.Id).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                var v = await VeicoloAsync(ctx, a.IdAst_Veicolo);
                if (v.Uso == AstUsoVeicolo.Pool)
                    throw new UserMessageException(AssetsMessages.Get("VehiclePoolNotAssignable"));
                var dip = await _Dip_Anagrafica.FindAll(x => x.Id == d.IdDip_Anagrafica && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                          ?? throw new UserMessageException(AssetsMessages.Get("EmployeeRequired"));
                var inizio = d.DataInizio.Date;
                var fine = d.DataFine?.Date;
                if (inizio == DateTime.MinValue || (fine != null && fine < inizio))
                    throw new UserMessageException(AssetsMessages.Get("InvalidPeriod"));
                // un veicolo e' assegnato a un dipendente per volta
                var fineMax = fine ?? DateTime.MaxValue.Date;
                if (await _Ast_VeicoloAssegnazione.FindAll(x => x.IdAst_Veicolo == v.Id && x.Id != a.Id && x.DataInizio <= fineMax
                                                                && (x.DataFine == null || x.DataFine >= inizio)).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("VehicleAlreadyAssigned"));

                bool cambioDipendente = a.Id != 0 && a.IdDip_Anagrafica != dip.Id;
                a.IdDip_Anagrafica = dip.Id;
                a.DataInizio = inizio;
                a.DataFine = fine;
                a.UsoPromiscuo = d.UsoPromiscuo;
                a.PercentualeFringe = !d.UsoPromiscuo ? 0 : d.PercentualeFringe > 0 ? Math.Clamp(Math.Round(d.PercentualeFringe, 2), 0, 100) : PercentualeDaEmissioni(v.EmissioniCO2);
                a.Note = AstUtility.Testo(d.Note, 500);
                if (cambioDipendente)
                {
                    a.IdDip_RapportoAutovetture = null;
                    a.DataAvvisoCessazione = null;
                }

                // autovettura del dipendente in Common (note spese delle Presenze)
                var descr = DescrizioneVeicolo(v.Marca, v.Modello, v.Targa);
                descr = descr.Length > 50 ? descr[..50] : descr;
                var rimborso = Math.Clamp(Math.Round(d.RimborsoKm, 4), 0, 999999m);
                var auto = a.IdDip_RapportoAutovetture == null ? null
                           : await _Dip_RapportoAutovetture.FindAll(x => x.Id == a.IdDip_RapportoAutovetture && x.IdDip_Anagrafica == dip.Id).AsTracking().FirstOrDefaultAsync();
                if (auto == null)
                {
                    auto = await _Dip_RapportoAutovetture.CreateAsync(new Dip_RapportoAutovetture
                    {
                        IdDip_Anagrafica = dip.Id, Descrizione = descr, Targa = v.Targa, RimborsoKM = rimborso
                    });
                }
                else
                {
                    auto.Descrizione = descr;
                    auto.Targa = v.Targa;
                    auto.RimborsoKM = rimborso;
                    auto.ModifiedDate = DateTime.Now;
                    auto.ChangeUser = ctx.IdUtente;
                    await _Dip_RapportoAutovetture.SaveChange();
                }
                a.IdDip_RapportoAutovetture = auto.Id;

                if (a.Id == 0)
                    await _Ast_VeicoloAssegnazione.CreateAsync(a);
                else
                {
                    a.ModifiedDate = DateTime.Now;
                    a.ChangeUser = ctx.IdUtente;
                    await _Ast_VeicoloAssegnazione.SaveChange();
                }
                var res = AssegnazioneModel(a, v, new Dictionary<int, string> { { dip.Id, AstUtility.Nominativo(dip.Cognome, dip.Nome) } });
                res.RimborsoKm = auto.RimborsoKM;
                return new Ast_AssegnazionePutOutModel { Assegnazione = res };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_DeleteOutModel>> Ast_AssegnazioneDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var a = await _Ast_VeicoloAssegnazione.FindAll(x => x.Id == model.Data.Id && x.Ast_VeicoloNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                await _Ast_VeicoloAssegnazione.FindAll(x => x.Id == a.Id).ExecuteDeleteAsync();
                return new Ast_DeleteOutModel { Eliminato = true };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ scadenze (AST-FLO.2)

        public virtual async Task<GenericResult<Ast_VeicoloScadenzaPutOutModel>> Ast_VeicoloScadenzaPut(GenericRequest<Ast_VeicoloScadenzaPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var d = model.Data.Scadenza;
                Ast_VeicoloScadenza s;
                if (d.Id == 0)
                    s = new Ast_VeicoloScadenza { IdAst_Veicolo = d.IdAst_Veicolo };
                else
                    s = await _Ast_VeicoloScadenza.FindAll(x => x.Id == d.Id).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                var v = await VeicoloAsync(ctx, s.IdAst_Veicolo);
                if (d.DataScadenza == DateTime.MinValue)
                    throw new UserMessageException(AssetsMessages.Get("DateRequired"));
                s.Tipo = Enum.IsDefined(typeof(AstTipoScadenzaVeicolo), d.Tipo) ? (AstTipoScadenzaVeicolo)d.Tipo : AstTipoScadenzaVeicolo.Altro;
                s.Descrizione = AstUtility.Testo(d.Descrizione, 100);
                s.DataScadenza = d.DataScadenza.Date;
                s.Km = d.Km is > 0 ? d.Km : null;
                s.Importo = d.Importo is > 0 ? Math.Round(d.Importo.Value, 2) : null;
                s.DataEsecuzione = d.DataEsecuzione?.Date;
                s.Note = AstUtility.Testo(d.Note, 500);
                if (s.Id == 0)
                    await _Ast_VeicoloScadenza.CreateAsync(s);
                else
                {
                    s.ModifiedDate = DateTime.Now;
                    s.ChangeUser = ctx.IdUtente;
                    await _Ast_VeicoloScadenza.SaveChange();
                }
                await RegistraScadenzaAsync(ctx, v, s);
                return new Ast_VeicoloScadenzaPutOutModel { Scadenza = ScadenzaModel(s) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_DeleteOutModel>> Ast_VeicoloScadenzaDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var s = await _Ast_VeicoloScadenza.FindAll(x => x.Id == model.Data.Id && x.Ast_VeicoloNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                await _Ast_VeicoloScadenza.FindAll(x => x.Id == s.Id).ExecuteDeleteAsync();
                await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.Assets, FonteScadenza, s.Id);
                return new Ast_DeleteOutModel { Eliminato = true };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ costi e multe (AST-FLO.3)

        public virtual async Task<GenericResult<Ast_VeicoloCostoPutOutModel>> Ast_VeicoloCostoPut(GenericRequest<Ast_VeicoloCostoPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var d = model.Data.Costo;
                Ast_VeicoloCosto c;
                if (d.Id == 0)
                    c = new Ast_VeicoloCosto { IdAst_Veicolo = d.IdAst_Veicolo };
                else
                    c = await _Ast_VeicoloCosto.FindAll(x => x.Id == d.Id).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                var v = await VeicoloAsync(ctx, c.IdAst_Veicolo);
                if (d.Data == DateTime.MinValue)
                    throw new UserMessageException(AssetsMessages.Get("DateRequired"));
                if (d.Importo <= 0)
                    throw new UserMessageException(AssetsMessages.Get("AmountRequired"));
                c.Data = d.Data.Date;
                c.Tipo = Enum.IsDefined(typeof(AstTipoCostoVeicolo), d.Tipo) ? (AstTipoCostoVeicolo)d.Tipo : AstTipoCostoVeicolo.Altro;
                c.Importo = Math.Round(d.Importo, 2);
                c.Km = d.Km is > 0 ? d.Km : null;
                c.Descrizione = AstUtility.Testo(d.Descrizione, 200);
                c.Documento = AstUtility.Testo(d.Documento, 50);
                if (c.Id == 0)
                    await _Ast_VeicoloCosto.CreateAsync(c);
                else
                {
                    c.ModifiedDate = DateTime.Now;
                    c.ChangeUser = ctx.IdUtente;
                    await _Ast_VeicoloCosto.SaveChange();
                }
                // i km del costo aggiornano i km attuali del veicolo
                if (c.Km != null && (v.KmAttuali ?? 0) < c.Km)
                    await _Ast_Veicolo.FindAll(x => x.Id == v.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.KmAttuali, c.Km));
                return new Ast_VeicoloCostoPutOutModel { Costo = CostoModel(c) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_DeleteOutModel>> Ast_VeicoloCostoDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                int n = await _Ast_VeicoloCosto.FindAll(x => x.Id == model.Data.Id && x.Ast_VeicoloNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ExecuteDeleteAsync();
                if (n == 0)
                    throw new UserMessageException(AssetsMessages.Get("NotFound"));
                return new Ast_DeleteOutModel { Eliminato = true };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_MultaPutOutModel>> Ast_MultaPut(GenericRequest<Ast_MultaPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var d = model.Data.Multa;
                Ast_Multa m;
                if (d.Id == 0)
                    m = new Ast_Multa { IdAst_Veicolo = d.IdAst_Veicolo };
                else
                    m = await _Ast_Multa.FindAll(x => x.Id == d.Id).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                var v = await VeicoloAsync(ctx, m.IdAst_Veicolo);
                if (d.DataInfrazione == DateTime.MinValue)
                    throw new UserMessageException(AssetsMessages.Get("DateRequired"));
                m.DataInfrazione = d.DataInfrazione;
                m.NumeroVerbale = AstUtility.Testo(d.NumeroVerbale, 50);
                m.Luogo = AstUtility.Testo(d.Luogo, 200);
                m.Descrizione = AstUtility.Testo(d.Descrizione, 500);
                m.Importo = Math.Max(0, Math.Round(d.Importo, 2));
                // conducente: quello indicato, altrimenti chi aveva il veicolo il giorno dell'infrazione
                int? conducente = d.IdDip_Anagrafica;
                if (conducente != null && !await _Dip_Anagrafica.FindAll(x => x.Id == conducente && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    throw new UserMessageException(AssetsMessages.Get("NotFound"));
                if (conducente == null && d.Id == 0)
                {
                    var giorno = d.DataInfrazione.Date;
                    conducente = await _Ast_VeicoloAssegnazione.FindAll(x => x.IdAst_Veicolo == v.Id && x.DataInizio <= giorno && (x.DataFine == null || x.DataFine >= giorno))
                                                               .Select(x => (int?)x.IdDip_Anagrafica).FirstOrDefaultAsync();
                }
                m.IdDip_Anagrafica = conducente;
                m.Stato = Enum.IsDefined(typeof(AstStatoMulta), d.Stato) ? (AstStatoMulta)d.Stato : AstStatoMulta.DaNotificare;
                m.DataPagamento = m.Stato == AstStatoMulta.Pagata ? (d.DataPagamento?.Date ?? DateTime.Today) : d.DataPagamento?.Date;
                m.Note = AstUtility.Testo(d.Note, 500);
                if (m.Id == 0)
                    await _Ast_Multa.CreateAsync(m);
                else
                {
                    m.ModifiedDate = DateTime.Now;
                    m.ChangeUser = ctx.IdUtente;
                    await _Ast_Multa.SaveChange();
                }
                return new Ast_MultaPutOutModel { Multa = MultaModel(m, v.Targa, await NomiAsync(ctx, m.IdDip_Anagrafica)) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Ast_DeleteOutModel>> Ast_MultaDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                int n = await _Ast_Multa.FindAll(x => x.Id == model.Data.Id && x.Ast_VeicoloNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica).ExecuteDeleteAsync();
                if (n == 0)
                    throw new UserMessageException(AssetsMessages.Get("NotFound"));
                return new Ast_DeleteOutModel { Eliminato = true };
            }, isSubProcess);
        }

        // notifica la multa al conducente (deve avere un utente): passa a "notificata"
        public virtual async Task<GenericResult<Ast_MultaPutOutModel>> Ast_MultaNotifica(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                _utility.RichiediAmministratore(ctx);
                var m = await _Ast_Multa.FindAll(x => x.Id == model.Data.Id && x.Ast_VeicoloNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));
                var v = await VeicoloAsync(ctx, m.IdAst_Veicolo);
                var utente = m.IdDip_Anagrafica == null ? null
                             : await _Dip_Anagrafica.FindAll(x => x.Id == m.IdDip_Anagrafica).Select(x => x.IdAspNetUsers).FirstOrDefaultAsync();
                if (string.IsNullOrEmpty(utente))
                    throw new UserMessageException(AssetsMessages.Get("FineDriverWithoutUser"));
                await _utility.NotificaAsync(ctx, new[] { utente },
                                             AssetsMessages.Get("FineNotificationTitle", v.Targa),
                                             AssetsMessages.Get("FineNotificationText", m.DataInfrazione.ToString("dd/MM/yyyy HH:mm"), v.Targa, m.Importo.ToString("N2"), m.NumeroVerbale ?? "-"),
                                             "/astmine");
                if (m.Stato == AstStatoMulta.DaNotificare)
                    m.Stato = AstStatoMulta.Notificata;
                m.DataNotifica = DateTime.Now;
                m.ModifiedDate = DateTime.Now;
                m.ChangeUser = ctx.IdUtente;
                await _Ast_Multa.SaveChange();
                return new Ast_MultaPutOutModel { Multa = MultaModel(m, v.Targa, await NomiAsync(ctx, m.IdDip_Anagrafica)) };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ utente

        public virtual async Task<GenericResult<Ast_MieiVeicoliOutModel>> Ast_MieiVeicoliGet(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.ContestoAsync();
                var res = new Ast_MieiVeicoliOutModel();
                var io = await _utility.MioDipendenteAsync(ctx);
                if (io == null)
                    return res;
                var inizioAnno = new DateTime(DateTime.Today.Year, 1, 1);
                var assegnazioni = await _Ast_VeicoloAssegnazione.FindAll(x => x.IdDip_Anagrafica == io && (x.DataFine == null || x.DataFine >= inizioAnno))
                                                                 .Include(x => x.Ast_VeicoloNavigation)
                                                                 .OrderByDescending(x => x.DataInizio).ToListAsync();
                res.Veicolo = assegnazioni.Select(a =>
                {
                    var (annuo, anno) = Fringe(a, a.Ast_VeicoloNavigation!);
                    return new Ast_MioVeicoloModel
                    {
                        Id = a.Id,
                        Targa = a.Ast_VeicoloNavigation!.Targa,
                        Descrizione = DescrizioneVeicolo(a.Ast_VeicoloNavigation.Marca, a.Ast_VeicoloNavigation.Modello, a.Ast_VeicoloNavigation.Targa),
                        DataInizio = a.DataInizio,
                        DataFine = a.DataFine,
                        UsoPromiscuo = a.UsoPromiscuo,
                        FringeAnnuo = annuo,
                        FringeAnnoCorrente = anno,
                    };
                }).ToList();
                var nomi = await NomiAsync(ctx, io);
                res.Multa = (await _Ast_Multa.FindAll(x => x.IdDip_Anagrafica == io && x.Stato != AstStatoMulta.DaNotificare)
                                             .Include(x => x.Ast_VeicoloNavigation)
                                             .OrderByDescending(x => x.DataInfrazione).ToListAsync())
                            .Select(x => MultaModel(x, x.Ast_VeicoloNavigation!.Targa, nomi)).ToList();
                return res;
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ supporto

        private async Task<Ast_Veicolo> VeicoloAsync(AstContesto ctx, int id)
            => await _Ast_Veicolo.FindAll(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
               ?? throw new UserMessageException(AssetsMessages.Get("NotFound"));

        private async Task<Dictionary<int, string>> NomiAsync(AstContesto ctx, int? id)
        {
            if (id == null)
                return new Dictionary<int, string>();
            return (await _Dip_Anagrafica.FindAll(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).Select(x => new { x.Id, x.Cognome, x.Nome }).ToListAsync())
                   .ToDictionary(x => x.Id, x => AstUtility.Nominativo(x.Cognome, x.Nome));
        }

        // scadenza del veicolo nello scadenzario: aperta finche' non e' eseguita e il veicolo e' attivo
        private async Task RegistraScadenzaAsync(AstContesto ctx, Ast_Veicolo v, Ast_VeicoloScadenza s)
        {
            var cfg = await _utility.CfgAsync(ctx);
            await _deadlineRegistry.UpsertAsync(new DeadlineRegistration
            {
                IdCompany = ctx.IdCompany,
                Application = ApplicationType.Assets,
                Fonte = FonteScadenza,
                IdRiferimento = s.Id,
                Descrizione = AssetsMessages.Get("VehicleDeadlineDescription", DescrizioneScadenza(s.Tipo, s.Descrizione), v.Targa),
                Note = s.Km != null ? AssetsMessages.Get("VehicleDeadlineKm", s.Km) : s.Note,
                DataScadenza = s.DataScadenza,
                GiorniPreavviso = cfg.GiorniPreavviso,
                Url = UrlVeicolo,
                UrlState = new { id = v.Id },
            });
            if (s.DataEsecuzione != null || v.Stato == AstStatoVeicolo.Dismesso)
                await _deadlineRegistry.CloseAsync(ctx.IdCompany, ApplicationType.Assets, FonteScadenza, s.Id);
        }

        private async Task RegistraContrattoAsync(AstContesto ctx, Ast_Veicolo v)
        {
            if (v.ScadenzaContratto == null)
            {
                await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.Assets, FonteContratto, v.Id);
                return;
            }
            var cfg = await _utility.CfgAsync(ctx);
            await _deadlineRegistry.UpsertAsync(new DeadlineRegistration
            {
                IdCompany = ctx.IdCompany,
                Application = ApplicationType.Assets,
                Fonte = FonteContratto,
                IdRiferimento = v.Id,
                Descrizione = AssetsMessages.Get("VehicleContractDescription", AssetsMessages.Get("VehicleOwnership_" + (int)v.Possesso), v.Targa),
                Note = v.Fornitore,
                DataScadenza = v.ScadenzaContratto.Value,
                GiorniPreavviso = cfg.GiorniPreavviso,
                Url = UrlVeicolo,
                UrlState = new { id = v.Id },
            });
            if (v.Stato == AstStatoVeicolo.Dismesso)
                await _deadlineRegistry.CloseAsync(ctx.IdCompany, ApplicationType.Assets, FonteContratto, v.Id);
        }

        // targa senza spazi e trattini, in maiuscolo
        public static string NormalizzaTarga(string? targa)
        {
            var t = new string((targa ?? string.Empty).Where(char.IsLetterOrDigit).ToArray()).ToUpperInvariant();
            return t.Length > 15 ? t[..15] : t;
        }

        public static string DescrizioneVeicolo(string? marca, string? modello, string targa)
        {
            var mm = ((marca ?? string.Empty) + " " + (modello ?? string.Empty)).Trim();
            return mm.Length == 0 ? targa : mm + " (" + targa + ")";
        }

        private static string DescrizioneScadenza(AstTipoScadenzaVeicolo tipo, string? descrizione)
            => tipo == AstTipoScadenzaVeicolo.Altro && !string.IsNullOrWhiteSpace(descrizione) ? descrizione : AssetsMessages.Get("VehicleDeadlineType_" + (int)tipo);

        // percentuale del fringe benefit dalle emissioni (veicoli dal 2020): fino a 60 g/km 25%, fino a 160 30%, fino a 190 50%, oltre 60%
        public static decimal PercentualeDaEmissioni(int? co2) => co2 switch
        {
            null => 30m,
            <= 60 => 25m,
            <= 160 => 30m,
            <= 190 => 50m,
            _ => 60m,
        };

        // fringe benefit annuo e quota dell'anno in corso (giorni dell'assegnazione nell'anno)
        public static (decimal annuo, decimal annoCorrente) Fringe(Ast_VeicoloAssegnazione a, Ast_Veicolo v)
        {
            if (!a.UsoPromiscuo)
                return (0, 0);
            decimal annuo = Math.Round(KmConvenzionali * v.CostoKmAci * a.PercentualeFringe / 100m, 2);
            var inizioAnno = new DateTime(DateTime.Today.Year, 1, 1);
            var fineAnno = new DateTime(DateTime.Today.Year, 12, 31);
            var dal = a.DataInizio > inizioAnno ? a.DataInizio : inizioAnno;
            var al = a.DataFine == null || a.DataFine > fineAnno ? fineAnno : a.DataFine.Value;
            int giorni = al < dal ? 0 : (al - dal).Days + 1;
            int giorniAnno = DateTime.IsLeapYear(DateTime.Today.Year) ? 366 : 365;
            return (annuo, Math.Round(annuo * giorni / giorniAnno, 2));
        }

        private static Ast_VeicoloModel VeicoloModel(Ast_Veicolo v) => new()
        {
            Id = v.Id, Targa = v.Targa, Marca = v.Marca, Modello = v.Modello, Alimentazione = (int)v.Alimentazione,
            DataImmatricolazione = v.DataImmatricolazione, Possesso = (int)v.Possesso, Fornitore = v.Fornitore, NumeroContratto = v.NumeroContratto,
            ScadenzaContratto = v.ScadenzaContratto, KmInclusi = v.KmInclusi, CanoneMensile = v.CanoneMensile, IdAz_Sedi = v.IdAz_Sedi,
            Uso = (int)v.Uso, CostoKmAci = v.CostoKmAci, EmissioniCO2 = v.EmissioniCO2, KmAttuali = v.KmAttuali, Stato = (int)v.Stato, Note = v.Note,
        };

        private static Ast_VeicoloAssegnazioneModel AssegnazioneModel(Ast_VeicoloAssegnazione a, Ast_Veicolo v, Dictionary<int, string> nomi)
        {
            var (annuo, anno) = Fringe(a, v);
            return new Ast_VeicoloAssegnazioneModel
            {
                Id = a.Id, IdAst_Veicolo = a.IdAst_Veicolo, IdDip_Anagrafica = a.IdDip_Anagrafica,
                Dipendente = nomi.TryGetValue(a.IdDip_Anagrafica, out var n) ? n : null,
                DataInizio = a.DataInizio, DataFine = a.DataFine, UsoPromiscuo = a.UsoPromiscuo, PercentualeFringe = a.PercentualeFringe,
                IdDip_RapportoAutovetture = a.IdDip_RapportoAutovetture, Note = a.Note, FringeAnnuo = annuo, FringeAnnoCorrente = anno,
            };
        }

        private static Ast_VeicoloScadenzaModel ScadenzaModel(Ast_VeicoloScadenza s) => new()
        {
            Id = s.Id, IdAst_Veicolo = s.IdAst_Veicolo, Tipo = (int)s.Tipo, Descrizione = s.Descrizione, DataScadenza = s.DataScadenza,
            Km = s.Km, Importo = s.Importo, DataEsecuzione = s.DataEsecuzione, Note = s.Note,
        };

        private static Ast_VeicoloCostoModel CostoModel(Ast_VeicoloCosto c) => new()
        {
            Id = c.Id, IdAst_Veicolo = c.IdAst_Veicolo, Data = c.Data, Tipo = (int)c.Tipo, Importo = c.Importo, Km = c.Km,
            Descrizione = c.Descrizione, Documento = c.Documento,
        };

        private static Ast_MultaModel MultaModel(Ast_Multa m, string targa, Dictionary<int, string> nomi) => new()
        {
            Id = m.Id, IdAst_Veicolo = m.IdAst_Veicolo, Targa = targa, DataInfrazione = m.DataInfrazione, NumeroVerbale = m.NumeroVerbale,
            Luogo = m.Luogo, Descrizione = m.Descrizione, Importo = m.Importo, IdDip_Anagrafica = m.IdDip_Anagrafica,
            Conducente = m.IdDip_Anagrafica != null && nomi.TryGetValue(m.IdDip_Anagrafica.Value, out var n) ? n : null,
            Stato = (int)m.Stato, DataNotifica = m.DataNotifica, DataPagamento = m.DataPagamento, Note = m.Note,
        };
    }

    public interface IAst_FlottaService : IServiceBase
    {
        Task<GenericResult<Ast_VeicoloListOutModel>> Ast_VeicoloList(GenericRequest<Ast_VeicoloListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_VeicoloGetOutModel>> Ast_VeicoloGet(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_VeicoloPutOutModel>> Ast_VeicoloPut(GenericRequest<Ast_VeicoloPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_DeleteOutModel>> Ast_VeicoloDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_AssegnazionePutOutModel>> Ast_AssegnazionePut(GenericRequest<Ast_AssegnazionePutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_DeleteOutModel>> Ast_AssegnazioneDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_VeicoloScadenzaPutOutModel>> Ast_VeicoloScadenzaPut(GenericRequest<Ast_VeicoloScadenzaPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_DeleteOutModel>> Ast_VeicoloScadenzaDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_VeicoloCostoPutOutModel>> Ast_VeicoloCostoPut(GenericRequest<Ast_VeicoloCostoPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_DeleteOutModel>> Ast_VeicoloCostoDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_MultaPutOutModel>> Ast_MultaPut(GenericRequest<Ast_MultaPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_DeleteOutModel>> Ast_MultaDelete(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_MultaPutOutModel>> Ast_MultaNotifica(GenericRequest<Ast_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Ast_MieiVeicoliOutModel>> Ast_MieiVeicoliGet(GenericRequest<Ast_EmptyInModel> model, Boolean isSubProcess);
    }
}
