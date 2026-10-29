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
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_RicettaService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using System.Globalization;
using System.Text;
using System.Text.Json;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService
{



    public class TimeSheet_ExportService : ServiceBase, ITimeSheet_ExportService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IAz_SediRepartoUserRepository _az_SediRepartoUserRepository;
        private readonly IServiceProvider _serviceProvider;

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
                                      IAz_SediRepartoUserRepository az_SediRepartoUserRepository,
                                      IServiceProvider serviceProvider


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
            _serviceProvider = serviceProvider;

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


        /// <summary>
        /// Export verso le paghe (analisi ANALISI_EXPORT_AI.md, par. 3): job in background.
        /// Metadati JSON dei dipendenti scelti (solo quelli che l'amministratore puo' vedere); se il modello ha documentazione
        /// la ricetta dell'AI (salvata o rigenerata) li scrive nel formato del programma paghe, altrimenti il file e' il JSON.
        /// Se la ricetta non si puo' ottenere (AI non raggiungibile, ricetta non valida) si scarica comunque il JSON, con un avviso.
        /// </summary>
        public virtual async Task<GenericResult<TimeSheet_ExportOutModel>> Export(GenericRequest<TimeSheet_ExportInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TimeSheet_ExportOutModel();
                var richiesta = model.Data.TimeSheet_Export;
                retVal.TimeSheet_Export = richiesta;

                var dipendenti = await DipendentiEsportabili(richiesta.SelectedUserId);
                if (dipendenti.Count == 0)
                {
                    retVal.Messages.Add(new Message(AttendanceTrackingMessages.Get("ExportPdfNoEmployees"), MessageType.Warning));
                    return retVal;
                }

                var userId = this.UserIdFirstConnection;
                if (string.IsNullOrEmpty(userId))
                {
                    retVal.Messages.Add(new Message(AttendanceTrackingMessages.Get("JobUserNotIdentified"), MessageType.Error));
                    return retVal;
                }

                int.TryParse(this.CurrentCompany, out int idCompany);
                var jobId = Guid.NewGuid();
                var payload = new TimeSheet_ExportModel
                {
                    Year = richiesta.Year, Month = richiesta.Month, Dal = richiesta.Dal, Al = richiesta.Al,
                    IdPar_ExportCau = richiesta.IdPar_ExportCau, SelectedUserId = dipendenti
                };

                RunInBackground(async scope =>
                {
                    var longJobNotifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();

                    Task Notifica(int progress, Message message, JobFile? file = null, bool isFinished = false) =>
                        longJobNotifier.LongJobProgressAsync(userId,
                                                             new LongJobProgressUpdate
                                                             {
                                                                 JobId = jobId.ToString(),
                                                                 JobType = GestionePresenze_JobType.TimeSheet_Engine_Export,
                                                                 Payload = payload,
                                                                 Category = LongJobCategory.FileGeneration,
                                                                 DownloadUrl = file?.DownloadUrl,
                                                                 FileName = file?.FileName,
                                                                 ProgressPercentage = progress,
                                                                 Message = message,
                                                                 IsFinished = isFinished
                                                             });

                    try
                    {
                        await Notifica(0, new Message { Text = AttendanceTrackingMessages.Get("ExportStarted"), MsgType = MessageType.Information });

                        var preparati = await PreparaMetadati(scope.ServiceProvider, richiesta.IdPar_ExportCau, richiesta.Dal, richiesta.Al, dipendenti, idCompany);
                        var motivo = preparati.Errore ?? (preparati.Risultato!.Metadati.Dipendenti.Count == 0 ? AttendanceTrackingMessages.Get("ExportNessunDato") : null);
                        if (motivo != null)
                        {
                            Log.Warning("Export job {JobId}: nessun file generato, {Motivo}", jobId, motivo);
                            await Notifica(100, new Message { Text = AttendanceTrackingMessages.Get("ExportNoFileGenerated", motivo), MsgType = MessageType.Warning }, isFinished: true);
                            return;
                        }

                        var metadati = preparati.Risultato!.Metadati;
                        var nomeFile = $"{metadati.ModelloExport.Codice}_{richiesta.Dal:yyyyMMdd}-{richiesta.Al:yyyyMMdd}";
                        var avvisi = new List<string>();
                        JobFile file;

                        var ricettaService = scope.ServiceProvider.GetRequiredService<IPar_ExportCau_RicettaService>();
                        if (!ricettaService.HaDocumentazione(richiesta.IdPar_ExportCau))
                            file = NVXSystem.SalvaFileJob(jobId.ToString(), userId, nomeFile, "json", JsonMetadati(metadati));
                        else
                        {
                            try
                            {
                                await Notifica(60, new Message { Text = AttendanceTrackingMessages.Get("ExportRicettaInCorso"), MsgType = MessageType.Information });
                                var ricetta = (await ricettaService.OttieniRicetta(richiesta.IdPar_ExportCau, false))!;
                                await Notifica(85, new Message { Text = AttendanceTrackingMessages.Get("ExportScritturaFile"), MsgType = MessageType.Information });
                                var (contenuto, estensione) = ExportRicettaRenderer.Scrivi(ricetta, metadati);
                                file = NVXSystem.SalvaFileJob(jobId.ToString(), userId, nomeFile, estensione, contenuto);
                                avvisi.AddRange(ricetta.Avvisi);
                            }
                            catch (Exception ex) when (ex is UserMessageException or LlmBusyException or LlmTimeoutException or InvalidOperationException)
                            {
                                // senza ricetta i dati non si perdono: si scarica il JSON
                                Log.Warning(ex, "Export job {JobId}: ricetta non disponibile, si scarica il JSON", jobId);
                                avvisi.Add(AttendanceTrackingMessages.Get("ExportRicettaNonDisponibile", ex.Message));
                                file = NVXSystem.SalvaFileJob(jobId.ToString(), userId, nomeFile, "json", JsonMetadati(metadati));
                            }
                        }

                        if (preparati.Risultato.Negativi.Count > 0)
                            avvisi.Add(AttendanceTrackingMessages.Get("ExportValoriNegativi", preparati.Risultato.Negativi.Count, string.Join(", ", preparati.Risultato.Negativi.Take(5))));
                        var senzaMatricola = metadati.Dipendenti.Count(d => string.IsNullOrWhiteSpace(d.Anagrafica.Matricola));
                        if (senzaMatricola > 0)
                            avvisi.Add(AttendanceTrackingMessages.Get("ExportSenzaMatricola", senzaMatricola));

                        Log.Information("Export file created for job {JobId}: {FileName}", jobId, file.FileName);
                        await Notifica(100,
                                       avvisi.Count == 0
                                           ? new Message { Text = AttendanceTrackingMessages.Get("ExportCompletedClickToDownload"), MsgType = MessageType.Information }
                                           : new Message { Text = AttendanceTrackingMessages.Get("ExportCompletatoConAvvisi", string.Join(" ", avvisi)), MsgType = MessageType.Warning },
                                       file,
                                       isFinished: true);
                    }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Background task for job {JobId} failed.", jobId);
                        await Notifica(100, new Message { Text = AttendanceTrackingMessages.Get("ExportFailed", ex.Message), MsgType = MessageType.Exception }, isFinished: true);
                    }
                });

                await Task.CompletedTask;
                return retVal;
            }, isSubProcess);
        }

        /// <summary>
        /// Anteprima del file del modello di export su un dipendente e un periodo (pagina del modello, sezione Documentazione):
        /// con documentazione la ricetta (salvata, o richiesta all'AI se cambiata o se <c>Rigenera</c>) applicata ai dati reali,
        /// senza documentazione il JSON dei dati. Non salva file.
        /// </summary>
        public virtual async Task<GenericResult<TimeSheet_Export_Anteprima_OutModel>> Export_Anteprima(GenericRequest<TimeSheet_Export_Anteprima_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TimeSheet_Export_Anteprima_OutModel();
                var d = model.Data;
                var dipendenti = await DipendentiEsportabili(new List<string> { d.IdAspNetUsers ?? string.Empty });
                if (dipendenti.Count == 0)
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ExportPdfNoEmployees"));

                int.TryParse(this.CurrentCompany, out int idCompany);
                var preparati = await PreparaMetadati(_serviceProvider, d.IdPar_ExportCau, d.Dal, d.Al, dipendenti, idCompany);
                if (preparati.Errore != null)
                    throw new UserMessageException(preparati.Errore);
                var metadati = preparati.Risultato!.Metadati;

                var ricettaService = _serviceProvider.GetRequiredService<IPar_ExportCau_RicettaService>();
                ExportRicetta? ricetta;
                try
                {
                    ricetta = await ricettaService.OttieniRicetta(d.IdPar_ExportCau, d.Rigenera);
                }
                catch (Exception ex) when (ex is LlmBusyException or LlmTimeoutException)
                {
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ExportRicettaNonDisponibile", ex.Message), ex);
                }

                if (ricetta == null)
                {
                    retVal.Estensione = "json";
                    retVal.Anteprima = Encoding.UTF8.GetString(JsonMetadati(metadati));
                }
                else
                {
                    retVal.Ricetta = Par_ExportCau_RicettaService.Par_ExportCau_RicettaService.Serializza(ricetta);
                    retVal.Estensione = ricetta.Estensione;
                    retVal.Anteprima = string.Join("\n", ExportRicettaRenderer.Anteprima(ricetta, metadati, 50));
                    retVal.Avvisi.AddRange(ricetta.Avvisi);
                }
                if (metadati.Dipendenti.Count == 0)
                    retVal.Avvisi.Add(AttendanceTrackingMessages.Get("ExportNessunDato"));
                if (preparati.Risultato.Negativi.Count > 0)
                    retVal.Avvisi.Add(AttendanceTrackingMessages.Get("ExportValoriNegativi", preparati.Risultato.Negativi.Count, string.Join(", ", preparati.Risultato.Negativi.Take(5))));
                return retVal;
            }, isSubProcess);
        }

        private static readonly JsonSerializerOptions JsonExport = new()
        {
            WriteIndented = true,
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            Encoder = System.Text.Encodings.Web.JavaScriptEncoder.UnsafeRelaxedJsonEscaping
        };

        private static byte[] JsonMetadati(ExportMetadati metadati) => JsonSerializer.SerializeToUtf8Bytes(metadati, JsonExport);

        /// <summary>
        /// Metadati dell'export per i dipendenti indicati (gia' filtrati), dai servizi dello scope (job o richiesta).
        /// Errore (testo per l'utente) se il modello non c'e', non ha righe o i dati delle presenze non si leggono.
        /// </summary>
        private static async Task<(ExportMetadatiBuilder.Risultato? Risultato, string? Errore)> PreparaMetadati(IServiceProvider sp, int idPar_ExportCau,
                                                                                                                  DateTime dal, DateTime al, List<string> usersId, int idCompany)
        {
            var par_ExportCauService = sp.GetRequiredService<IPar_ExportCauService>();
            var req_Modello = new GenericRequest<Par_ExportCau_Get_InModel>();
            req_Modello.Data.Id = idPar_ExportCau;
            var res_Modello = await par_ExportCauService.Par_ExportCauGet(req_Modello, true);
            var testata = res_Modello.Success ? res_Modello.Data?.Par_ExportCau : null;
            if (testata == null || testata.Id == 0)
                return (null, AttendanceTrackingMessages.Get("ExportLayoutNotFound"));
            var righe = res_Modello.Data!.Par_ExportCau_Causali;
            if (righe.Count == 0)
                return (null, AttendanceTrackingMessages.Get("ExportLayoutWithoutCausali", testata.Descrizione));

            var engine = sp.GetRequiredService<ITimeSheet_EngineService>();
            var req_AllData = new GenericRequest<Timesheet_AllData_InModel>();
            req_AllData.Data.Dal = dal;
            req_AllData.Data.Al = al;
            req_AllData.Data.UsersId = usersId;
            var res_AllData = await engine.Get_Timesheet_AllData(req_AllData, true);
            if (!res_AllData.Success || res_AllData.Data == null)
                return (null, AttendanceTrackingMessages.Get("ExportAttendanceDataUnavailable"));

            var company = await sp.GetRequiredService<ICompanyRepository>().FindByIdAsync(idCompany);
            var utenti = sp.GetRequiredService<UserManager<ApplicationUser>>().Users
                           .Where(u => usersId.Contains(u.Id))
                           .Select(u => new { u.Id, u.Email, u.UserName })
                           .ToList()
                           .ToDictionary(u => u.Id, u => new ExportMetadatiBuilder.DatiUtente(u.Email, u.UserName));

            var schema = res_AllData.Data.OrariSchema_4User_OutModel;
            var risultato = ExportMetadatiBuilder.Costruisci(company?.Descrizione ?? string.Empty, testata.Codice, testata.Descrizione, dal, al,
                                                             schema.Dip_Anagrafica.Where(a => usersId.Contains(a.IdAspNetUsers)),
                                                             schema.Dip_RapportoLavoro,
                                                             res_AllData.Data.Dip_GG_AllData_OutModel.Dip_GG_Causali,
                                                             righe, utenti, DateTime.Now);
            return (risultato, null);
        }

        /// <summary>
        /// Dipendenti richiesti che l'utente puo' esportare: CompanyPowerAdmin tutti quelli dell'azienda, CompanyAdmin quelli
        /// dei reparti che amministra (come i cartellini dell'admin). Gli altri utenti non esportano.
        /// </summary>
        private async Task<List<string>> DipendentiEsportabili(List<string>? richiesti)
        {
            var usersId = (richiesti ?? new List<string>()).Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            if (!await HaRuolo("CompanyPowerAdmin"))
            {
                if (!await HaRuolo("CompanyAdmin"))
                    throw new UserMessageException(AttendanceTrackingMessages.Get("ExportPdfAdminsOnly"));
                var amministrati = DipendentiAmministrati(this.CurrentUserId);
                usersId = usersId.Where(u => amministrati.Contains(u)).ToList();
            }
            return (await _gestionePresenzeUserUtility.GetAnagraficheByUsersId(usersId)).Select(d => d.IdAspNetUsers).Distinct().ToList();
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

                    Task Notifica(int progress, Message message, JobFile? file = null, bool isFinished = false) =>
                        longJobNotifier.LongJobProgressAsync(userId,
                                                             new LongJobProgressUpdate
                                                             {
                                                                 JobId = jobId.ToString(),
                                                                 JobType = GestionePresenze_JobType.TimeSheet_Engine_ExportPdf_4User,
                                                                 Payload = periodo,
                                                                 Category = LongJobCategory.FileGeneration,
                                                                 DownloadUrl = file?.DownloadUrl,
                                                                 FileName = file?.FileName,
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

                        var nome = $"Cartellino_{periodo.Year}-{periodo.Month:00}";
                        var fileName = TimeSheetPdfWriter.Write(new[] { cartellino }, nome, jobId.ToString(), NVXSystem.ExportFolder);
                        Log.Information("Export PDF file created for job {JobId}: {FileName}", jobId, fileName);

                        await Notifica(100,
                                       new Message { Text = AttendanceTrackingMessages.Get("ExportCompletedClickToDownload"), MsgType = MessageType.Information },
                                       NVXSystem.RegistraFileJob(fileName, userId, nome),
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

                    Task Notifica(int progress, Message message, JobFile? file = null, bool isFinished = false) =>
                        longJobNotifier.LongJobProgressAsync(userId,
                                                             new LongJobProgressUpdate
                                                             {
                                                                 JobId = jobId.ToString(),
                                                                 JobType = GestionePresenze_JobType.TimeSheet_Engine_ExportPdf_4Admin,
                                                                 Payload = payload,
                                                                 Category = LongJobCategory.FileGeneration,
                                                                 DownloadUrl = file?.DownloadUrl,
                                                                 FileName = file?.FileName,
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

                        var nome = $"Cartellini_{richiesta.Year}-{richiesta.Month:00}";
                        var fileName = TimeSheetPdfWriter.Write(cartellini, nome, jobId.ToString(), NVXSystem.ExportFolder);
                        Log.Information("Export PDF file created for job {JobId}: {FileName} ({Count} employees)", jobId, fileName, cartellini.Count);

                        await Notifica(100,
                                       new Message { Text = AttendanceTrackingMessages.Get("ExportCompletedClickToDownload"), MsgType = MessageType.Information },
                                       NVXSystem.RegistraFileJob(fileName, userId, nome),
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
                                    // giustificativo a importo: l'importo al posto delle ore
                                    var ore = g.Importo is decimal importo && importo > 0
                                        ? " " + importo.ToString("€ #,##0.00", System.Globalization.CultureInfo.GetCultureInfo("it-IT"))
                                        : g.Hours is TimeSpan h && h > TimeSpan.Zero ? $" {(int)h.TotalHours}:{h.Minutes:00}" : "";
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

        #endregion
    }



    public interface ITimeSheet_ExportService : IServiceBase
    {
        Task<GenericResult<TimeSheet_ExportOutModel>> Export(GenericRequest<TimeSheet_ExportInModel> model, bool isSubProcess);
        Task<GenericResult<TimeSheet_Export_Anteprima_OutModel>> Export_Anteprima(GenericRequest<TimeSheet_Export_Anteprima_InModel> model, bool isSubProcess);
        Task<GenericResult<TimeSheet_ExportPdf_4User_OutModel>> ExportPdf_4User(GenericRequest<TimeSheet_ExportPdf_4User_InModel> model, bool isSubProcess);
        Task<GenericResult<TimeSheet_ExportPdf_4Admin_OutModel>> ExportPdf_4Admin(GenericRequest<TimeSheet_ExportPdf_4Admin_InModel> model, bool isSubProcess);
    }
}
