using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.HumanResources;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.Common;
using nvxapp.server.data.Repositories.Tenant.HumanResources;
using nvxapp.server.service.ClientServer_Service.HumanResources._utility;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.HumanResources
{
    /*
     Formazione e competenze (HR-FOR, ANALISI_HUMANRESOURCES_APPLICATIVO.md §4.3), per l'ufficio del personale:
        - catalogo dei corsi (competenza comune collegata, validita' in mesi)
        - edizioni con i partecipanti: esito, conseguimento, scadenza calcolata e attestato (IFileStorage)
          Corso superato: competenza del dipendente in Common (Dip_Competenza) e scadenza nello scadenzario comune
          (IDeadlineRegistry, fonte hr.corso, riferimento il partecipante); il rinnovo chiude le scadenze precedenti
        - piani di formazione per anno (corso da seguire per dipendente entro una data), con l'avanzamento calcolato
    */
    public class Hr_FormazioneService : ServiceBase, IHr_FormazioneService
    {
        public const string FonteScadenza = "hr.corso";
        public const long MaxAttestato = 20L * 1024 * 1024;

        private readonly IHrUtility _utility;
        private readonly IHr_CorsoRepository _Hr_Corso;
        private readonly IHr_CorsoEdizioneRepository _Hr_CorsoEdizione;
        private readonly IHr_CorsoPartecipanteRepository _Hr_CorsoPartecipante;
        private readonly IHr_PianoFormazioneRepository _Hr_PianoFormazione;
        private readonly IHr_PianoFormazioneRigaRepository _Hr_PianoFormazioneRiga;
        private readonly IHr_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly IHr_Par_CompetenzaReadRepository _Par_Competenza;
        private readonly IHr_Az_SediReadRepository _Az_Sedi;
        private readonly IDip_CompetenzaCommonRepository _Dip_CompetenzaCommon;
        private readonly IDeadlineRegistry _deadlineRegistry;
        private readonly IFileStorage _fileStorage;

        public Hr_FormazioneService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,
                                    IHrUtility utility,
                                    IHr_CorsoRepository hr_Corso,
                                    IHr_CorsoEdizioneRepository hr_CorsoEdizione,
                                    IHr_CorsoPartecipanteRepository hr_CorsoPartecipante,
                                    IHr_PianoFormazioneRepository hr_PianoFormazione,
                                    IHr_PianoFormazioneRigaRepository hr_PianoFormazioneRiga,
                                    IHr_Dip_AnagraficaReadRepository dip_Anagrafica,
                                    IHr_Par_CompetenzaReadRepository par_Competenza,
                                    IHr_Az_SediReadRepository az_Sedi,
                                    IDip_CompetenzaCommonRepository dip_CompetenzaCommon,
                                    IDeadlineRegistry deadlineRegistry,
                                    IFileStorage fileStorage) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _utility = utility;
            _Hr_Corso = hr_Corso;
            _Hr_CorsoEdizione = hr_CorsoEdizione;
            _Hr_CorsoPartecipante = hr_CorsoPartecipante;
            _Hr_PianoFormazione = hr_PianoFormazione;
            _Hr_PianoFormazioneRiga = hr_PianoFormazioneRiga;
            _Dip_Anagrafica = dip_Anagrafica;
            _Par_Competenza = par_Competenza;
            _Az_Sedi = az_Sedi;
            _Dip_CompetenzaCommon = dip_CompetenzaCommon;
            _deadlineRegistry = deadlineRegistry;
            _fileStorage = fileStorage;
        }

        // ------------------------------------------------------------------------------------------------ corsi
        public virtual async Task<GenericResult<Hr_CorsoListOutModel>> Hr_CorsoList(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                return new Hr_CorsoListOutModel { Corsi = await CorsiAsync(ctx, null) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_CorsoGetOutModel>> Hr_CorsoGet(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var corso = (await CorsiAsync(ctx, model.Data.Id)).FirstOrDefault() ?? throw new UserMessageException(HumanResourcesMessages.Get("CourseNotFound"));
                var edizioni = await _Hr_CorsoEdizione.FindAll(x => x.IdHr_Corso == corso.Id).OrderByDescending(x => x.DataInizio)
                    .Select(x => new Hr_EdizioneBreveModel
                    {
                        Id = x.Id, DataInizio = x.DataInizio, DataFine = x.DataFine, Docente = x.Docente,
                        Sede = x.Az_SediNavigation != null ? x.Az_SediNavigation.Descrizione : x.Luogo,
                        NumeroPartecipanti = x.Hr_CorsoPartecipante!.Count(),
                        NumeroSuperati = x.Hr_CorsoPartecipante!.Count(p => p.Esito == Hr_EsitoCorso.Superato)
                    }).ToListAsync();
                return new Hr_CorsoGetOutModel { Corso = corso, Edizioni = edizioni };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_CorsoPutOutModel>> Hr_CorsoPut(GenericRequest<Hr_CorsoPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var d = model.Data.Corso;
                if (string.IsNullOrWhiteSpace(d.Titolo))
                    throw new UserMessageException(HumanResourcesMessages.Get("CourseTitleRequired"));
                if (d.IdPar_Competenza is int idc && !await _Par_Competenza.FindAll(x => x.Id == idc && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    throw new UserMessageException(HumanResourcesMessages.Get("SkillNotFound"));

                Hr_Corso c;
                if (d.Id != 0)
                    c = await _Hr_Corso.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(HumanResourcesMessages.Get("CourseNotFound"));
                else
                {
                    c = new Hr_Corso { IdAz_Anagrafica = ctx.IdAz_Anagrafica };
                    await _Hr_Corso.CreateAsync(c, false);
                }
                int? validitaPrima = c.Id == 0 ? null : c.ValiditaMesi;
                c.Titolo = HrPdf.Taglia(d.Titolo.Trim(), 200);
                c.Ente = Testo(d.Ente, 200);
                c.DurataOre = d.DurataOre is decimal o && o > 0 ? Math.Round(o, 2) : null;
                c.ValiditaMesi = d.ValiditaMesi is int v && v > 0 ? Math.Min(v, 600) : null;
                c.IdPar_Competenza = d.IdPar_Competenza;
                c.Obbligatorio = d.Obbligatorio;
                c.Attivo = d.Attivo;
                c.Note = Testo(d.Note, 2000);
                c.ModifiedDate = DateTime.Now;
                c.ChangeUser = ctx.IdUtente;
                await _Hr_Corso.SaveChange();

                // validita' cambiata: si ricalcolano le scadenze dei partecipanti promossi
                if (d.Id != 0 && validitaPrima != c.ValiditaMesi)
                {
                    var cfg = await _utility.CfgAsync(ctx);
                    var partecipanti = await _Hr_CorsoPartecipante.FindAll(x => x.Hr_CorsoEdizioneNavigation!.IdHr_Corso == c.Id && x.Esito == Hr_EsitoCorso.Superato)
                                                                  .AsTracking().ToListAsync();
                    foreach (var p in partecipanti)
                        p.DataScadenza = Scadenza(p.DataConseguimento, c.ValiditaMesi);
                    await _Hr_CorsoPartecipante.SaveChange();
                    foreach (var p in partecipanti)
                        await ScadenzaAsync(ctx, cfg, c, p);
                }
                return new Hr_CorsoPutOutModel { Corso = (await CorsiAsync(ctx, c.Id)).First() };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_IdOutModel>> Hr_CorsoDelete(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var c = await _Hr_Corso.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(HumanResourcesMessages.Get("CourseNotFound"));
                if (await _Hr_CorsoEdizione.FindAll(x => x.IdHr_Corso == c.Id).AnyAsync() || await _Hr_PianoFormazioneRiga.FindAll(x => x.IdHr_Corso == c.Id).AnyAsync())
                    throw new UserMessageException(HumanResourcesMessages.Get("CourseInUse"));
                await _Hr_Corso.DeleteAsync(c);
                return new Hr_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        private async Task<List<Hr_CorsoModel>> CorsiAsync(HrContesto ctx, int? id)
            => await _Hr_Corso.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && (id == null || x.Id == id)).OrderBy(x => x.Titolo)
                .Select(x => new Hr_CorsoModel
                {
                    Id = x.Id, Titolo = x.Titolo, Ente = x.Ente, DurataOre = x.DurataOre, ValiditaMesi = x.ValiditaMesi, IdPar_Competenza = x.IdPar_Competenza,
                    Competenza = x.Par_CompetenzaNavigation != null ? x.Par_CompetenzaNavigation.Descrizione : null, Obbligatorio = x.Obbligatorio, Attivo = x.Attivo,
                    Note = x.Note, NumeroEdizioni = x.Hr_CorsoEdizione!.Count()
                }).ToListAsync();

        // ------------------------------------------------------------------------------------------------ edizioni
        public virtual async Task<GenericResult<Hr_EdizioneGetOutModel>> Hr_EdizioneGet(GenericRequest<Hr_EdizioneGetInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                Hr_EdizioneModel edizione;
                int idCorso = model.Data.IdHr_Corso;
                if (model.Data.Id != 0)
                {
                    edizione = await EdizioneAsync(ctx, model.Data.Id);
                    idCorso = edizione.IdHr_Corso;
                }
                else
                    edizione = new Hr_EdizioneModel { IdHr_Corso = idCorso, DataInizio = DateTime.Today };
                var corso = await _Hr_Corso.FindAll(x => x.Id == idCorso && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                            ?? throw new UserMessageException(HumanResourcesMessages.Get("CourseNotFound"));
                return new Hr_EdizioneGetOutModel { Corso = corso.Titolo, ValiditaMesi = corso.ValiditaMesi, Edizione = edizione, Dipendenti = await DipendentiAsync(ctx) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_EdizionePutOutModel>> Hr_EdizionePut(GenericRequest<Hr_EdizionePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var d = model.Data.Edizione;
                Hr_CorsoEdizione e;
                if (d.Id != 0)
                    e = await _Hr_CorsoEdizione.FindAll(x => x.Id == d.Id && x.Hr_CorsoNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(HumanResourcesMessages.Get("EditionNotFound"));
                else
                {
                    if (!await _Hr_Corso.FindAll(x => x.Id == d.IdHr_Corso && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                        throw new UserMessageException(HumanResourcesMessages.Get("CourseNotFound"));
                    e = new Hr_CorsoEdizione { IdHr_Corso = d.IdHr_Corso };
                    await _Hr_CorsoEdizione.CreateAsync(e, false);
                }
                var corso = await _Hr_Corso.FindAll(x => x.Id == e.IdHr_Corso).FirstAsync();
                if (d.DataFine != null && d.DataFine.Value.Date < d.DataInizio.Date)
                    throw new UserMessageException(HumanResourcesMessages.Get("EditionDatesInvalid"));
                if (d.IdAz_Sedi is int ids && !await _Az_Sedi.FindAll(x => x.Id == ids && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AnyAsync())
                    throw new UserMessageException(HumanResourcesMessages.Get("SiteNotFound"));
                e.DataInizio = d.DataInizio.Date;
                e.DataFine = d.DataFine?.Date;
                e.IdAz_Sedi = d.IdAz_Sedi;
                e.Luogo = Testo(d.Luogo, 200);
                e.Docente = Testo(d.Docente, 200);
                e.Note = Testo(d.Note, 2000);
                e.ModifiedDate = DateTime.Now;
                e.ChangeUser = ctx.IdUtente;

                // partecipanti: dipendenti dell'azienda, una sola volta per edizione
                var idDip = d.Partecipanti.Select(x => x.IdDip_Anagrafica).ToList();
                if (idDip.Distinct().Count() != idDip.Count)
                    throw new UserMessageException(HumanResourcesMessages.Get("ParticipantDuplicated"));
                var validi = await _Dip_Anagrafica.FindAll(x => idDip.Contains(x.Id) && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).Select(x => x.Id).ToListAsync();
                if (validi.Count != idDip.Count)
                    throw new UserMessageException(HumanResourcesMessages.Get("EmployeeNotFound"));

                var esistenti = e.Id == 0 ? new List<Hr_CorsoPartecipante>() : await _Hr_CorsoPartecipante.FindAll(x => x.IdHr_CorsoEdizione == e.Id).AsTracking().ToListAsync();
                var rimossi = esistenti.Where(x => !idDip.Contains(x.IdDip_Anagrafica)).ToList();
                foreach (var r in rimossi)
                    await _Hr_CorsoPartecipante.DeleteAsync(r);
                var salvati = new List<Hr_CorsoPartecipante>();
                foreach (var pd in d.Partecipanti)
                {
                    var p = esistenti.FirstOrDefault(x => x.IdDip_Anagrafica == pd.IdDip_Anagrafica);
                    if (p == null)
                    {
                        p = new Hr_CorsoPartecipante { IdDip_Anagrafica = pd.IdDip_Anagrafica, Hr_CorsoEdizioneNavigation = e };
                        await _Hr_CorsoPartecipante.CreateAsync(p, false);
                    }
                    p.Esito = Enum.IsDefined(typeof(Hr_EsitoCorso), pd.Esito) ? (Hr_EsitoCorso)pd.Esito : Hr_EsitoCorso.Iscritto;
                    if (p.Esito == Hr_EsitoCorso.Superato)
                    {
                        p.DataConseguimento = (pd.DataConseguimento ?? e.DataFine ?? e.DataInizio).Date;
                        p.DataScadenza = Scadenza(p.DataConseguimento, corso.ValiditaMesi);
                    }
                    else
                    {
                        p.DataConseguimento = null;
                        p.DataScadenza = null;
                    }
                    p.ModifiedDate = DateTime.Now;
                    salvati.Add(p);
                }
                await _Hr_CorsoEdizione.SaveChange();

                // dopo il salvataggio: scadenze, competenze comuni, file degli attestati tolti
                var cfg = await _utility.CfgAsync(ctx);
                foreach (var r in rimossi)
                {
                    await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.HumanResources, FonteScadenza, r.Id);
                    if (r.AttestatoStorageKey != null)
                        await EliminaFileAsync(r.AttestatoStorageKey);
                }
                foreach (var p in salvati)
                {
                    await ScadenzaAsync(ctx, cfg, corso, p);
                    if (p.Esito == Hr_EsitoCorso.Superato && corso.IdPar_Competenza is int idComp
                        && !await _Dip_CompetenzaCommon.FindAll(x => x.IdDip_Anagrafica == p.IdDip_Anagrafica && x.IdPar_Competenza == idComp).AnyAsync())
                        await _Dip_CompetenzaCommon.CreateAsync(new Dip_Competenza { IdDip_Anagrafica = p.IdDip_Anagrafica, IdPar_Competenza = idComp });
                }
                return new Hr_EdizionePutOutModel { Edizione = await EdizioneAsync(ctx, e.Id) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_IdOutModel>> Hr_EdizioneDelete(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var e = await _Hr_CorsoEdizione.FindAll(x => x.Id == model.Data.Id && x.Hr_CorsoNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(HumanResourcesMessages.Get("EditionNotFound"));
                var partecipanti = await _Hr_CorsoPartecipante.FindAll(x => x.IdHr_CorsoEdizione == e.Id).Select(x => new { x.Id, x.AttestatoStorageKey }).ToListAsync();
                await _Hr_CorsoEdizione.DeleteAsync(e);
                foreach (var p in partecipanti)
                {
                    await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.HumanResources, FonteScadenza, p.Id);
                    if (p.AttestatoStorageKey != null)
                        await EliminaFileAsync(p.AttestatoStorageKey);
                }
                return new Hr_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        private async Task<Hr_EdizioneModel> EdizioneAsync(HrContesto ctx, int id)
        {
            var e = await _Hr_CorsoEdizione.FindAll(x => x.Id == id && x.Hr_CorsoNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                .Select(x => new Hr_EdizioneModel
                {
                    Id = x.Id, IdHr_Corso = x.IdHr_Corso, DataInizio = x.DataInizio, DataFine = x.DataFine, IdAz_Sedi = x.IdAz_Sedi, Luogo = x.Luogo,
                    Docente = x.Docente, Note = x.Note
                }).FirstOrDefaultAsync() ?? throw new UserMessageException(HumanResourcesMessages.Get("EditionNotFound"));
            e.Partecipanti = await PartecipantiAsync(_Hr_CorsoPartecipante.FindAll(x => x.IdHr_CorsoEdizione == id));
            return e;
        }

        private static async Task<List<Hr_PartecipanteModel>> PartecipantiAsync(IQueryable<Hr_CorsoPartecipante> q)
            => await q.Select(x => new Hr_PartecipanteModel
            {
                Id = x.Id, IdDip_Anagrafica = x.IdDip_Anagrafica, Dipendente = (x.Dip_AnagraficaNavigation!.Cognome + " " + x.Dip_AnagraficaNavigation.Nome).Trim(),
                Esito = (int)x.Esito, DataConseguimento = x.DataConseguimento, DataScadenza = x.DataScadenza, AttestatoNomeFile = x.AttestatoNomeFile
            }).OrderBy(x => x.Dipendente).ToListAsync();

        // scadenza del corso nello scadenzario comune; il rinnovo (scadenza piu' lontana dello stesso corso) chiude le precedenti
        private async Task ScadenzaAsync(HrContesto ctx, Hr_Cfg cfg, Hr_Corso corso, Hr_CorsoPartecipante p)
        {
            if (p.DataScadenza is not DateTime scad || p.Esito != Hr_EsitoCorso.Superato)
            {
                await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.HumanResources, FonteScadenza, p.Id);
                return;
            }
            var dip = await _Dip_Anagrafica.FindAll(x => x.Id == p.IdDip_Anagrafica).FirstAsync();
            await _deadlineRegistry.UpsertAsync(new DeadlineRegistration
            {
                IdCompany = ctx.IdCompany, Application = ApplicationType.HumanResources, Fonte = FonteScadenza, IdRiferimento = p.Id,
                Descrizione = HumanResourcesMessages.Get("CourseDeadline", corso.Titolo, HrUtility.NomeDipendente(dip)),
                DataScadenza = scad, GiorniPreavviso = cfg.GiorniPreavviso, Url = "/hrcourseedition", UrlState = new { id = p.IdHr_CorsoEdizione }
            });
            var precedenti = await _Hr_CorsoPartecipante.FindAll(x => x.Id != p.Id && x.IdDip_Anagrafica == p.IdDip_Anagrafica && x.Esito == Hr_EsitoCorso.Superato
                                                                      && x.Hr_CorsoEdizioneNavigation!.IdHr_Corso == corso.Id && x.DataScadenza != null && x.DataScadenza < scad)
                                                        .Select(x => x.Id).ToListAsync();
            foreach (var id in precedenti)
                await _deadlineRegistry.CloseAsync(ctx.IdCompany, ApplicationType.HumanResources, FonteScadenza, id);
        }

        private static DateTime? Scadenza(DateTime? conseguimento, int? validitaMesi)
            => conseguimento is DateTime c && validitaMesi is int v && v > 0 ? c.Date.AddMonths(v) : null;

        // ------------------------------------------------------------------------------------------------ attestati
        public virtual async Task<GenericResult<Hr_AttestatoUploadOutModel>> Hr_AttestatoUpload(GenericRequest<Hr_AttestatoUploadModel> model, Stream content, string fileName, string? contentType, long length, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                if (length <= 0)
                    throw new UserMessageException(HumanResourcesMessages.Get("FileEmpty"));
                if (length > MaxAttestato)
                    throw new UserMessageException(HumanResourcesMessages.Get("FileTooLarge", MaxAttestato / (1024 * 1024)));
                var p = await _Hr_CorsoPartecipante.FindAll(x => x.Id == model.Data.IdPartecipante && x.Hr_CorsoEdizioneNavigation!.Hr_CorsoNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                                   .AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(HumanResourcesMessages.Get("ParticipantNotFound"));
                var key = await _fileStorage.SaveAsync(ctx.IdCompany, content);
                var vecchia = p.AttestatoStorageKey;
                p.AttestatoStorageKey = key;
                p.AttestatoNomeFile = HrCedoliniCore.NomeFile(fileName);
                p.AttestatoContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : HrPdf.Taglia(contentType, 100);
                p.AttestatoDimensione = length;
                p.ModifiedDate = DateTime.Now;
                try
                {
                    await _Hr_CorsoPartecipante.SaveChange();
                }
                catch
                {
                    await _fileStorage.DeleteAsync(key);
                    throw;
                }
                if (vecchia != null)
                    await EliminaFileAsync(vecchia);
                return new Hr_AttestatoUploadOutModel { Partecipante = (await PartecipantiAsync(_Hr_CorsoPartecipante.FindAll(x => x.Id == p.Id))).First() };
            }, isSubProcess);
        }

        // attestato: per l'ufficio del personale o per il dipendente stesso
        public virtual async Task<Hr_FileDownload> Hr_AttestatoDownload(int id)
        {
            var ctx = await _utility.ContestoAsync();
            var p = await _Hr_CorsoPartecipante.FindAll(x => x.Id == id && x.Hr_CorsoEdizioneNavigation!.Hr_CorsoNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                                               .Include(x => x.Dip_AnagraficaNavigation).FirstOrDefaultAsync();
            if (p?.AttestatoStorageKey == null || (!ctx.Amministratore && p.Dip_AnagraficaNavigation?.IdAspNetUsers != ctx.IdUtente))
                throw new UserMessageException(HumanResourcesMessages.Get("DocumentNotFound"));
            return new Hr_FileDownload { Content = await _fileStorage.OpenAsync(p.AttestatoStorageKey), NomeFile = p.AttestatoNomeFile ?? "attestato", ContentType = p.AttestatoContentType ?? "application/octet-stream" };
        }

        // ------------------------------------------------------------------------------------------------ piani di formazione
        public virtual async Task<GenericResult<Hr_PianoListOutModel>> Hr_PianoList(GenericRequest<Hr_PianoListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var piani = await _Hr_PianoFormazione.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && (model.Data.Anno == null || x.Anno == model.Data.Anno))
                    .OrderByDescending(x => x.Anno).ThenBy(x => x.Descrizione)
                    .Select(x => new Hr_PianoModel { Id = x.Id, Anno = x.Anno, Descrizione = x.Descrizione, Note = x.Note }).ToListAsync();
                foreach (var p in piani)
                {
                    var righe = await RigheAsync(p.Id, p.Anno);
                    p.NumeroRighe = righe.Count;
                    p.NumeroSvolte = righe.Count(x => x.Svolto);
                }
                return new Hr_PianoListOutModel { Piani = piani };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_PianoGetOutModel>> Hr_PianoGet(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var piano = model.Data.Id == 0 ? new Hr_PianoModel { Anno = DateTime.Today.Year } : await PianoAsync(ctx, model.Data.Id);
                return new Hr_PianoGetOutModel { Piano = piano, Corsi = await CorsiAsync(ctx, null), Dipendenti = await DipendentiAsync(ctx) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_PianoPutOutModel>> Hr_PianoPut(GenericRequest<Hr_PianoPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var d = model.Data.Piano;
                if (string.IsNullOrWhiteSpace(d.Descrizione))
                    throw new UserMessageException(HumanResourcesMessages.Get("PlanDescriptionRequired"));
                if (d.Anno < 2000 || d.Anno > 2100)
                    throw new UserMessageException(HumanResourcesMessages.Get("PayslipYearInvalid"));
                var idCorsi = d.Righe.Select(x => x.IdHr_Corso).Distinct().ToList();
                var idDip = d.Righe.Select(x => x.IdDip_Anagrafica).Distinct().ToList();
                if (await _Hr_Corso.FindAll(x => idCorsi.Contains(x.Id) && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).CountAsync() != idCorsi.Count)
                    throw new UserMessageException(HumanResourcesMessages.Get("CourseNotFound"));
                if (await _Dip_Anagrafica.FindAll(x => idDip.Contains(x.Id) && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).CountAsync() != idDip.Count)
                    throw new UserMessageException(HumanResourcesMessages.Get("EmployeeNotFound"));

                Hr_PianoFormazione p;
                if (d.Id != 0)
                    p = await _Hr_PianoFormazione.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(HumanResourcesMessages.Get("PlanNotFound"));
                else
                {
                    p = new Hr_PianoFormazione { IdAz_Anagrafica = ctx.IdAz_Anagrafica };
                    await _Hr_PianoFormazione.CreateAsync(p, false);
                }
                p.Anno = d.Anno;
                p.Descrizione = HrPdf.Taglia(d.Descrizione.Trim(), 200);
                p.Note = Testo(d.Note, 2000);
                p.ModifiedDate = DateTime.Now;
                p.ChangeUser = ctx.IdUtente;

                // righe: si aggiornano quelle con l'id, si aggiungono le nuove, si tolgono le altre
                var esistenti = p.Id == 0 ? new List<Hr_PianoFormazioneRiga>() : await _Hr_PianoFormazioneRiga.FindAll(x => x.IdHr_PianoFormazione == p.Id).AsTracking().ToListAsync();
                var tenute = d.Righe.Where(x => x.Id != 0).Select(x => x.Id).ToHashSet();
                foreach (var r in esistenti.Where(x => !tenute.Contains(x.Id)))
                    await _Hr_PianoFormazioneRiga.DeleteAsync(r);
                foreach (var rd in d.Righe)
                {
                    var r = esistenti.FirstOrDefault(x => x.Id == rd.Id && rd.Id != 0);
                    if (r == null)
                    {
                        r = new Hr_PianoFormazioneRiga { Hr_PianoFormazioneNavigation = p };
                        await _Hr_PianoFormazioneRiga.CreateAsync(r, false);
                    }
                    r.IdHr_Corso = rd.IdHr_Corso;
                    r.IdDip_Anagrafica = rd.IdDip_Anagrafica;
                    r.EntroIl = rd.EntroIl?.Date;
                    r.Note = Testo(rd.Note, 500);
                    r.ModifiedDate = DateTime.Now;
                }
                await _Hr_PianoFormazione.SaveChange();
                return new Hr_PianoPutOutModel { Piano = await PianoAsync(ctx, p.Id) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_IdOutModel>> Hr_PianoDelete(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var p = await _Hr_PianoFormazione.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(HumanResourcesMessages.Get("PlanNotFound"));
                await _Hr_PianoFormazione.DeleteAsync(p);
                return new Hr_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        private async Task<Hr_PianoModel> PianoAsync(HrContesto ctx, int id)
        {
            var p = await _Hr_PianoFormazione.FindAll(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica)
                        .Select(x => new Hr_PianoModel { Id = x.Id, Anno = x.Anno, Descrizione = x.Descrizione, Note = x.Note }).FirstOrDefaultAsync()
                    ?? throw new UserMessageException(HumanResourcesMessages.Get("PlanNotFound"));
            p.Righe = await RigheAsync(p.Id, p.Anno);
            p.NumeroRighe = p.Righe.Count;
            p.NumeroSvolte = p.Righe.Count(x => x.Svolto);
            return p;
        }

        // righe con l'avanzamento: corso superato dal dipendente nell'anno del piano
        private async Task<List<Hr_PianoRigaModel>> RigheAsync(int idPiano, int anno)
        {
            var righe = await _Hr_PianoFormazioneRiga.FindAll(x => x.IdHr_PianoFormazione == idPiano)
                .Select(x => new Hr_PianoRigaModel
                {
                    Id = x.Id, IdHr_Corso = x.IdHr_Corso, Corso = x.Hr_CorsoNavigation!.Titolo, IdDip_Anagrafica = x.IdDip_Anagrafica,
                    Dipendente = (x.Dip_AnagraficaNavigation!.Cognome + " " + x.Dip_AnagraficaNavigation.Nome).Trim(), EntroIl = x.EntroIl, Note = x.Note
                }).OrderBy(x => x.Dipendente).ThenBy(x => x.Corso).ToListAsync();
            if (righe.Count == 0)
                return righe;
            var corsi = righe.Select(x => x.IdHr_Corso).Distinct().ToList();
            var dip = righe.Select(x => x.IdDip_Anagrafica).Distinct().ToList();
            var dal = new DateTime(anno, 1, 1);
            var superati = await _Hr_CorsoPartecipante.FindAll(x => x.Esito == Hr_EsitoCorso.Superato && dip.Contains(x.IdDip_Anagrafica)
                                                                    && corsi.Contains(x.Hr_CorsoEdizioneNavigation!.IdHr_Corso) && x.DataConseguimento >= dal)
                .Select(x => new { x.IdDip_Anagrafica, x.Hr_CorsoEdizioneNavigation!.IdHr_Corso, x.DataConseguimento }).ToListAsync();
            foreach (var r in righe)
            {
                var s = superati.Where(x => x.IdDip_Anagrafica == r.IdDip_Anagrafica && x.IdHr_Corso == r.IdHr_Corso).OrderBy(x => x.DataConseguimento).FirstOrDefault();
                r.Svolto = s != null;
                r.DataSvolto = s?.DataConseguimento;
            }
            return righe;
        }

        // ------------------------------------------------------------------------------------------------ utilita'
        private Task<List<Hr_DipendenteBreveModel>> DipendentiAsync(HrContesto ctx)
            => _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Cognome).ThenBy(x => x.Nome)
                              .Select(x => new Hr_DipendenteBreveModel
                              {
                                  Id = x.Id, Cognome = x.Cognome ?? string.Empty, Nome = x.Nome ?? string.Empty, Matricola = x.Matricola, CodiceFiscale = x.CodiceFiscale, Attivo = x.Attivo
                              }).ToListAsync();

        private static string? Testo(string? s, int max) => string.IsNullOrWhiteSpace(s) ? null : HrPdf.Taglia(s.Trim(), max);

        private async Task EliminaFileAsync(string key)
        {
            try { await _fileStorage.DeleteAsync(key); }
            catch (Exception ex) { Log.Error(ex, "[HR] File {Key} non eliminato", key); }
        }
    }

    public interface IHr_FormazioneService : IServiceBase
    {
        Task<GenericResult<Hr_CorsoListOutModel>> Hr_CorsoList(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_CorsoGetOutModel>> Hr_CorsoGet(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_CorsoPutOutModel>> Hr_CorsoPut(GenericRequest<Hr_CorsoPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_IdOutModel>> Hr_CorsoDelete(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_EdizioneGetOutModel>> Hr_EdizioneGet(GenericRequest<Hr_EdizioneGetInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_EdizionePutOutModel>> Hr_EdizionePut(GenericRequest<Hr_EdizionePutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_IdOutModel>> Hr_EdizioneDelete(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_AttestatoUploadOutModel>> Hr_AttestatoUpload(GenericRequest<Hr_AttestatoUploadModel> model, Stream content, string fileName, string? contentType, long length, Boolean isSubProcess);
        Task<Hr_FileDownload> Hr_AttestatoDownload(int id);
        Task<GenericResult<Hr_PianoListOutModel>> Hr_PianoList(GenericRequest<Hr_PianoListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_PianoGetOutModel>> Hr_PianoGet(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_PianoPutOutModel>> Hr_PianoPut(GenericRequest<Hr_PianoPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_IdOutModel>> Hr_PianoDelete(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess);
    }
}
