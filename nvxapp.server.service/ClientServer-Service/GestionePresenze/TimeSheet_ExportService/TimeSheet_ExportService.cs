using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService.Models;
using nvxapp.server.service.ClientServer_Service.infrastructure.MyMokeLongJob.Models;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
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

                                      IGestionePresenzeUserUtility gestionePresenzeUserUtility


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
                        outModel.Messages.Add(new Message("User not identified; cannot start job.", MessageType.Error));
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
                                                                                Message = new Message { Text = "Export presenze avviato...", MsgType = MessageType.Information }
                                                                            }
                                                                         );

                                var req_OrariSchema_4User = new GenericRequest<Timesheet_AllData_InModel>();
                                req_OrariSchema_4User.Data.Dal = model.Data.TimeSheet_Export.Dal;
                                req_OrariSchema_4User.Data.Al = model.Data.TimeSheet_Export.Al;
                                req_OrariSchema_4User.Data.UsersId = model.Data.TimeSheet_Export.SelectedUserId;

                                var AllData_Res = await _timeSheet_EngineService.Get_Timesheet_AllData(req_OrariSchema_4User, true);

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

                                            var exportsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "exports");
                                            Directory.CreateDirectory(exportsFolder);

                                            string fileName;
                                            if (exportCau.TipoFile == Par_Export_TipoFile.CSV)
                                                fileName = await WriteCsvFile(rows, exportCau.Codice ?? "export", jobId.ToString(), exportsFolder);
                                            else
                                                fileName = await WriteTxtFile(rows, exportCau.Codice ?? "export", jobId.ToString(), exportsFolder);

                                            retVal.DownloadUrl = $"TimeSheet_Export/Download/{fileName}";

                                            Log.Information("Export file created for job {JobId}: {FileName}", jobId, fileName);
                                        }
                                    }
                                }

                                await Task.Delay(10000);
                                Log.Information("Background task for job {JobId} has finished successfully.", jobId);
                                await _longJobNotifier.LongJobProgressAsync(userId,
                                                                            new LongJobProgressUpdate
                                                                                {
                                                                                    JobId = jobId.ToString(),
                                                                                    JobType = GestionePresenze_JobType.TimeSheet_Engine_Export,
                                                                                    Payload = new { model.Data.TimeSheet_Export, DownloadUrl = retVal.DownloadUrl },
                                                                                    Category = LongJobCategory.FileGeneration,
                                                                                    ProgressPercentage = 100,
                                                                                    Message = new Message { Text = "Export completato, clicca per scaricare", MsgType = MessageType.Information },
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
                                                                                Message = new Message { Text = $"Export presenze fallito: {ex.Message}", MsgType = MessageType.Exception },
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



        #region Preparazione dati

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
                        Message = new Message { Text = $"Elaborazione {processedUsers}/{totalUsers}...", MsgType = MessageType.Information }
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
    }
}
