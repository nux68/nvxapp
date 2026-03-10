using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
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
        //private readonly IPar_OrarioRepository _par_OrarioRepository;
        //private readonly IPar_OrarioIntervalloHHRepository _par_OrarioIntervalloHHRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly ILongJobNotifier _longJobNotifier;

        public TimeSheet_EngineService(IMapper mapper,
                                      UserManager<ApplicationUser> userManager,
                                      IAspNetUsersRepository aspNetUsersRepository,
                                      IOptions<JwtParameter> jwtParameter,
                                      IHttpContextAccessor httpContextAccessor,
                                      IConfiguration configuration,

                                      ILongJobNotifier longJobNotifier,
                                      IGestionePresenzeUserUtility gestionePresenzeUserUtility
                                      //IPar_OrarioRepository par_OrarioRepository,
                                      //IPar_OrarioIntervalloHHRepository par_OrarioIntervalloHHRepository
                                      ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _longJobNotifier = longJobNotifier;
            //_par_OrarioIntervalloHHRepository = par_OrarioIntervalloHHRepository;
            //_par_OrarioRepository = par_OrarioRepository;
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

                    //////////////////////////////////

                    var outModel = new MyMokeLongJobOutModel();
                    var jobId = Guid.NewGuid();
                    outModel.JobId = jobId.ToString();

                    // Get the current user's ID to send targeted SignalR notifications
                    var userId = this.UserIdFirstConnection;

                    if (string.IsNullOrEmpty(userId))
                    {
                        Log.Information("Could not find user ID. Unable to send SignalR notifications for job {JobId}.", jobId);
                        outModel.Messages.Add(new Message("User not identified; cannot start job.", MessageType.Error));
                    }
                    else
                    {
                        Log.Information("Request to start long-running job {JobId} for user {UserId} received.", jobId, userId);

                        // Fire and forget the background task
                        _ = Task.Run(async () =>
                        {
                            try
                            {
                                Log.Information("Background task for job {JobId} is starting.", jobId);
                                await _longJobNotifier.LongJobProgressAsync(userId,
                                                                            new LongJobProgressUpdate
                                                                            {
                                                                                JobId = jobId.ToString(),
                                                                                ProgressPercentage = 0,
                                                                                Message = new Message { Text = "Job is starting...", MsgType = MessageType.Information }
                                                                            }
                                                                           );

                                // Simulate a long process with 5 steps
                                for (int i = 1; i <= 5; i++)
                                {
                                    await Task.Delay(1000); // 3-second delay for each step
                                    int progress = i * 20;
                                    Log.Information("Job {JobId}: Progress step {Step}/5 ({Progress}%)", jobId, i, progress);

                                    await _longJobNotifier.LongJobProgressAsync(userId,
                                                                                new LongJobProgressUpdate
                                                                                {
                                                                                    JobId = jobId.ToString(),
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

    }

    public interface ITimeSheet_EngineService : IServiceBase
    {

        Task<GenericResult<TimeSheet_CalculateOutModel>> Calculate(GenericRequest<TimeSheet_CalculateInModel> model, bool isSubProcess);

    }
}
