using nvxapp.server.data.Localization;
using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_PeriodoService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_PeriodoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService.Models;
using nvxapp.server.service.ClientServer_Service.infrastructure.MyMokeLongJob.Models;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;
using System.Globalization;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService
{



    public class TimeSheet_ExportService : ServiceBase, ITimeSheet_ExportService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IAz_SediRepartoUserRepository _az_SediRepartoUserRepository;
        private ILongJobNotifier? _longJobNotifier;
        private ITimeSheet_EngineService? _timeSheet_EngineService;
        private IPar_ExportCauService? _par_ExportCauService;

        //private readonly IJobNotifier _jobNotifier;


        //private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;
        //private readonly IDip_GG_CausaliService _dip_GG_CausaliService;
        //private readonly IDip_GG_GiustificativiService _dip_GG_GiustificativiService;
        //private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;
        //private readonly IDip_GG_ResultService _dip_GG_ResultService;


        //private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        //private readonly IDip_ProfiloOrarioRepository _dip_ProfiloOrarioRepository;
        //private readonly IPar_OrarioService _par_OrarioService;
        //private readonly IPar_ProfiloOrarioService _par_ProfiloOrarioService;
        //private readonly IDip_RapportoLavoroService _dip_RapportoLavoroService;
        //private readonly IPar_GiustificativiService _par_GiustificativiService;



        public TimeSheet_ExportService(IMapper mapper,
                                      UserManager<ApplicationUser> userManager,
                                      IAspNetUsersRepository aspNetUsersRepository,
                                      IOptions<JwtParameter> jwtParameter,
                                      IHttpContextAccessor httpContextAccessor,
                                      IConfiguration configuration,

                                      IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                      IAz_SediRepartoUserRepository az_SediRepartoUserRepository


                                      //ILongJobNotifier longJobNotifier,
                                      //IJobNotifier jobNotifier,
                                      //ITimeSheet_EngineService timeSheet_EngineService
                                      //IDip_GG_TimbraturaRepository dip_GG_TimbraturaRepository,
                                      //IDip_ProfiloOrarioRepository dip_ProfiloOrarioRepository,
                                      //IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                      //IPar_OrarioService par_OrarioService,
                                      //IPar_ProfiloOrarioService par_ProfiloOrarioService,
                                      //IDip_RapportoLavoroService dip_RapportoLavoroService,

                                      //IDip_GG_TimbraturaService dip_GG_TimbraturaService,
                                      //IDip_GG_CausaliService dip_GG_CausaliService,
                                      //IDip_GG_GiustificativiService dip_GG_GiustificativiService,
                                      //IDip_GG_ResultService dip_GG_ResultService,
                                      //IPar_GiustificativiService par_GiustificativiService,
                                      //IDip_GG_RichiestaService dip_GG_RichiestaService

                                      ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _az_SediRepartoUserRepository = az_SediRepartoUserRepository;

            //_longJobNotifier = longJobNotifier;
            //_jobNotifier = jobNotifier;
            //_timeSheet_EngineService = timeSheet_EngineService;

            //_dip_GG_TimbraturaService = dip_GG_TimbraturaService;
            //_dip_GG_CausaliService = dip_GG_CausaliService;
            //_dip_GG_GiustificativiService = dip_GG_GiustificativiService;
            //_dip_GG_RichiestaService = dip_GG_RichiestaService;
            //_dip_ProfiloOrarioRepository = dip_ProfiloOrarioRepository;
            //_dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            //_gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            //_par_OrarioService = par_OrarioService;
            //_dip_GG_ResultService = dip_GG_ResultService;
            //_par_ProfiloOrarioService = par_ProfiloOrarioService;
            //_dip_RapportoLavoroService = dip_RapportoLavoroService;
            //_par_GiustificativiService = par_GiustificativiService;

        }


        public virtual async Task<GenericResult<TimeSheet_ExportOutModel>> Export(GenericRequest<TimeSheet_ExportInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                TimeSheet_ExportOutModel retVal = new TimeSheet_ExportOutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {

                    var outModel = new MyMokeLongJobOutModel();
                    var jobId = Guid.NewGuid();
                    outModel.JobId = jobId.ToString();

                    var userId = this.UserIdFirstConnection;
                    if (string.IsNullOrEmpty(userId))
                    {
                        Log.Information("Could not find user ID. Unable to send SignalR notifications for job {JobId}.", jobId);
                        outModel.Messages.Add(new Message(AttendanceTrackingMessages.Get("JobUserNotIdentified"), MessageType.Error));
                    }
                    else
                    {
                        RunInBackground(async scope =>
                        {
                            _longJobNotifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();
                            _timeSheet_EngineService = scope.ServiceProvider.GetRequiredService<ITimeSheet_EngineService>();
                            _par_ExportCauService = scope.ServiceProvider.GetRequiredService<IPar_ExportCauService>();

                             

                            try
                            {

                                await _longJobNotifier.LongJobProgressAsync(userId,
                                                                            new LongJobProgressUpdate
                                                                            {
                                                                                JobId = jobId.ToString(),
                                                                                JobType = GestionePresenze_JobType.TimeSheet_Engine_Export,
                                                                                Payload = model.Data.TimeSheet_Export,
                                                                                Category = LongJobCategory.FileGeneration,
                                                                                ProgressPercentage = 0,
                                                                                Message = new Message { Text = AttendanceTrackingMessages.Get("ExportStarted"), MsgType = MessageType.Information }
                                                                            }
                                                                         );

                                var req_AllData_4User = new GenericRequest<Timesheet_AllData_InModel>();
                                req_AllData_4User.Data.Dal = model.Data.TimeSheet_Export.Dal;
                                req_AllData_4User.Data.Al = model.Data.TimeSheet_Export.Al;
                                req_AllData_4User.Data.UsersId = model.Data.TimeSheet_Export.SelectedUserId;

                                string? downloadUrl = null;
                                // perche' non e' stato generato il file (null = file generato)
                                string? motivoSenzaFile = null;
                                var AllData_Res = await _timeSheet_EngineService.Get_Timesheet_AllData(req_AllData_4User, true);

                                if (AllData_Res.Success && AllData_Res.Data != null)
                                {
                                    var req_Par_ExportCau = new GenericRequest<Par_ExportCau_Get_InModel>();
                                    req_Par_ExportCau.Data.Id = model.Data.TimeSheet_Export.IdPar_ExportCau;
                                    
                                    var res_Par_ExportCau = await _par_ExportCauService.Par_ExportCauGet(req_Par_ExportCau, true);
                                    if (res_Par_ExportCau.Success && res_Par_ExportCau.Data != null)
                                    {
                                        var exportCau = res_Par_ExportCau.Data.Par_ExportCau;
                                        var exportCausali = res_Par_ExportCau.Data.Par_ExportCau_Causali;

                                        if (exportCau != null && exportCausali != null && exportCausali.Count > 0)
                                        {
                                            var allData = AllData_Res.Data;

                                            var rows = await PrepareExportRows(
                                                allData, exportCausali,
                                                userId, jobId.ToString(), model.Data.TimeSheet_Export);

                                            var exportsFolder = NVXSystem.ExportFolder;
                                            

                                            string fileName;
                                            if (exportCau.TipoFile == Par_Export_TipoFile.CSV)
                                                fileName = await WriteCsvFile(rows, exportCau.Codice ?? "export", jobId.ToString(), exportsFolder);
                                            else
                                                fileName = await WriteTxtFile(rows, exportCau.Codice ?? "export", jobId.ToString(), exportsFolder);

                                            downloadUrl = $"{NVXSystem.DownloadURL}{fileName}";

                                            

                                            Log.Information("Export file created for job {JobId}: {FileName}", jobId, fileName);
                                        }
                                        else if (exportCau == null)
                                            motivoSenzaFile = AttendanceTrackingMessages.Get("ExportLayoutNotFound");
                                        else
                                            motivoSenzaFile = AttendanceTrackingMessages.Get("ExportLayoutWithoutCausali", exportCau.Descrizione);
                                    }
                                    else
                                        motivoSenzaFile = AttendanceTrackingMessages.Get("ExportLayoutNotFound");
                                }
                                else
                                    motivoSenzaFile = AttendanceTrackingMessages.Get("ExportAttendanceDataUnavailable");

                                if (motivoSenzaFile != null)
                                    Log.Warning("Export job {JobId}: nessun file generato, {Motivo}", jobId, motivoSenzaFile);

                                await Task.Delay(10000);
                                Log.Information("Background task for job {JobId} has finished successfully.", jobId);
                                await _longJobNotifier.LongJobProgressAsync(userId,
                                                                            new LongJobProgressUpdate
                                                                                {
                                                                                    JobId = jobId.ToString(),
                                                                                    JobType = GestionePresenze_JobType.TimeSheet_Engine_Export,
                                                                                    Payload = model.Data.TimeSheet_Export,
                                                                                    Category = LongJobCategory.FileGeneration,
                                                                                    DownloadUrl = downloadUrl,
                                                                                    ProgressPercentage = 100,
                                                                                    Message = motivoSenzaFile == null
                                                                                        ? new Message { Text = AttendanceTrackingMessages.Get("ExportCompletedClickToDownload"), MsgType = MessageType.Information }
                                                                                        : new Message { Text = AttendanceTrackingMessages.Get("ExportNoFileGenerated", motivoSenzaFile), MsgType = MessageType.Warning },
                                                                                    IsFinished = true
                                                                                }
                                                                            );


                            }
                            catch (Exception ex)
                            {
                                  Log.Error(ex, "Background task for job {JobId} failed.", jobId);
                                  await _longJobNotifier.LongJobProgressAsync(userId,
                                                                            new LongJobProgressUpdate
                                                                            {
                                                                                JobId = jobId.ToString(),
                                                                                JobType = GestionePresenze_JobType.TimeSheet_Engine_Export,
                                                                                Payload = model.Data.TimeSheet_Export,
                                                                                ProgressPercentage = 100,
                                                                                Category = LongJobCategory.FileGeneration,
                                                                                Message = new Message { Text = AttendanceTrackingMessages.Get("ExportFailed", ex.Message), MsgType = MessageType.Exception },
                                                                                IsFinished = true
                                                                            }
                                                                            );
                            }

                        });
                    }



                }

                //eliminare
                // Nessun 'await' qui
                //await Task.Delay(DelayAsyncMethod);
                await Task.Delay(0);

                return retVal;
            }, isSubProcess);
        }


        /// <summary>
        /// Cartellino del mese in PDF, solo per l'utente collegato (job in background, file scaricabile a fine job).
        /// L'utente non arriva dal client: si stampa sempre il proprio cartellino. L'export dell'admin sara' un metodo a parte.
        /// </summary>
        public virtual async Task<GenericResult<TimeSheet_ExportPdf_4User_OutModel>> ExportPdf_4User(GenericRequest<TimeSheet_ExportPdf_4User_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TimeSheet_ExportPdf_4User_OutModel();
                var periodo = model.Data.TimeSheet_ExportPdf;

                if (periodo.Month < 1 || periodo.Month > 12 || periodo.Year < 2000 || periodo.Year > 2100)
                {
                    retVal.Messages.Add(new Message(AttendanceTrackingMessages.Get("ExportPdfInvalidPeriod", periodo.Month, periodo.Year), MessageType.Error));
                    return retVal;
                }

                var jobId = Guid.NewGuid();
                retVal.JobId = jobId.ToString();

                // notifiche a chi ha fatto il login, dati dell'utente corrente (anche se impersonato)
                var userId = this.UserIdFirstConnection;
                var currentUserId = this.CurrentUserId;
                if (string.IsNullOrEmpty(userId) || string.IsNullOrEmpty(currentUserId))
                {
                    Log.Information("Could not find user ID. Unable to send SignalR notifications for job {JobId}.", jobId);
                    retVal.Messages.Add(new Message(AttendanceTrackingMessages.Get("JobUserNotIdentified"), MessageType.Error));
                    return retVal;
                }

                int.TryParse(this.CurrentCompany, out int idCompany);

                RunInBackground(async scope =>
                {
                    var longJobNotifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();
                    var timeSheet_EngineService = scope.ServiceProvider.GetRequiredService<ITimeSheet_EngineService>();
                    var dip_PeriodoService = scope.ServiceProvider.GetRequiredService<IDip_PeriodoService>();
                    var companyRepository = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();

                    Task Notifica(int progress, Message message, string? downloadUrl = null, bool isFinished = false) =>
                        longJobNotifier.LongJobProgressAsync(userId,
                                                             new LongJobProgressUpdate
                                                             {
                                                                 JobId = jobId.ToString(),
                                                                 JobType = GestionePresenze_JobType.TimeSheet_Engine_ExportPdf_4User,
                                                                 Payload = periodo,
                                                                 Category = LongJobCategory.FileGeneration,
                                                                 DownloadUrl = downloadUrl,
                                                                 ProgressPercentage = progress,
                                                                 Message = message,
                                                                 IsFinished = isFinished
                                                             });

                    try
                    {
                        await Notifica(0, new Message { Text = AttendanceTrackingMessages.Get("ExportPdfStarted"), MsgType = MessageType.Information });

                        var company = await companyRepository.FindByIdAsync(idCompany);
                        var cartellino = await PrepareCartellinoPdf(timeSheet_EngineService, dip_PeriodoService, currentUserId, periodo.Year, periodo.Month);
                        cartellino.Azienda = company?.Descrizione ?? string.Empty;

                        await Notifica(60, new Message { Text = AttendanceTrackingMessages.Get("ExportPdfWriting"), MsgType = MessageType.Information });

                        var fileName = TimeSheetPdfWriter.Write(new[] { cartellino }, $"Cartellino_{periodo.Year}-{periodo.Month:00}", jobId.ToString(), NVXSystem.ExportFolder);
                        Log.Information("Export PDF file created for job {JobId}: {FileName}", jobId, fileName);

                        await Notifica(100,
                                       new Message { Text = AttendanceTrackingMessages.Get("ExportCompletedClickToDownload"), MsgType = MessageType.Information },
                                       $"{NVXSystem.DownloadURL}{fileName}",
                                       isFinished: true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Background task for job {JobId} failed.", jobId);
                        await Notifica(100, new Message { Text = AttendanceTrackingMessages.Get("ExportFailed", ex.Message), MsgType = MessageType.Exception }, isFinished: true);
                    }
                });

                await Task.Delay(0);
                return retVal;
            }, isSubProcess);
        }


        /// <summary>
        /// Cartellini del mese in un unico PDF (un dipendente per sezione) per i dipendenti scelti dall'admin.
        /// Il CompanyPowerAdmin puo' stampare tutti i dipendenti dell'azienda, il CompanyAdmin solo quelli dei reparti che amministra:
        /// gli altri id ricevuti dal client vengono scartati.
        /// </summary>
        public virtual async Task<GenericResult<TimeSheet_ExportPdf_4Admin_OutModel>> ExportPdf_4Admin(GenericRequest<TimeSheet_ExportPdf_4Admin_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TimeSheet_ExportPdf_4Admin_OutModel();
                var richiesta = model.Data.TimeSheet_ExportPdf;

                if (richiesta.Month < 1 || richiesta.Month > 12 || richiesta.Year < 2000 || richiesta.Year > 2100)
                {
                    retVal.Messages.Add(new Message(AttendanceTrackingMessages.Get("ExportPdfInvalidPeriod", richiesta.Month, richiesta.Year), MessageType.Error));
                    return retVal;
                }

                // dipendenti richiesti, limitati a quelli che l'admin puo' vedere
                var usersId = (richiesta.SelectedUserId ?? new List<string>()).Distinct().ToList();
                if (!await HaRuolo("CompanyPowerAdmin"))
                {
                    if (!await HaRuolo("CompanyAdmin"))
                        throw new UserMessageException(AttendanceTrackingMessages.Get("ExportPdfAdminsOnly"));
                    var dipendentiAmministrati = DipendentiAmministrati(this.CurrentUserId);
                    usersId = usersId.Where(u => dipendentiAmministrati.Contains(u)).ToList();
                }

                // solo i dipendenti dell'azienda, in ordine alfabetico come nel PDF
                var dipendenti = (await _gestionePresenzeUserUtility.GetAnagraficheByUsersId(usersId))
                                     .OrderBy(d => d.Cognome).ThenBy(d => d.Nome)
                                     .Select(d => d.IdAspNetUsers)
                                     .Distinct()
                                     .ToList();
                if (dipendenti.Count == 0)
                {
                    retVal.Messages.Add(new Message(AttendanceTrackingMessages.Get("ExportPdfNoEmployees"), MessageType.Warning));
                    return retVal;
                }

                var jobId = Guid.NewGuid();
                retVal.JobId = jobId.ToString();

                var userId = this.UserIdFirstConnection;
                if (string.IsNullOrEmpty(userId))
                {
                    Log.Information("Could not find user ID. Unable to send SignalR notifications for job {JobId}.", jobId);
                    retVal.Messages.Add(new Message(AttendanceTrackingMessages.Get("JobUserNotIdentified"), MessageType.Error));
                    return retVal;
                }

                int.TryParse(this.CurrentCompany, out int idCompany);
                var payload = new TimeSheet_ExportPdf_4Admin_Model { Year = richiesta.Year, Month = richiesta.Month, SelectedUserId = dipendenti };

                RunInBackground(async scope =>
                {
                    var longJobNotifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();
                    var timeSheet_EngineService = scope.ServiceProvider.GetRequiredService<ITimeSheet_EngineService>();
                    var dip_PeriodoService = scope.ServiceProvider.GetRequiredService<IDip_PeriodoService>();
                    var companyRepository = scope.ServiceProvider.GetRequiredService<ICompanyRepository>();

                    Task Notifica(int progress, Message message, string? downloadUrl = null, bool isFinished = false) =>
                        longJobNotifier.LongJobProgressAsync(userId,
                                                             new LongJobProgressUpdate
                                                             {
                                                                 JobId = jobId.ToString(),
                                                                 JobType = GestionePresenze_JobType.TimeSheet_Engine_ExportPdf_4Admin,
                                                                 Payload = payload,
                                                                 Category = LongJobCategory.FileGeneration,
                                                                 DownloadUrl = downloadUrl,
                                                                 ProgressPercentage = progress,
                                                                 Message = message,
                                                                 IsFinished = isFinished
                                                             });

                    try
                    {
                        await Notifica(0, new Message { Text = AttendanceTrackingMessages.Get("ExportPdfStarted"), MsgType = MessageType.Information });

                        var company = await companyRepository.FindByIdAsync(idCompany);
                        var cartellini = new List<CartellinoPdf>();

                        for (int i = 0; i < dipendenti.Count; i++)
                        {
                            var cartellino = await PrepareCartellinoPdf(timeSheet_EngineService, dip_PeriodoService, dipendenti[i], richiesta.Year, richiesta.Month);
                            cartellino.Azienda = company?.Descrizione ?? string.Empty;
                            cartellini.Add(cartellino);

                            // la lettura dei dati e' quasi tutto il lavoro: fino al 90%, il resto e' la scrittura del PDF
                            await Notifica((i + 1) * 90 / dipendenti.Count,
                                           new Message { Text = AttendanceTrackingMessages.Get("ExportProcessingProgress", i + 1, dipendenti.Count), MsgType = MessageType.Information });
                        }

                        await Notifica(90, new Message { Text = AttendanceTrackingMessages.Get("ExportPdfWriting"), MsgType = MessageType.Information });

                        var fileName = TimeSheetPdfWriter.Write(cartellini, $"Cartellini_{richiesta.Year}-{richiesta.Month:00}", jobId.ToString(), NVXSystem.ExportFolder);
                        Log.Information("Export PDF file created for job {JobId}: {FileName} ({Count} employees)", jobId, fileName, cartellini.Count);

                        await Notifica(100,
                                       new Message { Text = AttendanceTrackingMessages.Get("ExportCompletedClickToDownload"), MsgType = MessageType.Information },
                                       $"{NVXSystem.DownloadURL}{fileName}",
                                       isFinished: true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Background task for job {JobId} failed.", jobId);
                        await Notifica(100, new Message { Text = AttendanceTrackingMessages.Get("ExportFailed", ex.Message), MsgType = MessageType.Exception }, isFinished: true);
                    }
                });

                return retVal;
            }, isSubProcess);
        }

        // dipendenti dei reparti in cui l'utente e' amministratore (come Dip_GG_NotaSpesaService)
        private HashSet<string> DipendentiAmministrati(string idAspNetUsers)
        {
            var reparti = _az_SediRepartoUserRepository.FindAll(x => x.IdAspNetUsers == idAspNetUsers && x.EnabledToAdmin)
                                                       .Select(x => x.IdAz_SediReparto).ToList();
            return _az_SediRepartoUserRepository.FindAll(x => reparti.Contains(x.IdAz_SediReparto) && x.UserInDepartment)
                                                .Select(x => x.IdAspNetUsers).ToHashSet();
        }

        private async Task<bool> HaRuolo(string ruolo)
        {
            var user = await _userManager.FindByIdAsync(this.CurrentUserId);
            return user != null && await _userManager.IsInRoleAsync(user, ruolo);
        }



        #region Preparazione dati

        /// <summary>
        /// Cartellino di un dipendente per un mese: un giorno per riga (come il calendario) e il riepilogo
        /// del mese (come il componente cartellino-quadratura). Riusabile dall'export dell'admin, un dipendente alla volta.
        /// </summary>
        private static async Task<CartellinoPdf> PrepareCartellinoPdf(ITimeSheet_EngineService timeSheet_EngineService,
                                                                      IDip_PeriodoService dip_PeriodoService,
                                                                      string idAspNetUsers, int anno, int mese)
        {
            var dal = new DateTime(anno, mese, 1);
            var al = dal.AddMonths(1).AddDays(-1);

            var req_AllData = new GenericRequest<Timesheet_AllData_InModel>();
            req_AllData.Data.Dal = dal;
            req_AllData.Data.Al = al;
            req_AllData.Data.UsersId = new List<string> { idAspNetUsers };

            var res_AllData = await timeSheet_EngineService.Get_Timesheet_AllData(req_AllData, true);
            if (!res_AllData.Success || res_AllData.Data == null)
                throw new UserMessageException(AttendanceTrackingMessages.Get("ExportAttendanceDataUnavailable"));

            var schema = res_AllData.Data.OrariSchema_4User_OutModel;
            var dati = res_AllData.Data.Dip_GG_AllData_OutModel;

            var cartellino = new CartellinoPdf { Anno = anno, Mese = mese };

            var dip = schema.Dip_Anagrafica.FirstOrDefault(d => d.IdAspNetUsers == idAspNetUsers) ?? schema.Dip_Anagrafica.FirstOrDefault();
            if (dip != null)
                cartellino.Dipendente = $"{dip.Cognome} {dip.Nome}".Trim();

            var giustificativi = schema.Par_Giustificativi.ToDictionary(g => g.Id);

            // le richieste cancellate o rifiutate non contano; quelle non ancora approvate hanno il segno
            static bool Valida(StatoRichiesta stato) => stato != StatoRichiesta.Cancellata && stato != StatoRichiesta.Rifiutata;
            static string SegnoStato(StatoRichiesta stato) =>
                stato == StatoRichiesta.Immessa || stato == StatoRichiesta.ApprovazioneInCorso || stato == StatoRichiesta.ParzialmenteApprovata
                    ? TimeSheetPdfWriter.SegnoInAttesa : "";

            for (var giorno = dal; giorno <= al; giorno = giorno.AddDays(1))
            {
                var result = dati.Dip_GG_Result.FirstOrDefault(r => r.Data.Date == giorno);

                var timbrature = dati.Dip_GG_Timbratura
                                     .Where(t => t.Timbratura.Date == giorno && Valida(t.RichiestaStato))
                                     .OrderBy(t => t.Timbratura)
                                     .Select(t => $"{SiglaTimbratura(t.TimbraturaTipo)} {t.Timbratura:HH:mm}{SegnoStato(t.RichiestaStato)}");

                var giust = dati.Dip_GG_Giustificativi
                                .Where(g => g.Data.Date == giorno && Valida(g.RichiestaStato))
                                .Select(g =>
                                {
                                    var codice = giustificativi.TryGetValue(g.IdPar_Giustificativi, out var par) ? (par.Codice ?? par.Descrizione ?? "") : "";
                                    var ore = g.Hours is TimeSpan h && h > TimeSpan.Zero ? $" {(int)h.TotalHours}:{h.Minutes:00}" : "";
                                    return $"{codice}{ore}{SegnoStato(g.RichiestaStato)}";
                                });

                var riga = new CartellinoPdf_Giorno
                {
                    Data = giorno,
                    Festivo = (result?.Festivo ?? false) || giorno.DayOfWeek == DayOfWeek.Saturday || giorno.DayOfWeek == DayOfWeek.Sunday,
                    Timbrature = string.Join("  ", timbrature),
                    Giustificativi = string.Join("  ", giust),
                };

                if (result != null)
                {
                    riga.Min_Teo = (int)result.HH_Teo.ToTimeSpan().TotalMinutes;
                    riga.Min_Lav = (int)result.HH_Lav.ToTimeSpan().TotalMinutes;
                    (riga.Min_InPiu, riga.InPiuProvvisorie) = EccedenzaGiorno(result);
                }

                cartellino.Giorni.Add(riga);
            }

            var req_Periodo = new GenericRequest<Dip_Periodo_Get_InModel>();
            req_Periodo.Data.IdAspNetUsers = idAspNetUsers;
            req_Periodo.Data.Anno = anno;
            req_Periodo.Data.Mese = mese;

            var res_Periodo = await dip_PeriodoService.Dip_Periodo_Get(req_Periodo, true);
            if (res_Periodo.Success && res_Periodo.Data?.Dip_Periodo != null)
            {
                cartellino.StatoPeriodo = res_Periodo.Data.Dip_Periodo.Stato switch
                {
                    PeriodoStato.Approvato => AttendanceTrackingMessages.Get("CartellinoPdf_StatoApprovato"),
                    PeriodoStato.DaApprovare => AttendanceTrackingMessages.Get("CartellinoPdf_StatoDaApprovare"),
                    _ => AttendanceTrackingMessages.Get("CartellinoPdf_StatoProvvisorio")
                };
                cartellino.NoteHR = res_Periodo.Data.Dip_Periodo.Note;
                cartellino.Riepilogo = VociRiepilogo(res_Periodo.Data);
            }

            return cartellino;
        }

        private static string SiglaTimbratura(TipoTimbratura tipo) => tipo switch
        {
            TipoTimbratura.Entrata => AttendanceTrackingMessages.Get("CartellinoPdf_Entrata"),
            TipoTimbratura.Uscita => AttendanceTrackingMessages.Get("CartellinoPdf_Uscita"),
            _ => AttendanceTrackingMessages.Get("CartellinoPdf_SenzaVerso")
        };

        /// <summary>Ore in piu' (o mancanti) del giorno, con la stessa regola del calendario (eccedenzaGiorno del client).</summary>
        private static (int? minuti, bool provvisorie) EccedenzaGiorno(Dip_GG_ResultModel r)
        {
            var nonAutorizzate = r.Min_NonAutorizzate;
            if (r.Min_Ecc == 0 && nonAutorizzate == 0)
                return (null, false);
            if (((long)r.Stato & ~(long)GG_ResultStato.Init) == 0)
                return (null, false);

            var totale = r.Min_Ecc > 0 ? r.Min_Ecc + nonAutorizzate : (r.Min_Ecc < 0 ? r.Min_Ecc : nonAutorizzate);
            var provvisorie = (r.Min_Ecc > 0 && r.EccProvvisoria) || r.Min_DaAutorizzare > 0;
            return (totale, provvisorie);
        }

        /// <summary>Voci del riepilogo del mese, nello stesso ordine e con le stesse condizioni del componente cartellino-quadratura.</summary>
        private static List<CartellinoPdf_Voce> VociRiepilogo(Dip_Periodo_Get_OutModel dati)
        {
            var p = dati.Dip_Periodo!;
            var partTime = dati.TipoContratto == TipoContratto.PartTime;
            static string T(string key, params object?[] args) => AttendanceTrackingMessages.Get("CartellinoPdf_Voce_" + key, args);

            var voci = new List<CartellinoPdf_Voce>
            {
                new() { Etichetta = T("OreTeoriche"), Minuti = p.Min_Teo, Nota = partTime && dati.OreSettimanaliContratto != null ? T("NotaPartTime", dati.OreSettimanaliContratto) : null },
                new() { Etichetta = T("OreLavorate"), Minuti = p.Min_Lav, Nota = T("NotaTimbratureNette") },
                new() { Etichetta = T("DiCuiOrdinarie"), Minuti = p.Min_Ord, Livello1 = true, Nota = T("NotaPagaBase") },
                new() { Etichetta = T("Assenze"), Minuti = p.Min_Assenze, Nota = T("NotaSottratti") },
                new() { Etichetta = T("OreInPiu"), Minuti = p.Min_Ecc, Nota = T("NotaOltreTeorico") },
            };

            if (p.Min_Provvisorie > 0)
                voci.Add(new() { Etichetta = T("InAttesaQuadratura"), Minuti = p.Min_Provvisorie, Livello1 = true, Nota = T("NotaPeriodoInCorso") });
            if (p.Min_Compensate > 0)
                voci.Add(new() { Etichetta = T("Compensate"), Minuti = p.Min_Compensate, Livello1 = true, Nota = T("NotaCompensate") });
            if (partTime || p.Min_Suppl + p.Min_SupplEcc > 0)
            {
                voci.Add(new() { Etichetta = T("Supplementare"), Minuti = p.Min_Suppl, Livello1 = true, Nota = T("NotaSupplementare") });
                if (p.Min_SupplEcc > 0)
                    voci.Add(new() { Etichetta = T("SupplementareOltre"), Minuti = p.Min_SupplEcc, Livello1 = true });
            }
            voci.Add(new() { Etichetta = T("Straordinario"), Minuti = p.Min_Strao, Livello1 = true, Nota = T("NotaStraordinario") });
            if (p.Min_StraoNott > 0)
                voci.Add(new() { Etichetta = T("StraoNotturno"), Minuti = p.Min_StraoNott, Livello1 = true });
            if (p.Min_StraoFest > 0)
                voci.Add(new() { Etichetta = T("StraoFestivo"), Minuti = p.Min_StraoFest, Livello1 = true });
            if (p.Min_StraoFestNott > 0)
                voci.Add(new() { Etichetta = T("StraoFestivoNotturno"), Minuti = p.Min_StraoFestNott, Livello1 = true });
            if (p.Min_EccOrd > 0)
                voci.Add(new() { Etichetta = T("EccedentiNonStrao"), Minuti = p.Min_EccOrd, Livello1 = true, Nota = T("NotaEccedenti") });
            if (p.Min_NonAutorizzate > 0)
            {
                voci.Add(new() { Etichetta = T("NonAutorizzate"), Minuti = p.Min_NonAutorizzate, Nota = T("NotaNonAutorizzate") });
                if (p.Min_DaAutorizzare > 0)
                    voci.Add(new() { Etichetta = T("InAttesaAutorizzazione"), Minuti = p.Min_DaAutorizzare, Livello1 = true, Nota = T("NotaInAttesaAutorizzazione") });
            }
            if (p.Min_NonRiconosciute > 0)
                voci.Add(new() { Etichetta = T("NonRiconosciute"), Minuti = p.Min_NonRiconosciute, Livello1 = true, Nota = T("NotaNonRiconosciute") });

            voci.Add(new() { Etichetta = T("Saldo"), Minuti = dati.Min_Saldo, Saldo = true, Nota = T("NotaSaldo") });

            return voci;
        }

        private async Task<List<ExportRow>> PrepareExportRows(
            Timesheet_AllData_OutModel allData,
            List<Par_ExportCau_CausaliModel> exportCausali,
            string userId, string jobId, TimeSheet_ExportModel exportPayload)
        {
            var rows = new List<ExportRow>();

            var dipAnagrafiche = allData.OrariSchema_4User_OutModel.Dip_Anagrafica;
            var rapportiLavoro = allData.OrariSchema_4User_OutModel.Dip_RapportoLavoro;
            var causaliGG = allData.Dip_GG_AllData_OutModel.Dip_GG_Causali;

            int totalUsers = dipAnagrafiche.Count;
            int processedUsers = 0;

            foreach (var dip in dipAnagrafiche)
            {
                var rapportiIds = rapportiLavoro
                    .Where(r => r.IdDip_Anagrafica == dip.Id)
                    .Select(r => r.Id)
                    .ToHashSet();

                foreach (var exportCausale in exportCausali)
                {
                    var causaliUtente = causaliGG
                        .Where(c => rapportiIds.Contains(c.IdDip_RapportoLavoro) && c.IdPar_Causali == exportCausale.IdCausale)
                        .ToList();

                    if (exportCausale.TipoElaborazione == Par_Export_TipoElaborazione.Gionaliera)
                    {
                        foreach (var causale in causaliUtente)
                        {
                            rows.Add(new ExportRow
                            {
                                Cognome = dip.Cognome ?? "",
                                Nome = dip.Nome ?? "",
                                Data = causale.Data.ToString("yyyy-MM-dd"),
                                CodiceCausale = exportCausale.Codice ?? "",
                                Valore = ConvertValore(causale.Valore, exportCausale.TipoUnita)
                            });
                        }
                    }
                    else // Mensile
                    {
                        var grouped = causaliUtente
                            .GroupBy(c => new { c.Data.Year, c.Data.Month });

                        foreach (var grp in grouped)
                        {
                            rows.Add(new ExportRow
                            {
                                Cognome = dip.Cognome ?? "",
                                Nome = dip.Nome ?? "",
                                Data = new DateTime(grp.Key.Year, grp.Key.Month, 1).ToString("yyyy-MM"),
                                CodiceCausale = exportCausale.Codice ?? "",
                                Valore = grp.Sum(c => ConvertValore(c.Valore, exportCausale.TipoUnita))
                            });
                        }
                    }
                }

                processedUsers++;
                var progress = (int)((processedUsers / (double)dipAnagrafiche.Count) * 100);

                await _longJobNotifier!.LongJobProgressAsync(userId,
                    new LongJobProgressUpdate
                    {
                        JobId = jobId,
                        JobType = GestionePresenze_JobType.TimeSheet_Engine_Export,
                        Payload = exportPayload,
                        Category = LongJobCategory.FileGeneration,
                        ProgressPercentage = progress,
                        Message = new Message { Text = AttendanceTrackingMessages.Get("ExportProcessingProgress", processedUsers, totalUsers), MsgType = MessageType.Information }
                    });
            }

            return rows;
        }

        private static decimal ConvertValore(TimeOnly valore, Par_Export_TipoUnita tipoUnita)
        {
            return tipoUnita switch
            {
                Par_Export_TipoUnita.Ore => (decimal)valore.Hour + (decimal)valore.Minute / 60m,
                Par_Export_TipoUnita.Giorni => (valore.Hour > 0 || valore.Minute > 0) ? 1m : 0m,
                Par_Export_TipoUnita.Importo => (decimal)valore.Hour + (decimal)valore.Minute / 60m,
                _ => (decimal)valore.Hour + (decimal)valore.Minute / 60m
            };
        }

        #endregion

        #region Generazione file

        private static async Task<string> WriteCsvFile(List<ExportRow> rows, string codice, string jobId, string folder)
        {
            const string separator = ";";
            var sb = new StringBuilder();

            sb.AppendLine(string.Join(separator, "Cognome", "Nome", "Data", "CodiceCausale", "Valore"));

            foreach (var row in rows)
            {
                sb.AppendLine(string.Join(separator,
                    row.Cognome,
                    row.Nome,
                    row.Data,
                    row.CodiceCausale,
                    row.Valore.ToString(CultureInfo.InvariantCulture)));
            }

            var fileName = $"{codice}_{jobId}.csv";
            var filePath = Path.Combine(folder, fileName);
            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
            return fileName;
        }

        private static async Task<string> WriteTxtFile(List<ExportRow> rows, string codice, string jobId, string folder)
        {
            const string separator = "\t";
            var sb = new StringBuilder();

            sb.AppendLine(string.Join(separator, "Cognome", "Nome", "Data", "CodiceCausale", "Valore"));

            foreach (var row in rows)
            {
                sb.AppendLine(string.Join(separator,
                    row.Cognome,
                    row.Nome,
                    row.Data,
                    row.CodiceCausale,
                    row.Valore.ToString(CultureInfo.InvariantCulture)));
            }

            var fileName = $"{codice}_{jobId}.txt";
            var filePath = Path.Combine(folder, fileName);
            await File.WriteAllTextAsync(filePath, sb.ToString(), Encoding.UTF8);
            return fileName;
        }

        #endregion
    }



    public interface ITimeSheet_ExportService : IServiceBase
    {
        Task<GenericResult<TimeSheet_ExportOutModel>> Export(GenericRequest<TimeSheet_ExportInModel> model, bool isSubProcess);
        Task<GenericResult<TimeSheet_ExportPdf_4User_OutModel>> ExportPdf_4User(GenericRequest<TimeSheet_ExportPdf_4User_InModel> model, bool isSubProcess);
        Task<GenericResult<TimeSheet_ExportPdf_4Admin_OutModel>> ExportPdf_4Admin(GenericRequest<TimeSheet_ExportPdf_4Admin_InModel> model, bool isSubProcess);
    }
}
