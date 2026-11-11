using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.HumanResources;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.HumanResources;
using nvxapp.server.service.ClientServer_Service.HumanResources._utility;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.HumanResources
{
    /*
     Onboarding e offboarding (HR-ONB, ANALISI_HUMANRESOURCES_APPLICATIVO.md §4.4), per l'ufficio del personale:
        - modelli di checklist (assunzione / cessazione) con le voci: responsabile, giorni rispetto alla data, documento da
          consegnare o firmare, azione sull'account (HR-ONB.3); i modelli li gestisce il CompanyPowerAdmin
        - checklist dei dipendenti generate dal modello predefinito sulle date di Common (HrOnboardingCore) o a mano
        - voci: completamento (scadenza chiusa nello scadenzario), documento (IFileStorage) e presa visione del dipendente
          (IAcknowledgementService, fonte hr.checklist.voce, con il file per il canale del documentale)
    */
    public class Hr_OnboardingService : ServiceBase, IHr_OnboardingService
    {
        public const string FontePresaVisione = "hr.checklist.voce";
        public const long MaxDocumento = 20L * 1024 * 1024;

        private readonly IHrUtility _utility;
        private readonly IHrOnboardingCore _core;
        private readonly IHr_ChecklistModelloRepository _Hr_ChecklistModello;
        private readonly IHr_ChecklistModelloVoceRepository _Hr_ChecklistModelloVoce;
        private readonly IHr_ChecklistRepository _Hr_Checklist;
        private readonly IHr_ChecklistVoceRepository _Hr_ChecklistVoce;
        private readonly IHr_Dip_AnagraficaReadRepository _Dip_Anagrafica;
        private readonly IUserCompanyRepository _userCompanyRepository;
        private readonly IPresaVisioneRepository _presaVisioneRepository;
        private readonly IDeadlineRegistry _deadlineRegistry;
        private readonly IAcknowledgementService _acknowledgementService;
        private readonly IFileStorage _fileStorage;

        public Hr_OnboardingService(IMapper mapper,
                                    UserManager<ApplicationUser> userManager,
                                    IAspNetUsersRepository aspNetUsersRepository,
                                    IOptions<JwtParameter> jwtParameter,
                                    IHttpContextAccessor httpContextAccessor,
                                    IConfiguration configuration,
                                    IHrUtility utility,
                                    IHrOnboardingCore core,
                                    IHr_ChecklistModelloRepository hr_ChecklistModello,
                                    IHr_ChecklistModelloVoceRepository hr_ChecklistModelloVoce,
                                    IHr_ChecklistRepository hr_Checklist,
                                    IHr_ChecklistVoceRepository hr_ChecklistVoce,
                                    IHr_Dip_AnagraficaReadRepository dip_Anagrafica,
                                    IUserCompanyRepository userCompanyRepository,
                                    IPresaVisioneRepository presaVisioneRepository,
                                    IDeadlineRegistry deadlineRegistry,
                                    IAcknowledgementService acknowledgementService,
                                    IFileStorage fileStorage) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _utility = utility;
            _core = core;
            _Hr_ChecklistModello = hr_ChecklistModello;
            _Hr_ChecklistModelloVoce = hr_ChecklistModelloVoce;
            _Hr_Checklist = hr_Checklist;
            _Hr_ChecklistVoce = hr_ChecklistVoce;
            _Dip_Anagrafica = dip_Anagrafica;
            _userCompanyRepository = userCompanyRepository;
            _presaVisioneRepository = presaVisioneRepository;
            _deadlineRegistry = deadlineRegistry;
            _acknowledgementService = acknowledgementService;
            _fileStorage = fileStorage;
        }

        // ------------------------------------------------------------------------------------------------ modelli
        public virtual async Task<GenericResult<Hr_ModelloListOutModel>> Hr_ModelloList(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                return new Hr_ModelloListOutModel { Modelli = await ModelliAsync(ctx, null) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_ModelloGetOutModel>> Hr_ModelloGet(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                Hr_ModelloModel m;
                if (model.Data.Id == 0)
                    m = new Hr_ModelloModel();
                else
                {
                    m = (await ModelliAsync(ctx, model.Data.Id)).FirstOrDefault() ?? throw new UserMessageException(HumanResourcesMessages.Get("TemplateNotFound"));
                    m.Voci = await _Hr_ChecklistModelloVoce.FindAll(x => x.IdHr_ChecklistModello == m.Id).OrderBy(x => x.Ordine).ThenBy(x => x.Id)
                        .Select(x => new Hr_ModelloVoceModel
                        {
                            Id = x.Id, Ordine = x.Ordine, Descrizione = x.Descrizione, IdAspNetUsersResponsabile = x.IdAspNetUsersResponsabile, Giorni = x.Giorni,
                            Documento = x.Documento, Azione = (int)x.Azione
                        }).ToListAsync();
                }
                return new Hr_ModelloGetOutModel { Modello = m, Utenti = await UtentiAsync(ctx) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_ModelloPutOutModel>> Hr_ModelloPut(GenericRequest<Hr_ModelloPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                if (!ctx.PowerAdmin)
                    throw new UserMessageException(HumanResourcesMessages.Get("NotAllowed"));
                var d = model.Data.Modello;
                if (string.IsNullOrWhiteSpace(d.Descrizione))
                    throw new UserMessageException(HumanResourcesMessages.Get("TemplateDescriptionRequired"));
                if (!Enum.IsDefined(typeof(Hr_TipoChecklist), d.Tipo))
                    throw new UserMessageException(HumanResourcesMessages.Get("TemplateTypeInvalid"));
                if (d.Voci.Any(v => string.IsNullOrWhiteSpace(v.Descrizione)))
                    throw new UserMessageException(HumanResourcesMessages.Get("TemplateItemRequired"));
                var utenti = (await UtentiAsync(ctx)).Select(x => x.Id).ToHashSet();

                Hr_ChecklistModello m;
                if (d.Id != 0)
                    m = await _Hr_ChecklistModello.FindAll(x => x.Id == d.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(HumanResourcesMessages.Get("TemplateNotFound"));
                else
                {
                    m = new Hr_ChecklistModello { IdAz_Anagrafica = ctx.IdAz_Anagrafica };
                    await _Hr_ChecklistModello.CreateAsync(m, false);
                }
                m.Tipo = (Hr_TipoChecklist)d.Tipo;
                m.Descrizione = HrPdf.Taglia(d.Descrizione.Trim(), 200);
                m.Predefinito = d.Predefinito && d.Attivo;
                m.Attivo = d.Attivo;
                m.ModifiedDate = DateTime.Now;
                m.ChangeUser = ctx.IdUtente;

                var esistenti = m.Id == 0 ? new List<Hr_ChecklistModelloVoce>() : await _Hr_ChecklistModelloVoce.FindAll(x => x.IdHr_ChecklistModello == m.Id).AsTracking().ToListAsync();
                var tenute = d.Voci.Where(x => x.Id != 0).Select(x => x.Id).ToHashSet();
                foreach (var v in esistenti.Where(x => !tenute.Contains(x.Id)))
                    await _Hr_ChecklistModelloVoce.DeleteAsync(v);
                int ordine = 0;
                foreach (var vd in d.Voci)
                {
                    var v = esistenti.FirstOrDefault(x => x.Id == vd.Id && vd.Id != 0);
                    if (v == null)
                    {
                        v = new Hr_ChecklistModelloVoce { Hr_ChecklistModelloNavigation = m };
                        await _Hr_ChecklistModelloVoce.CreateAsync(v, false);
                    }
                    v.Ordine = ++ordine;
                    v.Descrizione = HrPdf.Taglia(vd.Descrizione.Trim(), 300);
                    v.IdAspNetUsersResponsabile = vd.IdAspNetUsersResponsabile != null && utenti.Contains(vd.IdAspNetUsersResponsabile) ? vd.IdAspNetUsersResponsabile : null;
                    v.Giorni = Math.Clamp(vd.Giorni, -365, 365);
                    v.Documento = vd.Documento;
                    v.Azione = Enum.IsDefined(typeof(Hr_AzioneVoce), vd.Azione) ? (Hr_AzioneVoce)vd.Azione : Hr_AzioneVoce.Nessuna;
                    v.ModifiedDate = DateTime.Now;
                }
                await _Hr_ChecklistModello.SaveChange();

                // un solo modello predefinito per tipo
                if (m.Predefinito)
                    await _Hr_ChecklistModello.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && x.Tipo == m.Tipo && x.Id != m.Id && x.Predefinito)
                                              .ExecuteUpdateAsync(u => u.SetProperty(x => x.Predefinito, false));
                var outModel = new Hr_ModelloPutOutModel { Modello = (await ModelliAsync(ctx, m.Id)).First() };
                return outModel;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_IdOutModel>> Hr_ModelloDelete(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                if (!ctx.PowerAdmin)
                    throw new UserMessageException(HumanResourcesMessages.Get("NotAllowed"));
                var m = await _Hr_ChecklistModello.FindAll(x => x.Id == model.Data.Id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).AsTracking().FirstOrDefaultAsync()
                        ?? throw new UserMessageException(HumanResourcesMessages.Get("TemplateNotFound"));
                // le checklist gia' generate restano (copia delle voci): perdono solo il riferimento al modello
                await _Hr_Checklist.FindAll(x => x.IdHr_ChecklistModello == m.Id).ExecuteUpdateAsync(u => u.SetProperty(x => x.IdHr_ChecklistModello, (int?)null));
                await _Hr_ChecklistModello.DeleteAsync(m);
                return new Hr_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        private async Task<List<Hr_ModelloModel>> ModelliAsync(HrContesto ctx, int? id)
            => await _Hr_ChecklistModello.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && (id == null || x.Id == id))
                .OrderBy(x => x.Tipo).ThenBy(x => x.Descrizione)
                .Select(x => new Hr_ModelloModel
                {
                    Id = x.Id, Tipo = (int)x.Tipo, Descrizione = x.Descrizione, Predefinito = x.Predefinito, Attivo = x.Attivo,
                    NumeroVoci = x.Hr_ChecklistModelloVoce!.Count()
                }).ToListAsync();

        // ------------------------------------------------------------------------------------------------ checklist
        public virtual async Task<GenericResult<Hr_ChecklistListOutModel>> Hr_ChecklistList(GenericRequest<Hr_ChecklistListInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                await _core.SincronizzaAsync(ctx);
                return new Hr_ChecklistListOutModel
                {
                    Checklist = await ChecklistAsync(ctx, null, model.Data.SoloAperte),
                    Modelli = (await ModelliAsync(ctx, null)).Where(x => x.Attivo).ToList(),
                    Dipendenti = await _Dip_Anagrafica.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).OrderBy(x => x.Cognome).ThenBy(x => x.Nome)
                        .Select(x => new Hr_DipendenteBreveModel { Id = x.Id, Cognome = x.Cognome ?? "", Nome = x.Nome ?? "", Matricola = x.Matricola, CodiceFiscale = x.CodiceFiscale, Attivo = x.Attivo })
                        .ToListAsync()
                };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_ChecklistGetOutModel>> Hr_ChecklistGet(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                return await DettaglioAsync(ctx, model.Data.Id);
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_ChecklistGetOutModel>> Hr_ChecklistCrea(GenericRequest<Hr_ChecklistCreaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var dip = await _utility.DipendenteAsync(ctx, model.Data.IdDip_Anagrafica);
                var m = await _Hr_ChecklistModello.FindAll(x => x.Id == model.Data.IdHr_ChecklistModello && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica).FirstOrDefaultAsync()
                        ?? throw new UserMessageException(HumanResourcesMessages.Get("TemplateNotFound"));
                var data = model.Data.DataRiferimento ?? (m.Tipo == Hr_TipoChecklist.Assunzione ? dip.DataAssunzione : dip.DataCessazione)
                           ?? throw new UserMessageException(HumanResourcesMessages.Get(m.Tipo == Hr_TipoChecklist.Assunzione ? "ChecklistNoHireDate" : "ChecklistNoEndDate"));
                var c = await _core.CreaAsync(ctx, dip, m, data);
                return await DettaglioAsync(ctx, c.Id);
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_ChecklistGetOutModel>> Hr_ChecklistPut(GenericRequest<Hr_ChecklistPutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var c = await Checklist(ctx, model.Data.Id, true);
                c.Note = string.IsNullOrWhiteSpace(model.Data.Note) ? null : HrPdf.Taglia(model.Data.Note.Trim(), 2000);
                bool annullata = model.Data.Stato == (int)Hr_StatoChecklist.Annullata;
                bool cambiata = annullata != (c.Stato == Hr_StatoChecklist.Annullata);
                if (cambiata)
                    c.Stato = annullata ? Hr_StatoChecklist.Annullata : Hr_StatoChecklist.Aperta;
                c.ModifiedDate = DateTime.Now;
                await _Hr_Checklist.SaveChange();
                if (cambiata)
                {
                    // annullata: niente piu' scadenze ne' prese visione in attesa; riaperta: tornano le scadenze
                    var cfg = await _utility.CfgAsync(ctx);
                    var dip = await _utility.DipendenteAsync(ctx, c.IdDip_Anagrafica);
                    var voci = await _Hr_ChecklistVoce.FindAll(x => x.IdHr_Checklist == c.Id).ToListAsync();
                    foreach (var v in voci)
                    {
                        await _core.ScadenzaVoceAsync(ctx, cfg, c, v, dip);
                        if (annullata)
                            await _acknowledgementService.CancelAsync(ctx.IdCompany, ApplicationType.HumanResources, FontePresaVisione, v.Id);
                    }
                    if (!annullata)
                        await _core.AggiornaStatoAsync(c.Id);
                }
                return await DettaglioAsync(ctx, c.Id);
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_IdOutModel>> Hr_ChecklistDelete(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var c = await Checklist(ctx, model.Data.Id, true);
                var voci = await _Hr_ChecklistVoce.FindAll(x => x.IdHr_Checklist == c.Id).Select(x => new { x.Id, x.DocumentoStorageKey }).ToListAsync();
                await _Hr_Checklist.DeleteAsync(c);
                foreach (var v in voci)
                {
                    await _deadlineRegistry.RemoveAsync(ctx.IdCompany, ApplicationType.HumanResources, HrOnboardingCore.FonteScadenza, v.Id);
                    await _acknowledgementService.CancelAsync(ctx.IdCompany, ApplicationType.HumanResources, FontePresaVisione, v.Id);
                    if (v.DocumentoStorageKey != null)
                        await EliminaFileAsync(v.DocumentoStorageKey);
                }
                return new Hr_IdOutModel { Id = model.Data.Id };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ voci
        public virtual async Task<GenericResult<Hr_ChecklistVoceOutModel>> Hr_ChecklistVocePut(GenericRequest<Hr_ChecklistVocePutInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var d = model.Data.Voce;
                var v = await Voce(ctx, d.Id, true);
                var c = await Checklist(ctx, v.IdHr_Checklist, false);
                if (c.Stato == Hr_StatoChecklist.Annullata)
                    throw new UserMessageException(HumanResourcesMessages.Get("ChecklistCancelled"));
                var utenti = (await UtentiAsync(ctx)).Select(x => x.Id).ToHashSet();
                v.IdAspNetUsersResponsabile = d.IdAspNetUsersResponsabile != null && utenti.Contains(d.IdAspNetUsersResponsabile) ? d.IdAspNetUsersResponsabile : null;
                if (d.DataPrevista != default)
                    v.DataPrevista = d.DataPrevista.Date;
                v.Note = string.IsNullOrWhiteSpace(d.Note) ? null : HrPdf.Taglia(d.Note.Trim(), 1000);
                if (d.Completata != v.Completata)
                {
                    v.Completata = d.Completata;
                    v.DataCompletamento = d.Completata ? DateTime.Now : null;
                    v.IdAspNetUsersCompletamento = d.Completata ? ctx.IdUtente : null;
                }
                v.ModifiedDate = DateTime.Now;
                await _Hr_ChecklistVoce.SaveChange();
                await _core.ScadenzaVoceAsync(ctx, await _utility.CfgAsync(ctx), c, v, await _utility.DipendenteAsync(ctx, c.IdDip_Anagrafica));
                await _core.AggiornaStatoAsync(c.Id);
                return new Hr_ChecklistVoceOutModel { Voce = (await DettaglioAsync(ctx, c.Id)).Voci.First(x => x.Id == v.Id) };
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Hr_ChecklistVoceOutModel>> Hr_ChecklistVoceDocUpload(GenericRequest<Hr_VoceDocUploadModel> model, Stream content, string fileName, string? contentType, long length, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                if (length <= 0)
                    throw new UserMessageException(HumanResourcesMessages.Get("FileEmpty"));
                if (length > MaxDocumento)
                    throw new UserMessageException(HumanResourcesMessages.Get("FileTooLarge", MaxDocumento / (1024 * 1024)));
                var v = await Voce(ctx, model.Data.IdVoce, true);
                var key = await _fileStorage.SaveAsync(ctx.IdCompany, content);
                var vecchia = v.DocumentoStorageKey;
                v.Documento = true;
                v.DocumentoStorageKey = key;
                v.DocumentoNomeFile = HrCedoliniCore.NomeFile(fileName);
                v.DocumentoContentType = string.IsNullOrWhiteSpace(contentType) ? "application/octet-stream" : HrPdf.Taglia(contentType, 100);
                v.DocumentoDimensione = length;
                v.ModifiedDate = DateTime.Now;
                try
                {
                    await _Hr_ChecklistVoce.SaveChange();
                }
                catch
                {
                    await _fileStorage.DeleteAsync(key);
                    throw;
                }
                if (vecchia != null)
                    await EliminaFileAsync(vecchia);
                return new Hr_ChecklistVoceOutModel { Voce = (await DettaglioAsync(ctx, v.IdHr_Checklist)).Voci.First(x => x.Id == v.Id) };
            }, isSubProcess);
        }

        public virtual async Task<Hr_FileDownload> Hr_ChecklistVoceDocDownload(int id)
        {
            var ctx = await _utility.AmministratoreAsync();
            var v = await Voce(ctx, id, false);
            if (v.DocumentoStorageKey == null)
                throw new UserMessageException(HumanResourcesMessages.Get("DocumentNotFound"));
            return new Hr_FileDownload { Content = await _fileStorage.OpenAsync(v.DocumentoStorageKey), NomeFile = v.DocumentoNomeFile ?? "documento", ContentType = v.DocumentoContentType ?? "application/octet-stream" };
        }

        // presa visione del documento (o della voce) chiesta al dipendente: il file va al canale del documentale se attivo (firma)
        public virtual async Task<GenericResult<Hr_ChecklistVoceOutModel>> Hr_ChecklistVocePresaVisione(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var ctx = await _utility.AmministratoreAsync();
                var v = await Voce(ctx, model.Data.Id, true);
                var c = await Checklist(ctx, v.IdHr_Checklist, false);
                if (c.Stato == Hr_StatoChecklist.Annullata)
                    throw new UserMessageException(HumanResourcesMessages.Get("ChecklistCancelled"));
                var dip = await _utility.DipendenteAsync(ctx, c.IdDip_Anagrafica);
                if (string.IsNullOrEmpty(dip.IdAspNetUsers))
                    throw new UserMessageException(HumanResourcesMessages.Get("AcknowledgementNoAccount"));
                byte[]? contenuto = null;
                if (v.DocumentoStorageKey != null)
                {
                    await using var s = await _fileStorage.OpenAsync(v.DocumentoStorageKey);
                    using var ms = new MemoryStream();
                    await s.CopyToAsync(ms);
                    contenuto = ms.ToArray();
                }
                await _acknowledgementService.RequestAsync(new AcknowledgementRequest
                {
                    IdCompany = ctx.IdCompany, Application = ApplicationType.HumanResources, Fonte = FontePresaVisione, IdRiferimento = v.Id,
                    UserIds = new[] { dip.IdAspNetUsers! }, Titolo = v.Descrizione, Testo = v.Note,
                    NomeFile = v.DocumentoNomeFile, Contenuto = contenuto, RichiediDiNuovo = v.DataRichiestaPresaVisione != null,
                    Notifica = (await _utility.CfgAsync(ctx)).NotificheAttive
                });
                v.DataRichiestaPresaVisione = DateTime.Now;
                v.ModifiedDate = DateTime.Now;
                await _Hr_ChecklistVoce.SaveChange();
                return new Hr_ChecklistVoceOutModel { Voce = (await DettaglioAsync(ctx, c.Id)).Voci.First(x => x.Id == v.Id) };
            }, isSubProcess);
        }

        // ------------------------------------------------------------------------------------------------ letture
        private async Task<List<Hr_ChecklistModel>> ChecklistAsync(HrContesto ctx, int? id, bool soloAperte)
        {
            var oggi = DateTime.Today;
            return await _Hr_Checklist.FindAll(x => x.IdAz_Anagrafica == ctx.IdAz_Anagrafica && (id == null || x.Id == id) && (!soloAperte || x.Stato == Hr_StatoChecklist.Aperta))
                .OrderBy(x => x.DataRiferimento)
                .Select(x => new Hr_ChecklistModel
                {
                    Id = x.Id, IdDip_Anagrafica = x.IdDip_Anagrafica, Dipendente = (x.Dip_AnagraficaNavigation!.Cognome + " " + x.Dip_AnagraficaNavigation.Nome).Trim(),
                    Tipo = (int)x.Tipo, Descrizione = x.Descrizione, DataRiferimento = x.DataRiferimento, Stato = (int)x.Stato, Note = x.Note,
                    NumeroVoci = x.Hr_ChecklistVoce!.Count(), NumeroCompletate = x.Hr_ChecklistVoce!.Count(v => v.Completata),
                    ProssimaScadenza = x.Hr_ChecklistVoce!.Where(v => !v.Completata).Min(v => (DateTime?)v.DataPrevista)
                }).ToListAsync();
        }

        private async Task<Hr_ChecklistGetOutModel> DettaglioAsync(HrContesto ctx, int id)
        {
            var c = (await ChecklistAsync(ctx, id, false)).FirstOrDefault() ?? throw new UserMessageException(HumanResourcesMessages.Get("ChecklistNotFound"));
            var utenti = await UtentiAsync(ctx);
            var nomi = utenti.ToDictionary(x => x.Id, x => x.Nome);
            var voci = await _Hr_ChecklistVoce.FindAll(x => x.IdHr_Checklist == id).OrderBy(x => x.DataPrevista).ThenBy(x => x.Ordine)
                .Select(x => new Hr_ChecklistVoceModel
                {
                    Id = x.Id, Ordine = x.Ordine, Descrizione = x.Descrizione, IdAspNetUsersResponsabile = x.IdAspNetUsersResponsabile, DataPrevista = x.DataPrevista,
                    Completata = x.Completata, DataCompletamento = x.DataCompletamento, Documento = x.Documento, DocumentoNomeFile = x.DocumentoNomeFile,
                    DataRichiestaPresaVisione = x.DataRichiestaPresaVisione, Azione = (int)x.Azione, Note = x.Note
                }).ToListAsync();
            var ids = voci.Select(x => (int?)x.Id).ToList();
            var prese = await _presaVisioneRepository.FindAll(x => x.IdCompany == ctx.IdCompany && x.Application == (int)ApplicationType.HumanResources
                                                                   && x.Fonte == FontePresaVisione && ids.Contains(x.IdRiferimento) && !x.Annullata)
                                                     .Select(x => new { x.IdRiferimento, x.DataConferma }).ToListAsync();
            foreach (var v in voci)
            {
                v.Responsabile = v.IdAspNetUsersResponsabile != null ? nomi.GetValueOrDefault(v.IdAspNetUsersResponsabile) : null;
                v.DataConferma = prese.Where(x => x.IdRiferimento == v.Id).Select(x => x.DataConferma).FirstOrDefault();
            }
            // documenti con la presa visione confermata: la voce si completa da sola
            foreach (var v in voci.Where(x => !x.Completata && x.Documento && x.DataConferma != null))
            {
                await _core.CompletaVoceAsync(ctx, v.Id);
                v.Completata = true;
                v.DataCompletamento = DateTime.Now;
            }
            if (voci.Count > 0 && voci.All(x => x.Completata) && c.Stato == (int)Hr_StatoChecklist.Aperta)
                c.Stato = (int)Hr_StatoChecklist.Completata;
            c.NumeroCompletate = voci.Count(x => x.Completata);

            var dip = await _Dip_Anagrafica.FindAll(x => x.Id == c.IdDip_Anagrafica).Include(x => x.AspNetUsersNavigation).FirstAsync();
            var account = new Hr_AccountModel { Presente = dip.IdAspNetUsers != null, UserName = dip.AspNetUsersNavigation?.UserName, Mail = dip.AspNetUsersNavigation?.Email };
            if (dip.IdAspNetUsers != null)
                account.Disabilitato = await _userCompanyRepository.FindAll(x => x.IdAspNetUsers == dip.IdAspNetUsers && x.IdCompany == ctx.IdCompany && x.Disabled).AnyAsync();
            return new Hr_ChecklistGetOutModel { Checklist = c, Voci = voci, Account = account, Utenti = utenti };
        }

        // amministratori dell'azienda: responsabili delle voci
        private async Task<List<Hr_UtenteModel>> UtentiAsync(HrContesto ctx)
        {
            var ids = await _userCompanyRepository.FindAll(x => x.IdCompany == ctx.IdCompany && !x.Disabled).Select(x => x.IdAspNetUsers).ToListAsync();
            var admin = new List<Hr_UtenteModel>();
            foreach (var ruolo in CompanyRecipients.RuoliAmministratori)
                foreach (var u in await _userManager.GetUsersInRoleAsync(ruolo))
                    if (ids.Contains(u.Id) && admin.All(x => x.Id != u.Id))
                        admin.Add(new Hr_UtenteModel { Id = u.Id, Nome = u.UserName ?? u.Id });
            return admin.OrderBy(x => x.Nome).ToList();
        }

        private async Task<Hr_Checklist> Checklist(HrContesto ctx, int id, bool tracking)
        {
            var q = _Hr_Checklist.FindAll(x => x.Id == id && x.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
            return await (tracking ? q.AsTracking() : q).FirstOrDefaultAsync() ?? throw new UserMessageException(HumanResourcesMessages.Get("ChecklistNotFound"));
        }

        private async Task<Hr_ChecklistVoce> Voce(HrContesto ctx, int id, bool tracking)
        {
            var q = _Hr_ChecklistVoce.FindAll(x => x.Id == id && x.Hr_ChecklistNavigation!.IdAz_Anagrafica == ctx.IdAz_Anagrafica);
            return await (tracking ? q.AsTracking() : q).FirstOrDefaultAsync() ?? throw new UserMessageException(HumanResourcesMessages.Get("ChecklistItemNotFound"));
        }

        private async Task EliminaFileAsync(string key)
        {
            try { await _fileStorage.DeleteAsync(key); }
            catch (Exception ex) { Log.Error(ex, "[HR] File {Key} non eliminato", key); }
        }
    }

    public interface IHr_OnboardingService : IServiceBase
    {
        Task<GenericResult<Hr_ModelloListOutModel>> Hr_ModelloList(GenericRequest<Hr_EmptyInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_ModelloGetOutModel>> Hr_ModelloGet(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_ModelloPutOutModel>> Hr_ModelloPut(GenericRequest<Hr_ModelloPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_IdOutModel>> Hr_ModelloDelete(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_ChecklistListOutModel>> Hr_ChecklistList(GenericRequest<Hr_ChecklistListInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_ChecklistGetOutModel>> Hr_ChecklistGet(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_ChecklistGetOutModel>> Hr_ChecklistCrea(GenericRequest<Hr_ChecklistCreaInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_ChecklistGetOutModel>> Hr_ChecklistPut(GenericRequest<Hr_ChecklistPutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_IdOutModel>> Hr_ChecklistDelete(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_ChecklistVoceOutModel>> Hr_ChecklistVocePut(GenericRequest<Hr_ChecklistVocePutInModel> model, Boolean isSubProcess);
        Task<GenericResult<Hr_ChecklistVoceOutModel>> Hr_ChecklistVoceDocUpload(GenericRequest<Hr_VoceDocUploadModel> model, Stream content, string fileName, string? contentType, long length, Boolean isSubProcess);
        Task<Hr_FileDownload> Hr_ChecklistVoceDocDownload(int id);
        Task<GenericResult<Hr_ChecklistVoceOutModel>> Hr_ChecklistVocePresaVisione(GenericRequest<Hr_IdInModel> model, Boolean isSubProcess);
    }
}
