using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.infrastructure.MyMokeLongJob.Models;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;
/*

 premesa:
  questo calcolo verra esequito:
    1) su richiesta
    2) su alterazione timbrature (timbratura [inserimento/alterazione HR] / approvazione/cancellazione richiesta timbratura [HR/user]  )
    3) su alterazione giustificativo (giust [inserimento/alterazione HR] / approvazione/cancellazione richiesta [HR/user])
    4) su alterazione profilo / orario (FORSE)

 input :
   range Day
   user List


1) individua profilo
   se OK
    
    (dopo aver individuato il profilo posso capire se devo caricare solo le timbrature del giorno o di tutto il periodo [conteggio STRAO/SUPPL])


    2) check timbrature
        se ok
            arrotonda
        else
            genera / completa  (se richiesto)
            arrotonda

    3) calcola ore ord

    4) se =! da ore profilo
       se >
	      se strao/suppl GG
	        calcola strao/suppl
	      else
		    se perido strao/suppl è ok
		    calcola strat/suppl
       else
          calcola assenza
   
    else
      Notifico


 

 
 
 */

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService
{

    public class TimeSheet_EngineService : ServiceBase, ITimeSheet_EngineService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly ILongJobNotifier _longJobNotifier;
        private readonly IDip_ProfiloOrarioService _dip_ProfiloOrarioService;
        private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;
        private readonly IDip_GG_CausaliService _dip_GG_CausaliService;
        private readonly IDip_GG_GiustificativiService _dip_GG_GiustificativiService;
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;

        public TimeSheet_EngineService(IMapper mapper,
                                      UserManager<ApplicationUser> userManager,
                                      IAspNetUsersRepository aspNetUsersRepository,
                                      IOptions<JwtParameter> jwtParameter,
                                      IHttpContextAccessor httpContextAccessor,
                                      IConfiguration configuration,

                                      ILongJobNotifier longJobNotifier,
                                      IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                      IDip_ProfiloOrarioService dip_ProfiloOrarioService,
                                      IDip_GG_TimbraturaService dip_GG_TimbraturaService,
                                      IDip_GG_CausaliService dip_GG_CausaliService,
                                      IDip_GG_GiustificativiService dip_GG_GiustificativiService,
                                      IDip_GG_RichiestaService dip_GG_RichiestaService

                                      ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _longJobNotifier = longJobNotifier;
            _dip_ProfiloOrarioService = dip_ProfiloOrarioService;
            _dip_GG_TimbraturaService = dip_GG_TimbraturaService;
            _dip_GG_CausaliService = dip_GG_CausaliService;
            _dip_GG_GiustificativiService = dip_GG_GiustificativiService;
            _dip_GG_RichiestaService = dip_GG_RichiestaService;
        }


        public virtual async Task<GenericResult<TimeSheet_CalculateOutModel>> Calculate(GenericRequest<TimeSheet_CalculateInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                TimeSheet_CalculateOutModel retVal = new TimeSheet_CalculateOutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var AllData = await  GetAllData(model.Data.TimeSheet_Calculate.SelectedUserId,model.Data.TimeSheet_Calculate.Dal,model.Data.TimeSheet_Calculate.Al);

                    //////////////////////////////////

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
                        Log.Information("Request to start long-running job {JobId} for user {UserId} received.", jobId, userId);

                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                Log.Information("Background task for job {JobId} is starting.", jobId);
                                await _longJobNotifier.LongJobProgressAsync(userId,
                                                                            new LongJobProgressUpdate
                                                                            {
                                                                                JobId = jobId.ToString(),
                                                                                JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                                                                                Payload = model.Data.TimeSheet_Calculate,
                                                                                ProgressPercentage = 0,
                                                                                Message = new Message { Text = "Job is starting...", MsgType = MessageType.Information }
                                                                            }
                                                                           );

                                for (int i = 1; i <= 5; i++)
                                {
                                    await Task.Delay(1000);
                                    int progress = i * 20;
                                    Log.Information("Job {JobId}: Progress step {Step}/5 ({Progress}%)", jobId, i, progress);

                                    await _longJobNotifier.LongJobProgressAsync(userId,
                                                                                new LongJobProgressUpdate
                                                                                {
                                                                                    JobId = jobId.ToString(),
                                                                                    JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                                                                                    Payload = model.Data.TimeSheet_Calculate,
                                                                                    ProgressPercentage = progress,
                                                                                    Message = new Message { Text = $"Processing step {i} of 5...", MsgType = MessageType.Information }
                                                                                }
                                                                                );
                                }

                                Log.Information("Background task for job {JobId} has finished successfully.", jobId);
                                await _longJobNotifier.LongJobProgressAsync(userId,
                                                                            new LongJobProgressUpdate
                                                                            {
                                                                                JobId = jobId.ToString(),
                                                                                JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                                                                                Payload = model.Data.TimeSheet_Calculate,
                                                                                ProgressPercentage = 100,
                                                                                Message = new Message { Text = "Job completed successfully.", MsgType = MessageType.Information },
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
                                                                                JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                                                                                Payload = model.Data.TimeSheet_Calculate,
                                                                                ProgressPercentage = 100,
                                                                                Message = new Message { Text = $"Job failed: {ex.Message}", MsgType = MessageType.Exception },
                                                                                IsFinished = true
                                                                            }
                                                                            );
                            }
                        });

                        outModel.Messages.Add(new Message($"Job started with ID: {outModel.JobId}", MessageType.Information));
                    }

                    //////////////////////////////////
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }



        public class AllData
        {
            public Dip_ProfiloOrario_Get_Profile_4Calculation_OutModel Dip_ProfiloOrario_Calculate { get; set; } = new Dip_ProfiloOrario_Get_Profile_4Calculation_OutModel();
            public List<Dip_GG_TimbraturaModel>     Dip_GG_Timbratura     { get; set; } = new List<Dip_GG_TimbraturaModel>();
            public List<Dip_GG_CausaliModel>         Dip_GG_Causali        { get; set; } = new List<Dip_GG_CausaliModel>();
            public List<Dip_GG_GiustificativiModel>  Dip_GG_Giustificativi { get; set; } = new List<Dip_GG_GiustificativiModel>();
            public List<Dip_GG_RichiestaModel>       Dip_GG_Richiesta      { get; set; } = new List<Dip_GG_RichiestaModel>();
        }

        private async Task<AllData> GetAllData(List<string> UsersId, DateTime Dal, DateTime Al)
        {
            AllData data = new AllData();

            // 1) Recupera profili orario per il calcolo
            var req_ProfHHDip = new GenericRequest<Dip_ProfiloOrario_Get_Profile_4Calculation_InModel>();
            req_ProfHHDip.Data.Dal = Dal;
            req_ProfHHDip.Data.Al = Al;
            req_ProfHHDip.Data.UsersId = UsersId;

            var res_ProfHHDip = await _dip_ProfiloOrarioService.Dip_ProfiloOrario_Get_Profile_4Calculation(req_ProfHHDip, true);
            if (res_ProfHHDip.Success && res_ProfHHDip.Data != null)
            {
                data.Dip_ProfiloOrario_Calculate = res_ProfHHDip.Data;
            }

            // 2) Recupera timbrature per il calcolo
            var req_Timbrature = new GenericRequest<Dip_GG_Timbratura_Get_4Calculation_InModel>();
            req_Timbrature.Data.UsersId = UsersId;
            req_Timbrature.Data.Dal = Dal;
            req_Timbrature.Data.Al = Al;

            var res_Timbrature = await _dip_GG_TimbraturaService.Dip_GG_Timbratura_Get_4Calculation(req_Timbrature, true);
            if (res_Timbrature.Success && res_Timbrature.Data != null)
            {
                data.Dip_GG_Timbratura = res_Timbrature.Data.Dip_GG_Timbratura;
            }

            // 3) Recupera causali per il calcolo
            var req_Causali = new GenericRequest<Dip_GG_Causali_Get_4Calculation_InModel>();
            req_Causali.Data.UsersId = UsersId;
            req_Causali.Data.Dal = Dal;
            req_Causali.Data.Al = Al;

            var res_Causali = await _dip_GG_CausaliService.Dip_GG_Causali_Get_4Calculation(req_Causali, true);
            if (res_Causali.Success && res_Causali.Data != null)
            {
                data.Dip_GG_Causali = res_Causali.Data.Dip_GG_Causali;
            }

            // 4) Recupera giustificativi per il calcolo
            var req_Giustificativi = new GenericRequest<Dip_GG_Giustificativi_Get_4Calculation_InModel>();
            req_Giustificativi.Data.UsersId = UsersId;
            req_Giustificativi.Data.Dal = Dal;
            req_Giustificativi.Data.Al = Al;

            var res_Giustificativi = await _dip_GG_GiustificativiService.Dip_GG_Giustificativi_Get_4Calculation(req_Giustificativi, true);
            if (res_Giustificativi.Success && res_Giustificativi.Data != null)
            {
                data.Dip_GG_Giustificativi = res_Giustificativi.Data.Dip_GG_Giustificativi;
            }

            // 5) Recupera richieste per il calcolo
            var req_Richieste = new GenericRequest<Dip_GG_Richiesta_Get_4Calculation_InModel>();
            req_Richieste.Data.UsersId = UsersId;
            req_Richieste.Data.Dal = Dal;
            req_Richieste.Data.Al = Al;

            var res_Richieste = await _dip_GG_RichiestaService.Dip_GG_Richiesta_Get_4Calculation(req_Richieste, true);
            if (res_Richieste.Success && res_Richieste.Data != null)
            {
                data.Dip_GG_Richiesta = res_Richieste.Data.Dip_GG_Richiesta;
            }

            return data;
        }

    }

    public interface ITimeSheet_EngineService : IServiceBase
    {
        Task<GenericResult<TimeSheet_CalculateOutModel>> Calculate(GenericRequest<TimeSheet_CalculateInModel> model, bool isSubProcess);
    }
}
