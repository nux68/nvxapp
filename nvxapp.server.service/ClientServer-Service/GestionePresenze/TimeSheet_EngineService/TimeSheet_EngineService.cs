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
using nvxapp.server.data.Extensions;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.FormuleEngine;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FestivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.infrastructure.MyMokeLongJob.Models;
using nvxapp.server.service.ClientServer_Service.infrastructure.Notifications;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;
using System.Text.Json;

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


ALTRI DATI:
  ORE GIORNALIERE 
     TEORICHE (quelle che i dipendente in assenza di anomalie dovrebbe fare)
        le ore gornaliere si calcolano trovando l'orario attivo per quel giorno e condiderando le coppie di timbrature previste da quell'orario (es. 8:00-12:00 e 14:00-18:00) si sommano le ore teoriche (in questo caso 8 ore)
     REALI (quelle che il dipendente ha effettivamente fatto)
        le ore reali si calcolano considerando le timbrature effettive e arrotondate del giorno (es. 8:05-12:00 e 14:00-17:50) si sommano le ore reali (in questo caso 7 ore e 45 minuti)
 
  se le ore TEORICHE non coincidono con quelle REALI, si deve capire se ci sono le condizioni per considerare la differenza come STRAORDINARIO o LAVORO SUPPLEMENTARE o ASSENZA

 
 
 */



/* Spiegazione struttura dati per il calcolo:

Parametri:
    Par_Orario: definisce un orario con le sue caratteristiche
    Par_OrarioIntervalloHH figlio di Par_Orario: definisce l'orario una coppia di entrata /uscita (es. 8:00-12:00) sono presenti i campo Limite_DX e Limite_SX che indicano entro quali solo le tolleranze per ritenere la timbbatura valide
    per ogni oracio ci possono essere più intervalli (es. 8:00-12:00 e 14:00-18:00) e il loro numero e definito dal campo NumeroCoppita di Par_Orario
    sono presenti a che dei campi per definire il tipo di arrotondamento della timbratura

    Par_ProfiloOrario: definisce un profilo orario che può essere di tipo settimanale o ciclico, contiene la durata del ciclo (es. 7 giorni) e il giorno di partenza del ciclo (es. se il ciclo è di 7 giorni e il giorno di partenza è il lunedi, allora il giorno 1 del ciclo è sempre il lunedi, il giorno 2 è sempre il martedi ecc.)
        sono definite delle proprieta per calcolare gli straordinari e il lavoro supplementare
        NumGiorniCiclo definisce la durata del ciclo (es. 7 giorni)
    Par_ProfiloOrarioGG definisce per ogni giorno del profilo orario, quali orari devono essere applicati (collegamento a Par_Orario tramite IdPar_Orario) e l'ordine di applicazione (campo ZOrder)
        per un girno possono essere definiti più orari e la chiave verra agganciata su ZOrder
 
 

Per un utente puo esistere: 
   1 record di Dip_Anagrafica che contiene i dati anagrafici e i dati che non cambiano mai
   N record di Dip_RapportoLavoro che contengono i dati del rapporto di lavoro è hanno un inizio ed eventualmente una fine
     quando si effettuano i calcoli, si deve tenere conto del rappoorto di lavoro attivo in quel giorno
   N record di Dip_ProfiloOrario che sono figli di Dip_RapportoLavoro, per un record di Dip_RapportoLavoro ci possono essere piu profili orari, che indicano quali orari verranno applicati in quel determinato perido

Dati delle presenze      

    Dip_GG_Richieste fanno capo a Dip_RapportoLavoro e sono del tipo   Timbratura,Giustificativo,NotaSpesa,ApprovazioneStraordinario
    hanno un perido di pertinenza DataDa DataA 
    nel campo Dati ce una stringa che rappresenta un oggetto diverso a seconda del tipo di richiesta, che verra serializzato opportunamente
    RichiestaStato che puo essere 
            Diretta,Immessa,Cancellata,Rifiutata,ApprovazioneInCorso,ParzialmenteApprovata,Approvata,
    RichiestaStato che puo essere 
            Diretta,Immessa,Cancellata,Rifiutata,ApprovazioneInCorso,ParzialmenteApprovata,Approvata,

    Dip_GG_Richieste viene create e utilizzata quando una timbratuto o giustificavo o strao ecc,ecc non posso essere considereti previa approvazione

    Dip_GG_Timbrature fanno capo a Dip_RapportoLavoro e ce ne possono essere più di una al giorno, contengono la data e l'ora della timbratura, il tipo (entrata/uscita) e lo stato (approvato/da approvare/rifiutato)
        GiornoCompetenza è il giono al quale verra agganciata la timbratura e potrebbe non corrispondere con la timbratura stessa
            (es. per timbrature fatte dopo la mezzanotte, il giorno competenza potrebbe essre quello precedente a quello della timbratura)
            quando una timbratura viene eseguita in quel istamte il valore viene messo in Timbratura e TimbraturaOriginale
            se l'utente HR vuole procedere a una modifica della timbratura, la timbratura modificata viene salvata in Timbratura e la timbratura originale viene mantenuta in TimbraturaOriginale
            quando la giornate viene ritenuta valita (cioè se sono soddisfatte tutte le coppie di timbrature per quel giorno) o su forzatura il valore di Timbratura viene arrotondato in base alle regole di arrotondamento definite in Par_Orario e salvato in TimbraturaArrotondata
        RichiestaStato viene abbinato allo stato dell' eventuale richiesta timbratura che deve essere approvata
   
    Dip_GG_Giustificativi fanno capo a Dip_RapportoLavoro e ce ne possono essere più di una al giorno contengono della alterazioni all orario della giornata
        RichiestaStato anche per loro come le timbrature viene abbinato allo stato dell' eventuale richiesta giustificativo che deve essere approvata 
 

    IdDip_GG_Richiesta

    

 */


namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService
{

    public class TimeSheet_EngineService : ServiceBase, ITimeSheet_EngineService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly ILongJobNotifier _longJobNotifier;
        private readonly IJobNotifier _jobNotifier;
        private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;
        private readonly IDip_GG_CausaliService _dip_GG_CausaliService;
        private readonly IDip_GG_GiustificativiService _dip_GG_GiustificativiService;
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;
        private readonly IDip_GG_ResultService _dip_GG_ResultService;


        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IDip_ProfiloOrarioRepository _dip_ProfiloOrarioRepository;
        private readonly IPar_OrarioService _par_OrarioService;
        private readonly IPar_ProfiloOrarioService _par_ProfiloOrarioService;
        private readonly IDip_RapportoLavoroService _dip_RapportoLavoroService;
        private readonly IPar_GiustificativiService _par_GiustificativiService;
        private readonly IPar_FestivitaRepository _par_FestivitaRepository;
        private readonly IDip_PeriodoRepository _dip_PeriodoRepository;
        private readonly IFormuleRunner _formuleRunner;

        // "Prova formula": calcolo senza scritture (niente cartellino mensile, niente SaveData)
        private bool _simulazione;

        // causali del giorno calcolate nella fase giornaliera: la fase di periodo aggiunge quelle delle
        // ore in piu' e poi vengono scritte (chiave: rapporto, giorno)
        private readonly Dictionary<(int, DateTime), Dictionary<int, TimeSpan>> _causaliGiorno = new();
        // importi del giorno per causale (giustificativi a importo): non toccano le ore, si scrivono con le causali
        private readonly Dictionary<(int, DateTime), Dictionary<int, decimal>> _importiGiorno = new();


        //private readonly IDip_GG_TimbraturaRepository _dip_GG_TimbraturaRepository;


        public TimeSheet_EngineService(IMapper mapper,
                                      UserManager<ApplicationUser> userManager,
                                      IAspNetUsersRepository aspNetUsersRepository,
                                      IOptions<JwtParameter> jwtParameter,
                                      IHttpContextAccessor httpContextAccessor,
                                      IConfiguration configuration,

                                      IDip_GG_TimbraturaRepository dip_GG_TimbraturaRepository,
                                      IDip_ProfiloOrarioRepository dip_ProfiloOrarioRepository,
                                      IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                      IPar_OrarioService par_OrarioService,
                                      IPar_ProfiloOrarioService par_ProfiloOrarioService,
                                      IDip_RapportoLavoroService dip_RapportoLavoroService,
                                      ILongJobNotifier longJobNotifier,
                                      IJobNotifier jobNotifier,
                                      IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                      IDip_GG_TimbraturaService dip_GG_TimbraturaService,
                                      IDip_GG_CausaliService dip_GG_CausaliService,
                                      IDip_GG_GiustificativiService dip_GG_GiustificativiService,
                                      IDip_GG_ResultService dip_GG_ResultService,
                                      IPar_GiustificativiService par_GiustificativiService,
                                      IDip_GG_RichiestaService dip_GG_RichiestaService,
                                      IPar_FestivitaRepository par_FestivitaRepository,
                                      IDip_PeriodoRepository dip_PeriodoRepository,
                                      IFormuleRunner formuleRunner

                                      ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _longJobNotifier = longJobNotifier;
            _jobNotifier = jobNotifier;
            _dip_GG_TimbraturaService = dip_GG_TimbraturaService;
            _dip_GG_CausaliService = dip_GG_CausaliService;
            _dip_GG_GiustificativiService = dip_GG_GiustificativiService;
            _dip_GG_RichiestaService = dip_GG_RichiestaService;
            _dip_ProfiloOrarioRepository = dip_ProfiloOrarioRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _par_OrarioService = par_OrarioService;
            _dip_GG_ResultService = dip_GG_ResultService;
            _par_ProfiloOrarioService = par_ProfiloOrarioService;
            _dip_RapportoLavoroService = dip_RapportoLavoroService;
            _par_GiustificativiService = par_GiustificativiService;
            _par_FestivitaRepository = par_FestivitaRepository;
            _dip_PeriodoRepository = dip_PeriodoRepository;
            _formuleRunner = formuleRunner;

            //_dip_GG_TimbraturaRepository = dip_GG_TimbraturaRepository;
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
                            var scopedEngine = (TimeSheet_EngineService)scope.ServiceProvider.GetRequiredService<ITimeSheet_EngineService>();
                            var scopedNotifier = scope.ServiceProvider.GetRequiredService<ILongJobNotifier>();

                            try
                            {
                                Log.Information("Starting TimeSheet calculation for users {UserIds} from {Dal} to {Al}. jobID = {JobId}", model.Data.TimeSheet_Calculate.SelectedUserId, model.Data.TimeSheet_Calculate.Dal, model.Data.TimeSheet_Calculate.Al, jobId);

                                await scopedNotifier.LongJobProgressAsync(userId,
                                                                            new LongJobProgressUpdate
                                                                            {
                                                                                JobId = jobId.ToString(),
                                                                                JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                                                                                Payload = model.Data.TimeSheet_Calculate,
                                                                                ProgressPercentage = 0,
                                                                                Message = new Message { Text = AttendanceTrackingMessages.Get("TimesheetCalcStarted"), MsgType = MessageType.Information }
                                                                            }
                                                                         );

                                // si caricano i mesi interi e le settimane a cavallo: le ore in piu' si qualificano
                                // sul periodo di conteggio (giorno, settimana, mese) e servono anche i giorni non ricalcolati
                                var caricamento = RangeCaricamento(model.Data.TimeSheet_Calculate.Dal, model.Data.TimeSheet_Calculate.Al);
                                var req_OrariSchema_4User = new GenericRequest<Timesheet_AllData_InModel>();
                                req_OrariSchema_4User.Data.Dal = caricamento.Dal;
                                req_OrariSchema_4User.Data.Al = caricamento.Al;
                                req_OrariSchema_4User.Data.UsersId = model.Data.TimeSheet_Calculate.SelectedUserId;

                                var AllData = await scopedEngine.Get_Timesheet_AllData(req_OrariSchema_4User, true);

                                if (AllData.Success && AllData.Data != null)
                                {
                                    // ciclo su ogni utente selezionato
                                    int idxUser = 0;
                                    foreach (var userId_calc in model.Data.TimeSheet_Calculate.SelectedUserId)
                                    {
                                        var progress = (int)((idxUser / (double)model.Data.TimeSheet_Calculate.SelectedUserId.Count) * 100);
                                        await scopedNotifier.LongJobProgressAsync(userId,
                                                                                    new LongJobProgressUpdate
                                                                                    {
                                                                                        JobId = jobId.ToString(),
                                                                                        JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                                                                                        Payload = model.Data.TimeSheet_Calculate,
                                                                                        ProgressPercentage = progress,
                                                                                        Message = new Message { Text = AttendanceTrackingMessages.Get("TimesheetCalcStep", idxUser + 1, model.Data.TimeSheet_Calculate.SelectedUserId.Count), MsgType = MessageType.Information }
                                                                                    }
                                                                                    );



                                        // anagrafica dell'utente corrente
                                        var anagrafica_calc = AllData.Data.OrariSchema_4User_OutModel.Dip_Anagrafica
                                            .FirstOrDefault(a => a.IdAspNetUsers == userId_calc);

                                        if (anagrafica_calc == null) continue;

                                        // rapporti di lavoro dell'utente corrente
                                        var rapporti_calc = AllData.Data.OrariSchema_4User_OutModel.Dip_RapportoLavoro
                                            .Where(r => r.IdDip_Anagrafica == anagrafica_calc.Id)
                                            .ToList();

                                        foreach (var rapporto_calc in rapporti_calc)
                                        {
                                            await scopedEngine.CalcolaRapporto(anagrafica_calc, model.Data.TimeSheet_Calculate, rapporto_calc, AllData.Data);
                                        }

                                        //notifica singola, per il refresh di che fosse posizionato sul cartellino
                                        //await _jobNotifier.JobNotifierAsync(userId, new JobNotifierData()
                                        //{
                                        //    JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                                        //    Payload = model.Data.TimeSheet_Calculate,
                                        //    Message = new Message { Text = $"Calcolo presenze terminato user Id =  {userId_calc}", MsgType = MessageType.Information }
                                        //});


                                        idxUser++;
                                        //await Task.Delay(1000);
                                    }


                                    await scopedEngine.SaveData(AllData.Data);

                                    Log.Information("Background task for job {JobId} has finished successfully.", jobId);
                                    await scopedNotifier.LongJobProgressAsync(userId,
                                                                                new LongJobProgressUpdate
                                                                                {
                                                                                    JobId = jobId.ToString(),
                                                                                    JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                                                                                    Payload = model.Data.TimeSheet_Calculate,
                                                                                    ProgressPercentage = 100,
                                                                                    Message = new Message { Text = AttendanceTrackingMessages.Get("TimesheetCalcCompleted"), MsgType = MessageType.Information },
                                                                                    IsFinished = true
                                                                                }
                                                                               );

                                    // chi ha aperto il cartellino dei dipendenti ricalcolati lo ricarica
                                    await NotificaCartellini(scope, IdCompany, model.Data.TimeSheet_Calculate, userId);

                                }

                            }
                            catch (Exception ex)
                            {
                                Log.Error(ex, "Background task for job {JobId} failed.", jobId);
                                await scopedNotifier.LongJobProgressAsync(userId,
                                                                            new LongJobProgressUpdate
                                                                            {
                                                                                JobId = jobId.ToString(),
                                                                                JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                                                                                Payload = model.Data.TimeSheet_Calculate,
                                                                                ProgressPercentage = 100,
                                                                                Message = new Message { Text = AttendanceTrackingMessages.Get("TimesheetCalcFailed", ex.Message), MsgType = MessageType.Exception },
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

        /*
         Fine ricalcolo: notifica (JobNotifierAsync) ai dipendenti ricalcolati e agli amministratori dell'azienda
         (CompanyPowerAdmin e CompanyAdmin attivi), cosi' i cartellini aperti si ricaricano: il dipendente vede
         la richiesta approvata o rifiutata dall'HR e l'HR vede la richiesta fatta o cancellata dal dipendente.
         Chi ha avviato il ricalcolo e' escluso: riceve gia' la fine del job (LongJobProgress).
         Un errore di invio non tocca il ricalcolo, gia' salvato.
        */
        private static async Task NotificaCartellini(IServiceScope scope, int idCompany, TimeSheet_CalculateModel calcolo, string chiHaAvviato)
        {
            try
            {
                var jobNotifier = scope.ServiceProvider.GetRequiredService<IJobNotifier>();
                var userCompanyRepository = scope.ServiceProvider.GetRequiredService<IUserCompanyRepository>();
                var aspNetRolesRepository = scope.ServiceProvider.GetRequiredService<IAspNetRolesRepository>();
                var aspNetUserRolesRepository = scope.ServiceProvider.GetRequiredService<IAspNetUserRolesRepository>();

                var ruoliAdmin = aspNetRolesRepository.FindAll(r => r.Code == RoleCode.CompanyPowerAdmin || r.Code == RoleCode.CompanyAdmin)
                                                      .Select(r => r.Id)
                                                      .ToList();
                var utentiAzienda = userCompanyRepository.FindAll(u => u.IdCompany == idCompany && !u.Disabled)
                                                         .Select(u => u.IdAspNetUsers)
                                                         .ToList();
                var amministratori = aspNetUserRolesRepository.FindAll(r => ruoliAdmin.Contains(r.RoleId) && utentiAzienda.Contains(r.UserId))
                                                              .Select(r => r.UserId)
                                                              .ToList();

                var destinatari = calcolo.SelectedUserId.Concat(amministratori)
                                         .Where(x => !string.IsNullOrEmpty(x) && x != chiHaAvviato)
                                         .Distinct();

                foreach (var destinatario in destinatari)
                {
                    await jobNotifier.JobNotifierAsync(destinatario, new JobNotifierData
                    {
                        JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                        Payload = calcolo,
                    });
                }
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Ricalcolo cartellino: notifica di aggiornamento non inviata");
            }
        }

        public virtual async Task<GenericResult<OrariSchema_4User_OutModel>> Get_OrariSchema_4User(GenericRequest<OrariSchema_4User_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new OrariSchema_4User_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    var Dal = model.Data.Dal;
                    var Al = model.Data.Al;

                    #region "rapporto lavoro "

                    // ── 1. Dip_Anagrafica ────────────────────────────────────────────
                    // ricava le anagrafiche tramite utility (non accesso diretto al repository)

                    retVal.Dip_Anagrafica = await _gestionePresenzeUserUtility.GetAnagraficheByUsersId(model.Data.UsersId);

                    var anagraficaIds = retVal.Dip_Anagrafica.Select(a => a.Id).ToList();

                    // ── 2. Dip_RapportoLavoro ────────────────────────────────────────
                    // usa il service dedicato che applica la stessa logica di sovrapposizione
                    var rapporti_model = new List<Dip_RapportoLavoroModel>();
                    if (anagraficaIds.Count > 0)
                    {
                        var req_Rapporti = new GenericRequest<Dip_RapportoLavoro_Get_4Users_InModel>();
                        req_Rapporti.Data.UsersId = model.Data.UsersId;
                        req_Rapporti.Data.Dal = Dal;
                        req_Rapporti.Data.Al = Al;

                        var res_Rapporti = await _dip_RapportoLavoroService.Dip_RapportoLavoro_Get_4Users(req_Rapporti, true);
                        if (res_Rapporti.Success && res_Rapporti.Data != null)
                            retVal.Dip_RapportoLavoro = res_Rapporti.Data.Dip_RapportoLavoro;

                        // tutti i rapporti, anche fuori periodo: solo per la visualizzazione del cartellino
                        retVal.Dip_RapportoLavoro_Tutti = _mapper.Map<List<Dip_RapportoLavoroModel>>(
                            _dip_RapportoLavoroRepository.FindAll(r => anagraficaIds.Contains(r.IdDip_Anagrafica)).ToList());
                    }

                    // mantiene le entity in-memory per la logica di composizione DaySlots
                    var rapportoIds = retVal.Dip_RapportoLavoro.Select(r => r.Id).ToList();
                    var rapporti = _dip_RapportoLavoroRepository
                        .FindAll(r => rapportoIds.Contains(r.Id))
                        .ToList();



                    #endregion

                    #region "Par_ProfiloOrario / Par_ProfiloOrarioGG "

                    // Stesso criterio di sovrapposizione: la riga di profilo orario è
                    // compatibile se il suo intervallo [profilo.Dal, profilo.Al] si
                    // sovrappone anche parzialmente al periodo richiesto [Dal, Al].
                    //
                    // Condizione: profilo.Dal <= Al  &&  profilo.Al >= Dal


                    var profili = _dip_ProfiloOrarioRepository.FindAll(p =>
                                                                                rapportoIds.Contains(p.IdDip_RapportoLavoro) &&
                                                                                p.Dal <= Al &&
                                                                                p.Al >= Dal)
                                                                            .ToList();

                    // carica le testate dei profili orario referenziati
                    // (serve TipoProfilo e NumGiorniCiclo per la risoluzione del giorno)

                    var parProfiloIds = profili.Where(p => p.IdPar_ProfiloOrario.HasValue)
                                               .Select(p => p.IdPar_ProfiloOrario!.Value)
                                               .Distinct()
                                               .ToList();

                    retVal.Par_ProfiloOrario = new List<Par_ProfiloOrarioModel>();
                    retVal.Par_ProfiloOrarioGG = new List<Par_ProfiloOrarioGGModel>();

                    foreach (var idProfilo in parProfiloIds)
                    {
                        var req_ProfPar = new GenericRequest<Par_ProfiloOrario_GetInModel>();
                        req_ProfPar.Data.Id = idProfilo;

                        var res_ProfPar = await _par_ProfiloOrarioService.Par_ProfiloOrarioGet(req_ProfPar, true);
                        if (res_ProfPar.Success && res_ProfPar.Data != null)
                        {
                            if (res_ProfPar.Data.Par_ProfiloOrario != null)
                                retVal.Par_ProfiloOrario.Add(res_ProfPar.Data.Par_ProfiloOrario);

                            retVal.Par_ProfiloOrarioGG.AddRange(res_ProfPar.Data.Par_ProfiloOrarioGG);
                        }
                    }

                    #endregion

                    #region "Festivita'"

                    retVal.Par_Festivita = _mapper.Map<List<Par_FestivitaModel>>(
                        _par_FestivitaRepository.FindAll(f => f.IdAz_Anagrafica == company_DATA.az_Anagrafica.Id).ToList());

                    #endregion

                    #region "DaySlots"

                    // per ogni dipendente × ogni giorno del periodo richiesto, risolve
                    // quale profilo è attivo e tutte le sue righe orario ordinate per ZOrder

                    foreach (var rapporto in rapporti)
                    {
                        var anagrafica = retVal.Dip_Anagrafica.First(a => a.Id == rapporto.IdDip_Anagrafica);

                        // limite effettivo del rapporto intersecato con il periodo richiesto
                        var giornoInizio = Dal > rapporto.DataAss!.Value ? Dal : rapporto.DataAss!.Value;
                        var giornoFine = (rapporto.DataLic == null || rapporto.DataLic.Value > Al)
                                            ? Al
                                            : rapporto.DataLic.Value;

                        for (var giorno = giornoInizio; giorno <= giornoFine; giorno = giorno.AddDays(1))
                        {
                            // trova il profilo Dip attivo esattamente quel giorno
                            var profilo = profili
                                .Where(p => p.IdDip_RapportoLavoro == rapporto.Id
                                         && p.Dal <= giorno
                                         && p.Al >= giorno
                                         && p.IdPar_ProfiloOrario.HasValue)
                                .FirstOrDefault();

                            if (profilo == null) continue;

                            var parProfilo = retVal.Par_ProfiloOrario.FirstOrDefault(pp => pp.Id == profilo.IdPar_ProfiloOrario!.Value);

                            if (parProfilo == null) continue;

                            // calcola il NumGiorno in base al TipoProfilo
                            int numGiorno;
                            if (parProfilo.TipoProfilo == TipoProfilo.Settimanale)
                            {
                                // Convenzione DB: 1=Lun, 2=Mar, 3=Mer, 4=Gio, 5=Ven, 6=Sab, 7=Dom
                                // DayOfWeek C#:   1=Lun, 2=Mar, 3=Mer, 4=Gio, 5=Ven, 6=Sab, 0=Dom
                                numGiorno = giorno.DayOfWeek == DayOfWeek.Sunday ? 7 : (int)giorno.DayOfWeek;
                            }
                            else // Ciclico
                            {
                                numGiorno = ((giorno - profilo.Dal).Days + profilo.NumGiornoPartenzaCiclo)
                                            % parProfilo.NumGiorniCiclo;
                            }

                            // recupera TUTTE le righe del giorno ordinate per ZOrder
                            var orariDelGiorno = retVal.Par_ProfiloOrarioGG.Where(gg => gg.IdPar_ProfiloOrario == profilo.IdPar_ProfiloOrario!.Value
                                          && gg.NumGiorno == numGiorno)
                                .OrderBy(gg => gg.ZOrder)
                                .Select(gg => new Dip_ProfiloOrario_DaySlot_GG
                                {
                                    ZOrder = gg.ZOrder,
                                    IdPar_Orario = gg.IdPar_Orario
                                })
                                .ToList();

                            // festivita' del calendario: vale l'orario festivo del profilo
                            bool festivo = GestionePresenzeFestivita.IsFestivita(retVal.Par_Festivita, giorno);
                            if (festivo && parProfilo.idPar_Orario_Festivo > 0)
                            {
                                orariDelGiorno = new List<Dip_ProfiloOrario_DaySlot_GG>
                                {
                                    new Dip_ProfiloOrario_DaySlot_GG { ZOrder = 1, IdPar_Orario = parProfilo.idPar_Orario_Festivo }
                                };
                            }

                            retVal.DaySlots.Add(new Dip_ProfiloOrario_DaySlot
                            {
                                Festivo = festivo,
                                IdAspNetUsers = anagrafica.IdAspNetUsers,
                                IdDip_RapportoLavoro = rapporto.Id,
                                Data = giorno,
                                IdPar_ProfiloOrario = profilo.IdPar_ProfiloOrario!.Value,
                                Orari = orariDelGiorno,
                                IdAz_SubCommessaAttivita = rapporto.IdAz_SubCommessaAttivita
                            });
                        }
                    }

                    #endregion

                    #region "Orario Par_OrarioIntervalloHH"

                    // lista univoca di tutti gli IdPar_Orario presenti nei DaySlots
                    var parOrarioIds = retVal.DaySlots.SelectMany(ds => ds.Orari)
                                                      .Select(gg => gg.IdPar_Orario)
                                                      .Distinct()
                                                      .ToList();

                    retVal.ParOrario = new List<Par_OrarioModel>();
                    retVal.Par_OrarioIntervalloHH = new List<Par_OrarioIntervalloHHModel>();
                    retVal.Par_OrarioPausa = new List<Par_OrarioPausaModel>();

                    foreach (var idOrario in parOrarioIds)
                    {
                        var req = new GenericRequest<Par_Orario_GetInModel>();
                        req.Data.Id = idOrario;

                        var res = await _par_OrarioService.Par_OrarioGet(req, true);
                        if (res.Success && res.Data != null)
                        {
                            if (res.Data.Par_Orario != null)
                                retVal.ParOrario.Add(res.Data.Par_Orario);

                            retVal.Par_OrarioIntervalloHH.AddRange(res.Data.Par_OrarioIntervalloHH);
                            retVal.Par_OrarioPausa.AddRange(res.Data.Par_OrarioPausa);
                        }
                    }

                    #endregion

                    #region "Par_Giustificativi"

                    var req_Just = new GenericRequest<Par_GiustificativiInModel>();
                    var res_Just = await _par_GiustificativiService.GetAll(req_Just, true);
                    if (res_Just.Success && res_Just.Data != null)
                    {
                        retVal.Par_Giustificativi = res_Just.Data.Par_Giustificativi;
                    }


                    #endregion

                }

                //await Task.Delay(DelayAsyncMethod);
                await Task.Delay(0);
                return retVal;

            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Dip_GG_AllData_OutModel>> Dip_GG_AllData_AllData(GenericRequest<Dip_GG_AllData_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_GG_AllData_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {

                    // 1) Recupera timbrature per il calcolo
                    var req_Timbrature = new GenericRequest<Dip_GG_Timbratura_Get_4Calculation_InModel>();
                    req_Timbrature.Data.UsersId = model.Data.UsersId;
                    req_Timbrature.Data.Dal = model.Data.Dal;
                    req_Timbrature.Data.Al = model.Data.Al;

                    var res_Timbrature = await _dip_GG_TimbraturaService.Dip_GG_Timbratura_Get_4Calculation(req_Timbrature, true);
                    if (res_Timbrature.Success && res_Timbrature.Data != null)
                    {
                        retVal.Dip_GG_Timbratura = res_Timbrature.Data.Dip_GG_Timbratura.OrderBy(x => x.Timbratura).ToList();

                        var groupedByDay = retVal.Dip_GG_Timbratura.GroupBy(x => x.GiornoCompetenza.Date).ToList();
                        foreach (var group in groupedByDay)
                        {
                            for (int i = 0; i < group.Count(); i++)
                            {
                                var timbraturaItem = group.ElementAt(i);

                                // Applica la logica solo se il TipoTimbratura è diverso da Attivita
                                //if (timbraturaItem.TimbraturaTipo != TipoTimbratura.Attivita)
                                //{
                                //    //timbraturaItem.TimbraturaTipo = (i % 2 == 0) ? TipoTimbratura.Entrata : TipoTimbratura.Uscita;
                                //}
                            }
                        }

                    }

                    // 2) Recupera causali per il calcolo
                    var req_Causali = new GenericRequest<Dip_GG_Causali_Get_4Calculation_InModel>();
                    req_Causali.Data.UsersId = model.Data.UsersId;
                    req_Causali.Data.Dal = model.Data.Dal;
                    req_Causali.Data.Al = model.Data.Al;

                    var res_Causali = await _dip_GG_CausaliService.Dip_GG_Causali_Get_4Calculation(req_Causali, true);
                    if (res_Causali.Success && res_Causali.Data != null)
                    {
                        retVal.Dip_GG_Causali = res_Causali.Data.Dip_GG_Causali;
                    }

                    // 3) Recupera giustificativi per il calcolo
                    var req_Giustificativi = new GenericRequest<Dip_GG_Giustificativi_Get_4Calculation_InModel>();
                    req_Giustificativi.Data.UsersId = model.Data.UsersId;
                    req_Giustificativi.Data.Dal = model.Data.Dal;
                    req_Giustificativi.Data.Al = model.Data.Al;

                    var res_Giustificativi = await _dip_GG_GiustificativiService.Dip_GG_Giustificativi_Get_4Calculation(req_Giustificativi, true);
                    if (res_Giustificativi.Success && res_Giustificativi.Data != null)
                    {
                        retVal.Dip_GG_Giustificativi = res_Giustificativi.Data.Dip_GG_Giustificativi;
                    }

                    // 4) Recupera richieste per il calcolo
                    var req_Richieste = new GenericRequest<Dip_GG_Richiesta_Get_4Calculation_InModel>();
                    req_Richieste.Data.UsersId = model.Data.UsersId;
                    req_Richieste.Data.Dal = model.Data.Dal;
                    req_Richieste.Data.Al = model.Data.Al;

                    var res_Richieste = await _dip_GG_RichiestaService.Dip_GG_Richiesta_Get_4Calculation(req_Richieste, true);
                    if (res_Richieste.Success && res_Richieste.Data != null)
                    {
                        retVal.Dip_GG_Richiesta = res_Richieste.Data.Dip_GG_Richiesta;
                    }

                    // 5) Recupera result per il calcolo
                    var req_Result = new GenericRequest<Dip_GG_Result_Get_4Calculation_InModel>();
                    req_Result.Data.UsersId = model.Data.UsersId;
                    req_Result.Data.Dal = model.Data.Dal;
                    req_Result.Data.Al = model.Data.Al;

                    var res_Result = await _dip_GG_ResultService.Dip_GG_Result_Get_4Calculation(req_Result, true);
                    if (res_Result.Success && res_Result.Data != null)
                    {
                        retVal.Dip_GG_Result = res_Result.Data.Dip_GG_Result;
                    }

                }

                //await Task.Delay(DelayAsyncMethod);
                await Task.Delay(0);
                return retVal;

            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Timesheet_AllData_OutModel>> Get_Timesheet_AllData(GenericRequest<Timesheet_AllData_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Timesheet_AllData_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);
                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                {
                    List<string> usersId = new List<string>();
                    if (model.Data.UsersId == null)
                        usersId.Add(this.CurrentUserId);
                    else
                        usersId = model.Data.UsersId;



                    var req_OrariSchema_4User = new GenericRequest<OrariSchema_4User_InModel>();
                    req_OrariSchema_4User.Data.Dal = model.Data.Dal;
                    req_OrariSchema_4User.Data.Al = model.Data.Al;
                    req_OrariSchema_4User.Data.UsersId = usersId;

                    var res_OrariSchema_4User = await this.Get_OrariSchema_4User(req_OrariSchema_4User, true);
                    if (res_OrariSchema_4User.Success && res_OrariSchema_4User.Data != null)
                    {
                        retVal.OrariSchema_4User_OutModel = res_OrariSchema_4User.Data;
                    }

                    var req_Dip_GG_AllData = new GenericRequest<Dip_GG_AllData_InModel>();
                    req_Dip_GG_AllData.Data.Dal = model.Data.Dal;
                    req_Dip_GG_AllData.Data.Al = model.Data.Al;
                    req_Dip_GG_AllData.Data.UsersId = usersId;

                    var res_Dip_GG_AllData = await this.Dip_GG_AllData_AllData(req_Dip_GG_AllData, true);
                    if (res_Dip_GG_AllData.Success && res_Dip_GG_AllData.Data != null)
                    {
                        retVal.Dip_GG_AllData_OutModel = res_Dip_GG_AllData.Data;
                    }

                }

                //await Task.Delay(DelayAsyncMethod);
                await Task.Delay(0);
                return retVal;

            }, isSubProcess);
        }




        /// <summary>
        /// Range di dati da caricare per un ricalcolo: i mesi interi coinvolti, estesi alle settimane a cavallo
        /// (e ai rispettivi mesi), cosi' ogni periodo di conteggio toccato e il cartellino del mese sono completi.
        /// </summary>
        private static (DateTime Dal, DateTime Al) RangeCaricamento(DateTime dal, DateTime al)
        {
            var inizio = GestionePresenzeEccedenze.InizioSettimana(new DateTime(dal.Year, dal.Month, 1));
            inizio = new DateTime(inizio.Year, inizio.Month, 1);

            var fineMese = new DateTime(al.Year, al.Month, 1).AddMonths(1).AddDays(-1);
            var fine = GestionePresenzeEccedenze.InizioSettimana(fineMese).AddDays(6);
            fine = new DateTime(fine.Year, fine.Month, 1).AddMonths(1).AddDays(-1);

            return (inizio, fine);
        }

        private static Par_ProfiloOrarioModel? ProfiloDelGiorno(OrariSchema_4User_OutModel orariSchema, int IdDip_RapportoLavoro, DateTime day)
        {
            var daySlot = orariSchema.DaySlots.FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro && ds.Data.Date == day.Date);
            if (daySlot == null)
                return null;
            return orariSchema.Par_ProfiloOrario.FirstOrDefault(p => p.Id == daySlot.IdPar_ProfiloOrario);
        }

        private static GestionePresenzeEccedenze.Parametri ParametriEccedenze(Par_ProfiloOrarioModel profilo, Dip_RapportoLavoroModel rapporto)
        {
            return new GestionePresenzeEccedenze.Parametri
            {
                TipoConteggio = profilo.TipoConteggio,
                PartTime = rapporto.TipoContratto == TipoContratto.PartTime,
                SogliaGiornalieraFT = profilo.StraoSogliaHHFullTime.HasValue ? (int)profilo.StraoSogliaHHFullTime.Value.ToTimeSpan().TotalMinutes : 0,
                SogliaSettimanaleFT = (int)Math.Round(profilo.SogliaSettimanaleFT * 60),
                SupplLimitePerc = profilo.SupplLimitePerc,
                OreSettimanaliContratto = rapporto.OreSettimanaliContratto.HasValue ? (int)Math.Round(rapporto.OreSettimanaliContratto.Value * 60) : null,
                AssenzeContanoSoglia = profilo.AssenzeContanoSoglia
            };
        }

        /// <summary>
        /// Calcolo di un rapporto di lavoro sul periodo richiesto:
        ///   1) periodi di conteggio delle ore in piu' toccati dal ricalcolo (giorno / settimana / mese del profilo)
        ///   2) fase giornaliera su tutti i giorni di quei periodi (le opzioni del calcolo solo nel periodo richiesto)
        ///   3) qualifica delle ore in piu' per periodo, limite del supplementare per settimana, tetto mensile HR
        ///   4) causali dei giorni e cartellini mensili (Dip_Periodo)
        /// I giorni dei mesi approvati sono bloccati: non si ricalcolano e mantengono la loro qualifica.
        /// </summary>
        private async Task CalcolaRapporto(Dip_AnagraficaModel anagrafica, TimeSheet_CalculateModel calc, Dip_RapportoLavoroModel rapporto, Timesheet_AllData_OutModel AllData)
        {
            var orariSchema = AllData.OrariSchema_4User_OutModel;
            var results = AllData.Dip_GG_AllData_OutModel.Dip_GG_Result.Where(r => r.IdDip_RapportoLavoro == rapporto.Id).ToList();

            var inizioRapporto = rapporto.DataAss!.Value.Date;
            var fineRapporto = rapporto.DataLic?.Date;
            bool NelRapporto(DateTime d) => d >= inizioRapporto && (fineRapporto == null || d <= fineRapporto.Value);

            var dal = calc.Dal.Date;
            var al = calc.Al.Date;

            // 1) periodi di conteggio
            var periodi = new List<(DateTime Dal, DateTime Al, Par_ProfiloOrarioModel Profilo)>();
            var giorniDaCalcolare = new SortedSet<DateTime>();
            for (var d = dal; d <= al; d = d.AddDays(1))
            {
                if (!NelRapporto(d)) continue;
                giorniDaCalcolare.Add(d);

                var profilo = ProfiloDelGiorno(orariSchema, rapporto.Id, d);
                if (profilo == null || !profilo.EccedenzeAttive) continue;

                var per = GestionePresenzeEccedenze.Periodo(d, profilo.TipoConteggio);
                if (!periodi.Any(p => p.Dal == per.Dal && p.Al == per.Al))
                    periodi.Add((per.Dal, per.Al, profilo));
            }
            foreach (var per in periodi)
                for (var d = per.Dal; d <= per.Al; d = d.AddDays(1))
                    if (NelRapporto(d) && results.Any(r => r.Data.Date == d))
                        giorniDaCalcolare.Add(d);

            // formule personalizzate del rapporto nei giorni da calcolare (solo piano Pro, ANALISI_ATTENDANCETRACKING_FORMULE.md)
            if (giorniDaCalcolare.Count > 0)
            {
                int.TryParse(this.CurrentCompany, out var idCompanyFormule);
                await _formuleRunner.PreparaRapportoAsync(idCompanyFormule, rapporto, giorniDaCalcolare.Min, giorniDaCalcolare.Max, orariSchema.Par_Giustificativi);
            }

            // 2) fase giornaliera
            foreach (var d in giorniDaCalcolare)
                await CalcolaGiorno(anagrafica, calc, rapporto, d, AllData, conOpzioni: d >= dal && d <= al);

            // 3) fase di periodo
            var giorni = results.Where(r => NelRapporto(r.Data.Date))
                                .Select(r => new GestionePresenzeEccedenze.Giorno
                                {
                                    Data = r.Data.Date,
                                    Bloccato = r.Stato.HasFlag(GG_ResultStato.Locked) || !giorniDaCalcolare.Contains(r.Data.Date),
                                    Teo = (int)r.HH_Teo.ToTimeSpan().TotalMinutes,
                                    Lavorabili = r.Min_Lavorabili,
                                    Assenze = r.Min_Assenze,
                                    Ecc = r.Min_Ecc,
                                    EccNott = r.Min_EccNott,
                                    Festivo = r.Festivo,
                                    Provvisoria = r.EccProvvisoria,
                                    Suppl = r.Min_Suppl,
                                    SupplEcc = r.Min_SupplEcc,
                                    Strao = r.Min_Strao,
                                    StraoNott = r.Min_StraoNott,
                                    StraoFest = r.Min_StraoFest,
                                    StraoFestNott = r.Min_StraoFestNott,
                                    EccOrd = r.Min_EccOrd,
                                    NonRiconosciute = r.Min_NonRiconosciute
                                })
                                .OrderBy(g => g.Data)
                                .ToList();

            // i giorni ricalcolati ripartono da zero: quelli senza profilo o con le eccedenze spente restano senza qualifica
            foreach (var g in giorni.Where(g => !g.Bloccato))
                g.AzzeraQualifica();

            foreach (var per in periodi)
            {
                var giorniPeriodo = giorni.Where(g => g.Data >= per.Dal && g.Data <= per.Al).ToList();
                GestionePresenzeEccedenze.QualificaPeriodo(giorniPeriodo, per.Al, ParametriEccedenze(per.Profilo, rapporto), DateTime.Today);
            }

            // supplementare oltre il limite: per settimana lun-dom
            foreach (var lunedi in giorniDaCalcolare.Select(GestionePresenzeEccedenze.InizioSettimana).Distinct())
            {
                var giorniSettimana = giorni.Where(g => g.Data >= lunedi && g.Data <= lunedi.AddDays(6)).ToList();
                var profilo = giorniSettimana.Where(g => !g.Bloccato)
                                             .Select(g => ProfiloDelGiorno(orariSchema, rapporto.Id, g.Data))
                                             .LastOrDefault(p => p != null && p.EccedenzeAttive);
                if (profilo != null && rapporto.TipoContratto == TipoContratto.PartTime)
                    GestionePresenzeEccedenze.SeparaSupplementare(giorniSettimana, ParametriEccedenze(profilo, rapporto));
            }

            // tetto delle ore riconosciute deciso dall'HR sul cartellino del mese
            var mesi = giorniDaCalcolare.Select(d => (d.Year, d.Month)).Distinct().ToList();
            foreach (var (anno, mese) in mesi)
            {
                var periodo = _dip_PeriodoRepository.FindAll(p => p.IdDip_RapportoLavoro == rapporto.Id && p.Anno == anno && p.Mese == mese).FirstOrDefault();
                if (periodo?.Min_Riconosciute != null && periodo.Stato != PeriodoStato.Approvato)
                    GestionePresenzeEccedenze.ApplicaTetto(giorni.Where(g => g.Data.Year == anno && g.Data.Month == mese).ToList(), periodo.Min_Riconosciute.Value);
            }

            // 4) risultati e causali dei giorni ricalcolati
            bool bancaOre = rapporto.DestinazioneEccedenze == DestinazioneEccedenze.BancaOre;
            foreach (var g in giorni.Where(g => !g.Bloccato))
            {
                var res = results.First(r => r.Data.Date == g.Data);
                res.EccProvvisoria = g.Provvisoria;
                res.Min_Suppl = g.Suppl;
                res.Min_SupplEcc = g.SupplEcc;
                res.Min_Strao = g.Strao;
                res.Min_StraoNott = g.StraoNott;
                res.Min_StraoFest = g.StraoFest;
                res.Min_StraoFestNott = g.StraoFestNott;
                res.Min_EccOrd = g.EccOrd;
                res.Min_NonRiconosciute = g.NonRiconosciute;
                res.BancaOre = bancaOre && g.Qualificate > 0;

                bool haCausali = _causaliGiorno.TryGetValue((rapporto.Id, g.Data), out var causali);
                causali ??= new Dictionary<int, TimeSpan>();

                var profilo = ProfiloDelGiorno(orariSchema, rapporto.Id, g.Data);
                if (haCausali && profilo != null)
                    AggiungiCausaliEccedenze(causali, profilo, g, bancaOre);

                _importiGiorno.TryGetValue((rapporto.Id, g.Data), out var importi);

                // formule della fase Finale: lavorano sulle causali gia' qualificate, prima della scrittura
                var importiFormule = importi ?? new Dictionary<int, decimal>();
                var statoFormule = _formuleRunner.Esegui(FormulaFase.Finale, DatiFormule(anagrafica, rapporto, g.Data, FormulaFase.Finale, AllData, res, causali, importiFormule));
                if (statoFormule != 0)
                {
                    var stato = res.Stato;
                    AggiungiStatoFormule(ref stato, statoFormule);
                    res.Stato = stato;
                }

                if (!haCausali && causali.Count == 0 && importiFormule.Count == 0)
                    continue;
                ScriviCausali(AllData, rapporto.Id, g.Data, causali, importiFormule.Count > 0 ? importiFormule : importi);
            }

            // 5) cartellini mensili (non in "Prova formula": nessuna scrittura)
            if (!_simulazione)
                foreach (var (anno, mese) in mesi)
                    await AggiornaCartellinoMese(orariSchema, rapporto, anno, mese, results);
        }

        private static void AggiungiCausaliEccedenze(Dictionary<int, TimeSpan> causali, Par_ProfiloOrarioModel profilo, GestionePresenzeEccedenze.Giorno g, bool bancaOre)
        {
            void Aggiungi(int? idCausale, int minuti)
            {
                if (idCausale is int id && id > 0 && minuti > 0)
                    causali[id] = causali.GetValueOrDefault(id) + TimeSpan.FromMinutes(minuti);
            }

            if (bancaOre && profilo.IdCausale_BancaOre > 0)
            {
                Aggiungi(profilo.IdCausale_BancaOre, g.Qualificate);
                return;
            }

            // le causali facoltative, se non impostate, ricadono su quella base
            int strao = profilo.IdCausale_Lavoro_Strao;
            Aggiungi(strao, g.Strao);
            Aggiungi(profilo.IdCausale_Lavoro_StraoNott ?? strao, g.StraoNott);
            Aggiungi(profilo.IdCausale_Lavoro_StraoFest ?? strao, g.StraoFest);
            Aggiungi(profilo.IdCausale_Lavoro_StraoFestNott ?? profilo.IdCausale_Lavoro_StraoFest ?? profilo.IdCausale_Lavoro_StraoNott ?? strao, g.StraoFestNott);
            Aggiungi(profilo.IdCausale_Lavoro_Suppl, g.Suppl);
            Aggiungi(profilo.IdCausale_Lavoro_SupplEcc ?? profilo.IdCausale_Lavoro_Suppl, g.SupplEcc);
            Aggiungi(profilo.IdCausale_Lavoro_EccOrd ?? strao, g.EccOrd);
        }

        /// <summary>Totali del cartellino mensile (Dip_Periodo); un mese approvato non viene piu' toccato.</summary>
        private async Task AggiornaCartellinoMese(OrariSchema_4User_OutModel orariSchema, Dip_RapportoLavoroModel rapporto, int anno, int mese, List<Dip_GG_ResultModel> results)
        {
            var periodo = _dip_PeriodoRepository.FindAll(p => p.IdDip_RapportoLavoro == rapporto.Id && p.Anno == anno && p.Mese == mese).FirstOrDefault()
                          ?? new Dip_Periodo { IdDip_RapportoLavoro = rapporto.Id, Anno = anno, Mese = mese };

            if (periodo.Stato == PeriodoStato.Approvato)
                return;

            var inizioRapporto = rapporto.DataAss!.Value.Date;
            var fineRapporto = rapporto.DataLic?.Date;
            var giorni = results.Where(r => r.Data.Year == anno && r.Data.Month == mese &&
                                            r.Data.Date >= inizioRapporto && (fineRapporto == null || r.Data.Date <= fineRapporto.Value))
                                .ToList();

            int Qualificate(Dip_GG_ResultModel r) => r.Min_Suppl + r.Min_SupplEcc + r.Min_Strao + r.Min_StraoNott + r.Min_StraoFest + r.Min_StraoFestNott + r.Min_EccOrd;

            periodo.Min_Teo = giorni.Sum(r => (int)r.HH_Teo.ToTimeSpan().TotalMinutes);
            periodo.Min_Lav = giorni.Sum(r => (int)r.HH_Lav.ToTimeSpan().TotalMinutes);
            periodo.Min_Ord = giorni.Sum(r => r.Min_Ord);
            periodo.Min_Assenze = giorni.Sum(r => r.Min_Assenze);
            periodo.Min_Ecc = giorni.Sum(r => Math.Max(0, r.Min_Ecc));
            periodo.Min_Provvisorie = giorni.Where(r => r.EccProvvisoria).Sum(r => Math.Max(0, r.Min_Ecc));
            periodo.Min_Suppl = giorni.Sum(r => r.Min_Suppl);
            periodo.Min_SupplEcc = giorni.Sum(r => r.Min_SupplEcc);
            periodo.Min_Strao = giorni.Sum(r => r.Min_Strao);
            periodo.Min_StraoNott = giorni.Sum(r => r.Min_StraoNott);
            periodo.Min_StraoFest = giorni.Sum(r => r.Min_StraoFest);
            periodo.Min_StraoFestNott = giorni.Sum(r => r.Min_StraoFestNott);
            periodo.Min_EccOrd = giorni.Sum(r => r.Min_EccOrd);
            periodo.Min_NonRiconosciute = giorni.Sum(r => r.Min_NonRiconosciute);
            periodo.Min_BancaOre = giorni.Where(r => r.BancaOre).Sum(Qualificate);
            periodo.Min_NonAutorizzate = giorni.Sum(r => r.Min_NonAutorizzate);
            periodo.Min_DaAutorizzare = giorni.Sum(r => r.Min_DaAutorizzare);
            periodo.Min_PausaDetratta = giorni.Sum(r => r.Min_PausaDetratta);
            periodo.Min_Compensate = Math.Max(0, periodo.Min_Ecc - periodo.Min_Provvisorie - giorni.Sum(Qualificate) - periodo.Min_NonRiconosciute);

            // il mese e' concluso quando e' passato l'ultimo giorno (con il conteggio settimanale: la domenica della sua ultima settimana)
            var ultimo = new DateTime(anno, mese, 1).AddMonths(1).AddDays(-1);
            var fine = ultimo;
            var profilo = ProfiloDelGiorno(orariSchema, rapporto.Id, ultimo);
            if (profilo != null && profilo.EccedenzeAttive && profilo.TipoConteggio == StraoTipoConteggio.Settimanale)
                fine = GestionePresenzeEccedenze.InizioSettimana(ultimo).AddDays(6);

            // provvisorio anche con autorizzazioni allo straordinario ancora da approvare
            periodo.Stato = (fine >= DateTime.Today || giorni.Any(r => r.EccProvvisoria || r.Min_DaAutorizzare > 0))
                            ? PeriodoStato.Provvisorio
                            : PeriodoStato.DaApprovare;

            await _dip_PeriodoRepository.UpsertAsync(periodo);
        }

        private async Task CalcolaGiorno(Dip_AnagraficaModel Dip_Anagrafica, TimeSheet_CalculateModel timeSheet_CalculateModel, Dip_RapportoLavoroModel rapporto_calc, DateTime giorno, Timesheet_AllData_OutModel AllData, bool conOpzioni)
        {
            var dip_GG_Result = AllData.Dip_GG_AllData_OutModel.Dip_GG_Result.FirstOrDefault(x => x.IdDip_RapportoLavoro == rapporto_calc.Id &&
                                                                                                 x.Data.Date == giorno.Date);

            // giorno di un mese approvato: resta com'e'
            if (dip_GG_Result != null && dip_GG_Result.Stato.HasFlag(GG_ResultStato.Locked))
                return;

            // le opzioni del calcolo valgono solo nel periodo richiesto, non nei giorni ricalcolati per completare il periodo di conteggio
            var idGiustAutorizzazione = AllData.OrariSchema_4User_OutModel.Par_Giustificativi.Where(pg => pg.AutorizzaStraordinario).Select(pg => pg.Id).ToList();

            if (conOpzioni && timeSheet_CalculateModel.Approva_Richieste_Giustificativo)
            {
                await this.ApprovaRichiesta(TipoRichiesta.Giustificativo, AllData.Dip_GG_AllData_OutModel, rapporto_calc.Id, giorno, idGiustAutorizzazione);
            }

            if (conOpzioni && timeSheet_CalculateModel.Approva_Richieste_Timbrature)
            {
                await this.ApprovaRichiesta(TipoRichiesta.Timbratura, AllData.Dip_GG_AllData_OutModel, rapporto_calc.Id, giorno, idGiustAutorizzazione);
            }

            if (conOpzioni && timeSheet_CalculateModel.Genera_Timbrature_Mancanti)
            {
                GeneraTimbratureMancanti(AllData, rapporto_calc.Id, rapporto_calc.IdAz_SubCommessaAttivita, giorno);
            }

            GG_ResultStato Calcolo1Result = GG_ResultStato.OK;
            GG_ResultStato Calcolo2Result = GG_ResultStato.OK;

            GG_ResultStato VersoTimbratureResult = AssegnaVersoTimbrature(AllData, rapporto_calc.Id, giorno);

            if (dip_GG_Result != null)
            {
                dip_GG_Result.HH_Teo = TimeOnly.FromTimeSpan(this.CalcolaOreTeoriche(AllData, rapporto_calc.Id, giorno));
                dip_GG_Result.HH_Lav = TimeOnly.FromTimeSpan(this.CalcolaOreLavorate(AllData, rapporto_calc.Id, giorno));

                /* genera il giustificativo di assenza che farà quadrare la giornata (non è detto che questa funzione sia attiva) */
                if (conOpzioni && timeSheet_CalculateModel.Genera_Giustificativo_Assenza)
                {
                    await GeneraGiustificativoAssenza(Dip_Anagrafica, AllData, rapporto_calc.Id, giorno, dip_GG_Result);
                }

                Calcolo2Result = GeneraCausali(Dip_Anagrafica, AllData, rapporto_calc.Id, giorno, dip_GG_Result);

                // formule della fase Giorno: dopo le causali del giorno, prima della qualifica di periodo
                if (_causaliGiorno.TryGetValue((rapporto_calc.Id, giorno.Date), out var causaliFormule))
                {
                    if (!_importiGiorno.TryGetValue((rapporto_calc.Id, giorno.Date), out var importiFormule))
                        _importiGiorno[(rapporto_calc.Id, giorno.Date)] = importiFormule = new Dictionary<int, decimal>();
                    var statoFormule = _formuleRunner.Esegui(FormulaFase.Giorno, DatiFormule(Dip_Anagrafica, rapporto_calc, giorno, FormulaFase.Giorno, AllData, dip_GG_Result, causaliFormule, importiFormule));
                    AggiungiStatoFormule(ref Calcolo2Result, statoFormule);
                }

                dip_GG_Result.Stato = Dip_GG_Result_Helper.Combine(Calcolo1Result,
                                                                    Calcolo2Result,
                                                                    VersoTimbratureResult);
            }

            return;
        }
        private async Task SaveData(Timesheet_AllData_OutModel AllData)
        {
            #region "Result"
            foreach (var item in AllData.Dip_GG_AllData_OutModel.Dip_GG_Result)
            {
                if (item.IsHashChanged(item.Hash))
                {
                    var req_1 = new GenericRequest<Dip_GG_ResultPutInModel>();
                    req_1.Data.Dip_GG_Result = item;
                    req_1.Data.ExcludeRicalc = true;
                    req_1.Data.IdDip_RapportoLavoro = item.IdDip_RapportoLavoro;
                    await _dip_GG_ResultService.Dip_GG_ResultPut(req_1, true);
                }
            }
            #endregion

            #region "Timbrature"
            foreach (var item in AllData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura)
            {
                if (item.IsHashChanged(item.Hash))
                {
                    if (!item.ToBeDeleted)
                    {
                        var req_1 = new GenericRequest<Dip_GG_TimbraturaPutInModel>();
                        req_1.Data.Dip_GG_Timbratura = item;
                        req_1.Data.ExcludeRicalc = true;
                        await _dip_GG_TimbraturaService.Dip_GG_TimbraturaPut(req_1, true);
                    }
                    else
                    {
                        //to do
                    }
                }
            }
            #endregion

            #region "Giustificativi"
            foreach (var item in AllData.Dip_GG_AllData_OutModel.Dip_GG_Giustificativi)
            {
                if (item.IsHashChanged(item.Hash))
                {
                    if (!item.ToBeDeleted)
                    {
                        var req_1 = new GenericRequest<Dip_GG_GiustificativiPutInModel>();
                        req_1.Data.Dip_GG_Giustificativi = item;
                        req_1.Data.IdDip_RapportoLavoro = item.IdDip_RapportoLavoro;
                        req_1.Data.ExcludeRicalc = true;
                        await _dip_GG_GiustificativiService.Dip_GG_GiustificativiPut(req_1, true);
                    }
                    else
                    {
                        var req_1 = new GenericRequest<Dip_GG_Giustificativi_DeleteInModel>();
                        req_1.Data.Id = item.Id;
                        req_1.Data.ExcludeRicalc = true;
                        await _dip_GG_GiustificativiService.Dip_GG_GiustificativiDelete(req_1, true);
                    }
                }
            }
            #endregion

            #region "Richieste"
            foreach (var item in AllData.Dip_GG_AllData_OutModel.Dip_GG_Richiesta)
            {
                if (item.IsHashChanged(item.Hash))
                {
                    if (!item.ToBeDeleted)  // egstisco solo quelle da cancellare
                    {
                        var req_1 = new GenericRequest<Dip_GG_RichiestaPutInModel>();
                        req_1.Data.Dip_GG_Richiesta = item;
                        req_1.Data.ExcludeRicalc = true;
                        await _dip_GG_RichiestaService.Dip_GG_RichiestaPut(req_1, true);
                    }
                    else
                    {
                        var req_1 = new GenericRequest<Dip_GG_Richiesta_DeleteInModel>();
                        req_1.Data.Id = item.Id;
                        req_1.Data.ExcludeRicalc = true;
                        await _dip_GG_RichiestaService.Dip_GG_RichiestaDelete(req_1, true);
                    }
                }
            }
            #endregion

            #region "Causali"
            foreach (var item in AllData.Dip_GG_AllData_OutModel.Dip_GG_Causali)
            {
                if (item.IsHashChanged(item.Hash))
                {
                    if (!item.ToBeDeleted)
                    {
                        var req_1 = new GenericRequest<Dip_GG_CausaliPutInModel>();
                        req_1.Data.Dip_GG_Causali = item;
                        req_1.Data.ExcludeRicalc = true;
                        await _dip_GG_CausaliService.Dip_GG_CausaliPut(req_1, true);
                    }
                    else
                    {
                        var req_1 = new GenericRequest<Dip_GG_Causali_DeleteInModel>();
                        req_1.Data.Id = item.Id;
                        req_1.Data.ExcludeRicalc = true;
                        await _dip_GG_CausaliService.Dip_GG_CausaliDelete(req_1, true);
                    }
                }
            }
            #endregion

            // log delle formule dei giorni ricalcolati
            await _formuleRunner.SalvaLogAsync();

            return;
        }

        #region "Formule personalizzate (ANALISI_ATTENDANCETRACKING_FORMULE.md)"

        private static void AggiungiStatoFormule(ref GG_ResultStato stato, GG_ResultStato statoFormule)
        {
            if (statoFormule.HasFlag(FormuleRunner.StatoWarningFormula))
                Dip_GG_Result_Helper.AddDetail(ref stato, FormuleRunner.StatoWarningFormula);
            if (statoFormule.HasFlag(FormuleRunner.StatoAnomaliaFormula))
                Dip_GG_Result_Helper.AddDetail(ref stato, FormuleRunner.StatoAnomaliaFormula);
        }

        // dati del giorno per la facciata "x" delle formule: tutto in memoria, nessuna lettura dal DB
        private static FormulaDatiGiorno DatiFormule(Dip_AnagraficaModel? anagrafica, Dip_RapportoLavoroModel rapporto, DateTime giorno, FormulaFase fase,
                                                     Timesheet_AllData_OutModel allData, Dip_GG_ResultModel? result,
                                                     Dictionary<int, TimeSpan> causali, Dictionary<int, decimal> importi)
        {
            var schema = allData.OrariSchema_4User_OutModel;
            var dati = allData.Dip_GG_AllData_OutModel;
            var daySlot = schema.DaySlots.FirstOrDefault(ds => ds.IdDip_RapportoLavoro == rapporto.Id && ds.Data.Date == giorno.Date);
            var orarioBase = daySlot?.Orari.OrderBy(o => o.ZOrder).FirstOrDefault();
            var parOrario = orarioBase != null ? schema.ParOrario.FirstOrDefault(o => o.Id == orarioBase.IdPar_Orario) : null;

            return new FormulaDatiGiorno
            {
                Anagrafica = anagrafica,
                Rapporto = rapporto,
                Giorno = giorno.Date,
                Fase = fase,
                Result = result,
                Causali = causali,
                Importi = importi,
                Timbrature = dati.Dip_GG_Timbratura.Where(t => t.IdDip_RapportoLavoro == rapporto.Id && t.GiornoCompetenza.Date == giorno.Date &&
                                                               (t.RichiestaStato == StatoRichiesta.Diretta || t.RichiestaStato == StatoRichiesta.Approvata) &&
                                                               !t.ToBeDeleted)
                                                   .ToList(),
                Giustificativi = dati.Dip_GG_Giustificativi.Where(gg => gg.IdDip_RapportoLavoro == rapporto.Id && gg.Data.Date == giorno.Date &&
                                                                        (gg.RichiestaStato == StatoRichiesta.Diretta || gg.RichiestaStato == StatoRichiesta.Approvata) &&
                                                                        !gg.ToBeDeleted)
                                                           .ToList(),
                Orario = parOrario,
                Fasce = parOrario != null ? schema.Par_OrarioIntervalloHH.Where(hh => hh.IdPar_Orario == parOrario.Id).ToList() : new(),
                Pause = parOrario != null ? schema.Par_OrarioPausa.Where(p => p.IdPar_Orario == parOrario.Id).ToList() : new(),
                Profilo = daySlot != null ? schema.Par_ProfiloOrario.FirstOrDefault(p => p.Id == daySlot.IdPar_ProfiloOrario) : null,
                Festivo = daySlot?.Festivo ?? false
            };
        }

        /// <summary>
        /// "Prova formula": calcola un giorno di un rapporto eseguendo solo la formula indicata (testo non salvato),
        /// senza scrivere nulla. Restituisce le causali prima e dopo la formula e il log (anche di debug).
        /// </summary>
        public virtual async Task<FormulaProvaRisultato> SimulaFormula(string idAspNetUsers, int idDip_RapportoLavoro, DateTime giorno, string testo, FormulaFase fase, int idAz_Anagrafica)
        {
            _simulazione = true;
            _formuleRunner.ImpostaProva(testo, fase, giorno.Date, idAz_Anagrafica);

            var calc = new TimeSheet_CalculateModel
            {
                Year = giorno.Year,
                Month = giorno.Month,
                Dal = giorno.Date,
                Al = giorno.Date,
                SelectedUserId = new List<string> { idAspNetUsers }
            };
            var caricamento = RangeCaricamento(calc.Dal, calc.Al);
            var req = new GenericRequest<Timesheet_AllData_InModel>();
            req.Data.Dal = caricamento.Dal;
            req.Data.Al = caricamento.Al;
            req.Data.UsersId = calc.SelectedUserId;

            var allData = await Get_Timesheet_AllData(req, true);
            if (allData.Success && allData.Data != null)
            {
                var anagrafica = allData.Data.OrariSchema_4User_OutModel.Dip_Anagrafica.FirstOrDefault(a => a.IdAspNetUsers == idAspNetUsers);
                var rapporto = allData.Data.OrariSchema_4User_OutModel.Dip_RapportoLavoro.FirstOrDefault(r => r.Id == idDip_RapportoLavoro);
                if (anagrafica != null && rapporto != null)
                    await CalcolaRapporto(anagrafica, calc, rapporto, allData.Data);
            }
            return _formuleRunner.Prova ?? new FormulaProvaRisultato();
        }

        #endregion
        // le autorizzazioni alle ore in piu' non si approvano mai in automatico dal calcolo: le decide l'approvatore
        private static bool IsAutorizzazioneStraordinario(Dip_GG_RichiestaModel richiesta, List<int> idGiustAutorizzazione)
        {
            if (richiesta.RichiestaTipo != TipoRichiesta.Giustificativo || idGiustAutorizzazione.Count == 0 || string.IsNullOrEmpty(richiesta.Dati))
                return false;
            try
            {
                var body = JsonSerializer.Deserialize<Dip_GG_Richiesta_Body_Giustificativo>(richiesta.Dati, new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
                return body != null && idGiustAutorizzazione.Contains(body.IdPar_Giustificativi);
            }
            catch (JsonException)
            {
                return false;
            }
        }

        private async Task ApprovaRichiesta(TipoRichiesta tipoRichiesta, Dip_GG_AllData_OutModel dip_GG_AllData, int IdDip_RapportoLavoro, DateTime day, List<int> idGiustAutorizzazione)
        {


            var req_1 = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>();

            req_1.Data.RichiestaStato = StatoRichiesta.Approvata;
            req_1.Data.FromHR = true;
            req_1.Data.IdDip_GG_Richiesta = dip_GG_AllData.Dip_GG_Richiesta.Where(x => x.RichiestaTipo == tipoRichiesta &&
                                                                                    (x.RichiestaStato == StatoRichiesta.Immessa || x.RevocaStato == StatoRichiesta.Immessa) &&
                                                                                    x.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                                    x.Data == x.DataA &&
                                                                                    DateTime.Parse(x.Data) == day &&
                                                                                    !IsAutorizzazioneStraordinario(x, idGiustAutorizzazione))
                                                                           .Select(x => x.Id)
                                                                           .ToList();

            if (req_1.Data.IdDip_GG_Richiesta.Count == 0)
                return;

            var res_1 = await _dip_GG_RichiestaService.SetState(req_1, true);
            if (res_1.Success && res_1.Data != null)
                Merge_RichiestaService_Result(dip_GG_AllData, res_1.Data.Dip_GG_Richiesta, res_1.Data.Dip_GG_Timbratura, res_1.Data.Dip_GG_Giustificativi);

        }

        /* aggiorna i valori che il servizio ha aggiornato in autonomia */
        private void Merge_RichiestaService_Result(Dip_GG_AllData_OutModel dip_GG_AllData,
                                                   List<Dip_GG_RichiestaModel> Dip_GG_Richiesta,
                                                   List<Dip_GG_TimbraturaModel> Dip_GG_Timbratura,
                                                   List<Dip_GG_GiustificativiModel> Dip_GG_Giustificativi
                                                   )
        {
            foreach (var item in Dip_GG_Richiesta)
            {
                var idx = dip_GG_AllData.Dip_GG_Richiesta.FindIndex(x => x.Id == item.Id);
                if (idx == -1)
                    dip_GG_AllData.Dip_GG_Richiesta.Add(item);   // ← non esiste: aggiunge item (non tmp)
                else
                    dip_GG_AllData.Dip_GG_Richiesta[idx] = item; // ← esiste: sostituisce per indice
            }

            foreach (var item in Dip_GG_Timbratura)
            {
                var idx = dip_GG_AllData.Dip_GG_Timbratura.FindIndex(x => x.Id == item.Id);
                if (idx == -1)
                    dip_GG_AllData.Dip_GG_Timbratura.Add(item);
                else
                    dip_GG_AllData.Dip_GG_Timbratura[idx] = item;
            }

            foreach (var item in Dip_GG_Giustificativi)
            {
                var idx = dip_GG_AllData.Dip_GG_Giustificativi.FindIndex(x => x.Id == item.Id);
                if (idx == -1)
                    dip_GG_AllData.Dip_GG_Giustificativi.Add(item);
                else
                    dip_GG_AllData.Dip_GG_Giustificativi[idx] = item;
            }
        }


        private TimeSpan CalcolaOreTeoriche(Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day)
        {
            var oreTeoriche = TimeSpan.Zero;

            var orariSchema = allData.OrariSchema_4User_OutModel;

            var daySlot = orariSchema.DaySlots
                .FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro
                                   && ds.Data.Date == day.Date);

            if (daySlot == null || daySlot.Orari.Count == 0)
                return TimeSpan.Zero;

            var orarioBase = daySlot.Orari.OrderBy(o => o.ZOrder).First();

            var parOrario = orariSchema.ParOrario
                .FirstOrDefault(o => o.Id == orarioBase.IdPar_Orario);

            if (parOrario == null)
                return TimeSpan.Zero;

            if (parOrario.TimbratureTipo == OrarioTimbratureTipo.IntervalloOrario)
            {
                var coppie = orariSchema.Par_OrarioIntervalloHH.Where(hh => hh.IdPar_Orario == parOrario.Id)
                                                               .OrderBy(hh => hh.NumCoppia)
                                                               .ToList();

                if (coppie.Count == 0)
                    return TimeSpan.Zero;


                foreach (var coppia in coppie)
                {
                    if (coppia.Dalle.HasValue && coppia.Alle.HasValue)
                    {
                        var durataCoppia = coppia.Alle.Value.ToTimeSpan() - coppia.Dalle.Value.ToTimeSpan();
                        if (durataCoppia > TimeSpan.Zero)
                            oreTeoriche += durataCoppia;
                    }
                }

                // pausa pranzo: dentro una coppia toglie la parte non retribuita, nello stacco aggiunge quella retribuita
                var pause = orariSchema.Par_OrarioPausa.Where(p => p.IdPar_Orario == parOrario.Id).ToList();
                oreTeoriche += GestionePresenzePause.VariazioneTeorico(day, oreTeoriche, coppie, pause);
                if (oreTeoriche < TimeSpan.Zero)
                    oreTeoriche = TimeSpan.Zero;
            }
            else if (parOrario.TimbratureTipo == OrarioTimbratureTipo.MonteOre || parOrario.TimbratureTipo == OrarioTimbratureTipo.MonteOreValore)
            {
                oreTeoriche = parOrario.Hh_Teo_MonteOre.ToTimeSpan();
            }



            return oreTeoriche;
        }
        /// <summary>
        /// Segmenti lavorati del giorno su orario a coppie: entrata/uscita arrotondate, in ordine di timbratura,
        /// al netto della pausa (detrazione automatica, riaccredito dei minuti retribuiti).
        /// Base comune di ore lavorate e causali (ANALISI_ATTENDANCETRACKING_PAUSE.md 6.4).
        /// </summary>
        private static List<(DateTime Dal, DateTime Al)> SegmentiLavorati(Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day,
                                                                         Par_OrarioModel parOrario, List<Dip_GG_TimbraturaModel> timbrature,
                                                                         out GestionePresenzePause.Esito esito)
        {
            var segmenti = new List<(DateTime Dal, DateTime Al)>();
            var ordinate = timbrature.OrderBy(t => t.Timbratura).ToList();
            for (int i = 0; i + 1 < ordinate.Count;)
            {
                var entrata = ordinate[i];
                var uscita = ordinate[i + 1];
                if (entrata.TimbraturaTipo != TipoTimbratura.Entrata || uscita.TimbraturaTipo != TipoTimbratura.Uscita)
                {
                    i++;
                    continue;
                }
                if (entrata.TimbraturaArrotondata.HasValue && uscita.TimbraturaArrotondata.HasValue &&
                    uscita.TimbraturaArrotondata.Value > entrata.TimbraturaArrotondata.Value)
                    segmenti.Add((entrata.TimbraturaArrotondata.Value, uscita.TimbraturaArrotondata.Value));
                i += 2;
            }

            var orariSchema = allData.OrariSchema_4User_OutModel;
            var coppie = orariSchema.Par_OrarioIntervalloHH.Where(hh => hh.IdPar_Orario == parOrario.Id).OrderBy(hh => hh.NumCoppia).ToList();
            var pause = orariSchema.Par_OrarioPausa.Where(p => p.IdPar_Orario == parOrario.Id).ToList();

            // giustificativo "pausa non fruita" nel giorno: niente detrazione e niente avviso di pausa insufficiente
            var idAnnulla = orariSchema.Par_Giustificativi.Where(g => g.AnnullaPausa).Select(g => g.Id).ToList();
            bool annullata = idAnnulla.Count > 0 &&
                             allData.Dip_GG_AllData_OutModel.Dip_GG_Giustificativi.Any(g => g.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                                           g.Data.Date == day.Date && !g.ToBeDeleted &&
                                                                                           (g.RichiestaStato == StatoRichiesta.Diretta || g.RichiestaStato == StatoRichiesta.Approvata) &&
                                                                                           idAnnulla.Contains(g.IdPar_Giustificativi));

            return GestionePresenzePause.Applica(day, segmenti, coppie, pause, annullata, out esito);
        }

        private TimeSpan CalcolaOreLavorate(Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day)
        {
            var oreLavorate = TimeSpan.Zero;

            var orariSchema = allData.OrariSchema_4User_OutModel;

            var daySlot = orariSchema.DaySlots
                .FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro
                                   && ds.Data.Date == day.Date);

            if (daySlot == null || daySlot.Orari.Count == 0)
                return TimeSpan.Zero;

            var orarioBase = daySlot.Orari.OrderBy(o => o.ZOrder).First();

            var parOrario = orariSchema.ParOrario
                .FirstOrDefault(o => o.Id == orarioBase.IdPar_Orario);

            if (parOrario == null)
                return TimeSpan.Zero;

            var timbrature = Get_Timbrature_Giorno(allData, IdDip_RapportoLavoro, day, parOrario.TimbratureTipo);

            if (parOrario.TimbratureTipo == OrarioTimbratureTipo.IntervalloOrario)
            {
                foreach (var (dal, al) in SegmentiLavorati(allData, IdDip_RapportoLavoro, day, parOrario, timbrature, out _))
                    oreLavorate += al - dal;
            }
            else if (parOrario.TimbratureTipo == OrarioTimbratureTipo.MonteOre || parOrario.TimbratureTipo == OrarioTimbratureTipo.MonteOreValore)
            {
                for (int i = 0; i < timbrature.Count; i++)
                {
                    if (timbrature[i].Timbratura is DateTime arrotondata)
                        oreLavorate += arrotondata.TimeOfDay;
                }
            }



            return oreLavorate;
        }
        private GG_ResultStato AssegnaVersoTimbrature(Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day)
        {

            GG_ResultStato retVal = GG_ResultStato.OK;
            Dip_GG_Result_Helper.SetState(ref retVal, GG_ResultStato.OK);

            var orariSchema = allData.OrariSchema_4User_OutModel;

            // recupera le coppie solo per le TimeRoundOptions di arrotondamento
            var daySlot = orariSchema.DaySlots
                .FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro
                                   && ds.Data.Date == day.Date);

            if (daySlot == null || daySlot.Orari.Count == 0)
                return retVal;

            var orarioBase = daySlot.Orari.OrderBy(o => o.ZOrder).First();
            var parOrario = orariSchema.ParOrario.FirstOrDefault(o => o.Id == orarioBase.IdPar_Orario);
            if (parOrario == null)
                return retVal;


            if (parOrario.TimbratureTipo == OrarioTimbratureTipo.IntervalloOrario)
            {
                var coppie = orariSchema.Par_OrarioIntervalloHH
                    .Where(hh => hh.IdPar_Orario == parOrario.Id)
                    .OrderBy(hh => hh.NumCoppia)
                    .ToList();

                var pause = orariSchema.Par_OrarioPausa.Where(p => p.IdPar_Orario == parOrario.Id).ToList();

                // tutte le timbrature del giorno ripartono senza verso; si interpretano solo quelle valide
                var delGiorno = allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura.Where(t => t.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                                             t.GiornoCompetenza.Date == day.Date)
                                                                                 .ToList();
                foreach (var t in delGiorno)
                    t.TimbraturaTipo = TipoTimbratura.SenzaVerso;

                var valide = delGiorno.Where(t => !t.ToBeDeleted &&
                                                  (t.RichiestaStato == StatoRichiesta.Diretta || t.RichiestaStato == StatoRichiesta.Approvata))
                                      .OrderBy(t => t.Timbratura)
                                      .ToList();

                // interpretazione con gli intervalli di riconoscimento (ANALISI_ATTENDANCETRACKING_PAUSE.md 3.3):
                // ancore dentro l'intervallo, poi fuori intervallo nella zona adiacente, uscite intermedie dentro le coppie,
                // lavoro fuori orario prima e dopo; la pausa automatica rende facoltative le timbrature dello stacco
                var ancore = GestionePresenzeSequenza.Ancore(day, coppie);
                var esito = GestionePresenzeSequenza.Interpreta(valide.Select(t => t.Timbratura).ToList(), ancore,
                                                                AncoreFacoltative(day, ancore, coppie, pause));

                if (esito.TimbratureMancanti)
                {
                    Dip_GG_Result_Helper.AddDetail(ref retVal, GG_ResultStato.Err_TimbratureMancanti);
                    return retVal;
                }

                for (int i = 0; i < valide.Count; i++)
                {
                    valide[i].TimbraturaTipo = esito.Verso[i];
                    if (esito.Verso[i] != TipoTimbratura.SenzaVerso)
                        valide[i].TimbraturaArrotondata = GestionePresenzeSequenza.Arrotonda(valide[i].Timbratura,
                                                                  esito.IndiceAncora[i] >= 0 ? ancore[esito.IndiceAncora[i]] : null);
                }

                // Err_5: sequenza non interpretabile (uscita senza rientro); Warning_5: doppia o ancora fuori intervallo
                if (esito.NonInterpretabile)
                    Dip_GG_Result_Helper.AddDetail(ref retVal, GG_ResultStato.Err_5);
                if (esito.Avviso)
                    Dip_GG_Result_Helper.AddDetail(ref retVal, GG_ResultStato.Warning_5);


            }
            else if (parOrario.TimbratureTipo == OrarioTimbratureTipo.MonteOre || parOrario.TimbratureTipo == OrarioTimbratureTipo.MonteOreValore)
            {

            }




            return retVal;

        }
        // uscita k / entrata k+1 facoltative: pausa pranzo automatica nello stacco tra le due coppie
        private static List<(int Uscita, int Entrata)> AncoreFacoltative(DateTime day, List<GestionePresenzeSequenza.Ancora> ancore,
                                                                         List<Par_OrarioIntervalloHHModel> coppie, List<Par_OrarioPausaModel> pause)
        {
            var retVal = new List<(int Uscita, int Entrata)>();
            var pranzo = GestionePresenzePause.Pranzo(pause);
            if (pranzo == null || pranzo.Modalita != PausaModalita.Automatica)
                return retVal;
            var finestra = GestionePresenzePause.Finestra(day, pranzo, coppie);
            if (finestra == null)
                return retVal;
            for (int k = 0; k + 1 < ancore.Count; k++)
                if (!ancore[k].Entrata && ancore[k + 1].Entrata &&
                    GestionePresenzePause.Sovrapposizione(ancore[k].Ora, ancore[k + 1].Ora, finestra.Value.Dal, finestra.Value.Al) > TimeSpan.Zero)
                    retVal.Add((k, k + 1));
            return retVal;
        }

        private void GeneraTimbratureMancanti(Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, int IdAz_SubCommessaAttivita, DateTime day)
        {
            var orariSchema = allData.OrariSchema_4User_OutModel;

            if (day.Date >= DateTime.Today)
                return;

            var daySlot = orariSchema.DaySlots.FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                    ds.Data.Date == day.Date);

            if (daySlot == null || daySlot.Orari.Count == 0)
                return;

            var orarioBase = daySlot.Orari.OrderBy(o => o.ZOrder).First();

            var parOrario = orariSchema.ParOrario
                .FirstOrDefault(o => o.Id == orarioBase.IdPar_Orario);

            if (parOrario == null)
                return;

            var timbratureEsistenti = allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura.Where(t => t.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                                                   t.GiornoCompetenza.Date == day.Date)
                                                                                       .ToList();

            if (parOrario.TimbratureTipo == OrarioTimbratureTipo.IntervalloOrario)
            {
                var coppie = orariSchema.Par_OrarioIntervalloHH.Where(hh => hh.IdPar_Orario == parOrario.Id)
                                                        .OrderBy(hh => hh.NumCoppia)
                                                        .ToList();

                if (coppie.Count == 0)
                    return;

                // stessa interpretazione del calcolo: si generano solo le ore attese senza timbratura,
                // non quelle dello stacco con pausa automatica (ANALISI_ATTENDANCETRACKING_PAUSE.md 6.3)
                var pause = orariSchema.Par_OrarioPausa.Where(p => p.IdPar_Orario == parOrario.Id).ToList();
                var ancore = GestionePresenzeSequenza.Ancore(day, coppie);
                var esistenti = timbratureEsistenti.Where(t => !t.ToBeDeleted)
                                                   .OrderBy(t => t.Timbratura)
                                                   .Select(t => t.Timbratura)
                                                   .ToList();
                var esito = GestionePresenzeSequenza.Interpreta(esistenti, ancore, AncoreFacoltative(day, ancore, coppie, pause));

                foreach (var k in esito.AncoreVuote.Where(k => !esito.VuoteAmmesse.Contains(k)))
                {
                    var entity = new Dip_GG_Timbratura
                    {
                        IdDip_RapportoLavoro = IdDip_RapportoLavoro,
                        IdAz_SubCommessaAttivita = IdAz_SubCommessaAttivita //nvx 27/04/2026
                    };
                    var vm = _mapper.Map<Dip_GG_TimbraturaModel>(entity);
                    vm.Timbratura = ancore[k].Ora;
                    vm.TimbraturaOriginale = ancore[k].Ora;
                    vm.GiornoCompetenza = day.Date;
                    vm.TimbraturaTipo = TipoTimbratura.SenzaVerso;
                    vm.RichiestaStato = StatoRichiesta.Diretta;
                    allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura.Add(vm);
                }


            }
            else if (parOrario.TimbratureTipo == OrarioTimbratureTipo.MonteOre || parOrario.TimbratureTipo == OrarioTimbratureTipo.MonteOreValore)
            {
                if (timbratureEsistenti.Count == 0)
                {
                    var entity = new Dip_GG_Timbratura { IdDip_RapportoLavoro = IdDip_RapportoLavoro, IdAz_SubCommessaAttivita = IdAz_SubCommessaAttivita };
                    var vm = _mapper.Map<Dip_GG_TimbraturaModel>(entity);
                    vm.Timbratura = day.Date + (parOrario.Hh_Teo_MonteOre).ToTimeSpan();
                    vm.TimbraturaOriginale = day.Date + (parOrario.Hh_Teo_MonteOre).ToTimeSpan();
                    vm.TimbraturaArrotondata = day.Date + (parOrario.Hh_Teo_MonteOre).ToTimeSpan();
                    vm.GiornoCompetenza = day.Date;
                    vm.TimbraturaTipo = TipoTimbratura.SenzaVerso;
                    vm.RichiestaStato = StatoRichiesta.Diretta;
                    allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura.Add(vm);
                }
            }




        }
        //private async Task GeneraGiustificativoAssenza(Dip_AnagraficaModel Dip_Anagrafica, Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day, Dip_GG_ResultModel dip_GG_Result)
        //{

        //    if (day.Date >= DateTime.Today)
        //        return;

        //    var orariSchema = allData.OrariSchema_4User_OutModel;

        //    if (orariSchema == null)
        //        return;

        //    var daySlot = orariSchema.DaySlots.FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
        //                                                            ds.Data.Date == day.Date);

        //    if (daySlot == null)
        //        return;

        //    Par_ProfiloOrarioModel? par_ProfiloOrario = orariSchema.Par_ProfiloOrario.Where(x => x.Id == daySlot.IdPar_ProfiloOrario).FirstOrDefault();

        //    if (par_ProfiloOrario == null)
        //        return;

        //    if (par_ProfiloOrario.IdGiustificativo_Assenza_Ingiust > 0)
        //    {
        //        var dip_GG_Giustificativi = allData.Dip_GG_AllData_OutModel.Dip_GG_Giustificativi.FindAll(x => x.IdPar_Giustificativi == par_ProfiloOrario.IdGiustificativo_Assenza_Ingiust &&
        //                                                                                                       x.Data == day).FirstOrDefault();

        //        if (dip_GG_Result.HH_Teo.Ticks > 0)
        //        {
        //            bool justIsRequired = false;
        //            TimeSpan diff = new TimeSpan();

        //            if (dip_GG_Result.HH_Teo > dip_GG_Result.HH_Lav)
        //            {
        //                justIsRequired = true;
        //                diff = dip_GG_Result.HH_Teo - dip_GG_Result.HH_Lav;
        //            }


        //            if (dip_GG_Giustificativi == null)
        //            {
        //                if (justIsRequired)
        //                {
        //                    // necessario ed assente, lo aggiungo con il service delle richieste
        //                    Dip_GG_Richiesta_Body_Giustificativo dip_GG_Richiesta_Body_Giustificativo = new Dip_GG_Richiesta_Body_Giustificativo()
        //                    {
        //                        IdPar_Giustificativi = par_ProfiloOrario.IdGiustificativo_Assenza_Ingiust,
        //                        AllDay = false,
        //                        hhmm = diff.ToString(@"hh\:mm"),
        //                    };

        //                    var req_1 = new GenericRequest<Dip_GG_Richiesta_Send_InModel>();

        //                    req_1.Data.IdAspNetUsers = Dip_Anagrafica.IdAspNetUsers;
        //                    req_1.Data.FromHR = true;
        //                    req_1.Data.Dip_GG_Richiesta = new Dip_GG_RichiestaModel()
        //                    {
        //                        Id = 0,
        //                        IdDip_RapportoLavoro = IdDip_RapportoLavoro,
        //                        Dati = JsonSerializer.Serialize(dip_GG_Richiesta_Body_Giustificativo)
        //                    };
        //                    req_1.Data.Dip_GG_Richiesta.RichiestaStato = StatoRichiesta.Approvata;
        //                    req_1.Data.Dip_GG_Richiesta.RichiestaTipo = TipoRichiesta.Giustificativo;
        //                    req_1.Data.Dip_GG_Richiesta.Data = day.ToString("dd/MM/yyyy");
        //                    req_1.Data.Dip_GG_Richiesta.DataA = day.ToString("dd/MM/yyyy");
        //                    req_1.Data.ExcludeRicalc = true;

        //                    var res_1 = await _dip_GG_RichiestaService.Send(req_1, true);
        //                    if (res_1.Success && res_1.Data != null)
        //                    {
        //                        Merge_RichiestaService_Result(allData.Dip_GG_AllData_OutModel, res_1.Data.Dip_GG_Richiesta, res_1.Data.Dip_GG_Timbratura, res_1.Data.Dip_GG_Giustificativi);
        //                    }
        //                }
        //            }
        //            else
        //            {
        //                Dip_GG_RichiestaModel? dip_GG_RichiestaModel = null;
        //                if (dip_GG_Giustificativi.IdDip_GG_Richiesta != null)
        //                    dip_GG_RichiestaModel = allData.Dip_GG_AllData_OutModel.Dip_GG_Richiesta.Where(x => x.Id == dip_GG_Giustificativi.IdDip_GG_Richiesta).FirstOrDefault();

        //                if (justIsRequired)
        //                {
        //                    if (dip_GG_Giustificativi.Hours != diff)
        //                    {
        //                        //aggiorno il giust 
        //                        dip_GG_Giustificativi.Hours = diff;

        //                        if (dip_GG_RichiestaModel != null)
        //                        {
        //                            // aggiorno la richiesta
        //                            var body = JsonSerializer.Deserialize<Dip_GG_Richiesta_Body_Giustificativo>(dip_GG_RichiestaModel.Dati);
        //                            if (body != null)
        //                            {
        //                                body.hhmm = diff.ToString(@"hh\:mm");
        //                                dip_GG_RichiestaModel.Dati = JsonSerializer.Serialize(body);
        //                            }
        //                        }
        //                    }
        //                }
        //                else
        //                {
        //                    // non server piu, cancello
        //                    dip_GG_Giustificativi.ToBeDeleted = true;
        //                    if (dip_GG_RichiestaModel != null)
        //                        dip_GG_RichiestaModel.ToBeDeleted = true;
        //                }
        //            }
        //        }
        //    }


        //}

        private async Task GeneraGiustificativoAssenza(Dip_AnagraficaModel Dip_Anagrafica, Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day, Dip_GG_ResultModel dip_GG_Result)
        {

            if (day.Date >= DateTime.Today)
                return;

            var orariSchema = allData.OrariSchema_4User_OutModel;

            if (orariSchema == null)
                return;

            var daySlot = orariSchema.DaySlots.FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                    ds.Data.Date == day.Date);

            if (daySlot == null)
                return;

            Par_ProfiloOrarioModel? par_ProfiloOrario = orariSchema.Par_ProfiloOrario.Where(x => x.Id == daySlot.IdPar_ProfiloOrario).FirstOrDefault();

            if (par_ProfiloOrario == null)
                return;

            if (par_ProfiloOrario.IdGiustificativo_Assenza_Ingiust > 0)
            {
                var dip_GG_Giustificativi = allData.Dip_GG_AllData_OutModel.Dip_GG_Giustificativi.FindAll(x => x.IdPar_Giustificativi == par_ProfiloOrario.IdGiustificativo_Assenza_Ingiust &&
                                                                                                               x.Data == day).FirstOrDefault();

                if (dip_GG_Result.HH_Teo.Ticks > 0)
                {
                    bool justIsRequired = false;
                    TimeSpan diff = new TimeSpan();

                    if (dip_GG_Result.HH_Teo > dip_GG_Result.HH_Lav)
                    {
                        justIsRequired = true;
                        diff = dip_GG_Result.HH_Teo - dip_GG_Result.HH_Lav;
                    }


                    if (dip_GG_Giustificativi == null)
                    {
                        if (justIsRequired)
                        {
                            //UTILIZZO RICHIESTA DISABILITATO, CREO GIUSTIFICATIVO
                            var dip_GG_Giustificativi_New = new Dip_GG_Giustificativi() { IdDip_RapportoLavoro = 0, IdPar_Giustificativi = 0 };
                            var entity = new Dip_GG_Giustificativi
                            {
                                IdDip_RapportoLavoro = IdDip_RapportoLavoro,
                                IdPar_Giustificativi = par_ProfiloOrario.IdGiustificativo_Assenza_Ingiust
                            };
                            var vm = _mapper.Map<Dip_GG_GiustificativiModel>(entity);
                            vm.Data = day.Date;
                            vm.Hours = diff;
                            allData.Dip_GG_AllData_OutModel.Dip_GG_Giustificativi.Add(vm);


                            //UTILIZZO RICHIESTA DISABILITATO
                            //// necessario ed assente, lo aggiungo con il service delle richieste
                            //Dip_GG_Richiesta_Body_Giustificativo dip_GG_Richiesta_Body_Giustificativo = new Dip_GG_Richiesta_Body_Giustificativo()
                            //{
                            //    IdPar_Giustificativi = par_ProfiloOrario.IdGiustificativo_Assenza_Ingiust,
                            //    AllDay = false,
                            //    hhmm = diff.ToString(@"hh\:mm"),
                            //};

                            //var req_1 = new GenericRequest<Dip_GG_Richiesta_Send_InModel>();

                            //req_1.Data.IdAspNetUsers = Dip_Anagrafica.IdAspNetUsers;
                            //req_1.Data.FromHR = true;
                            //req_1.Data.Dip_GG_Richiesta = new Dip_GG_RichiestaModel()
                            //{
                            //    Id = 0,
                            //    IdDip_RapportoLavoro = IdDip_RapportoLavoro,
                            //    Dati = JsonSerializer.Serialize(dip_GG_Richiesta_Body_Giustificativo)
                            //};
                            //req_1.Data.Dip_GG_Richiesta.RichiestaStato = StatoRichiesta.Approvata;
                            //req_1.Data.Dip_GG_Richiesta.RichiestaTipo = TipoRichiesta.Giustificativo;
                            //req_1.Data.Dip_GG_Richiesta.Data = day.ToString("dd/MM/yyyy");
                            //req_1.Data.Dip_GG_Richiesta.DataA = day.ToString("dd/MM/yyyy");
                            //req_1.Data.ExcludeRicalc = true;

                            //var res_1 = await _dip_GG_RichiestaService.Send(req_1, true);
                            //if (res_1.Success && res_1.Data != null)
                            //{
                            //    Merge_RichiestaService_Result(allData.Dip_GG_AllData_OutModel, res_1.Data.Dip_GG_Richiesta, res_1.Data.Dip_GG_Timbratura, res_1.Data.Dip_GG_Giustificativi);
                            //}
                        }
                    }
                    else
                    {
                        Dip_GG_RichiestaModel? dip_GG_RichiestaModel = null;
                        if (dip_GG_Giustificativi.IdDip_GG_Richiesta != null)
                            dip_GG_RichiestaModel = allData.Dip_GG_AllData_OutModel.Dip_GG_Richiesta.Where(x => x.Id == dip_GG_Giustificativi.IdDip_GG_Richiesta).FirstOrDefault();

                        if (justIsRequired)
                        {
                            if (dip_GG_Giustificativi.Hours != diff)
                            {
                                //aggiorno il giust 
                                dip_GG_Giustificativi.Hours = diff;

                                //UTILIZZO RICHIESTA DISABILITATO
                                //if (dip_GG_RichiestaModel != null)
                                //{
                                //    // aggiorno la richiesta
                                //    var body = JsonSerializer.Deserialize<Dip_GG_Richiesta_Body_Giustificativo>(dip_GG_RichiestaModel.Dati);
                                //    if (body != null)
                                //    {
                                //        body.hhmm = diff.ToString(@"hh\:mm");
                                //        dip_GG_RichiestaModel.Dati = JsonSerializer.Serialize(body);
                                //    }
                                //}
                            }
                        }
                        else
                        {
                            // non server piu, cancello
                            dip_GG_Giustificativi.ToBeDeleted = true;
                            if (dip_GG_RichiestaModel != null)
                                dip_GG_RichiestaModel.ToBeDeleted = true;
                        }
                    }
                }
            }


        }




        /// <summary>
        /// Fase giornaliera: causali dei giustificativi e delle ore ordinarie, ore lavorabili, eccedenza del giorno.
        /// Le causali restano in _causaliGiorno: la fase di periodo aggiunge quelle delle ore in piu' e le scrive.
        /// </summary>
        private GG_ResultStato GeneraCausali(Dip_AnagraficaModel Dip_Anagrafica, Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day, Dip_GG_ResultModel dip_GG_Result)
        {

            GG_ResultStato retVal = GG_ResultStato.OK;

            var orariSchema = allData.OrariSchema_4User_OutModel;

            TimeSpan oreLavorate = new TimeSpan();    // ore che hanno riscontro sulle timbrature
            TimeSpan oreLavorabili = this.CalcolaOreTeoriche(allData, IdDip_RapportoLavoro, day);  // alle ore teoriche si sommano i just con segno + e si sottraggono quelli con -
            TimeSpan oreAssenza = TimeSpan.Zero;      // giustificativi che riducono il lavorabile
            TimeSpan oreAutorizzate = TimeSpan.Zero;  // autorizzazioni alle ore in piu' approvate (o inserite dall'HR)
            TimeSpan oreInAttesa = TimeSpan.Zero;     // autorizzazioni alle ore in piu' richieste e non ancora approvate

            // dizionario temporaneo IdCausale -> TimeSpan accumulato
            var causaliAccumulate = new Dictionary<int, TimeSpan>();
            var importiAccumulati = new Dictionary<int, decimal>();
            void Accumula(int idCausale, TimeSpan valore)
            {
                if (idCausale <= 0 || valore <= TimeSpan.Zero) return;
                causaliAccumulate[idCausale] = causaliAccumulate.GetValueOrDefault(idCausale) + valore;
            }

            // -- A) Causali da Giustificativi -------------------------------------

            var giustificativiDelGiorno = allData.Dip_GG_AllData_OutModel.Dip_GG_Giustificativi
                .Where(g => g.IdDip_RapportoLavoro == IdDip_RapportoLavoro
                         && g.Data.Date == day.Date
                         && (g.RichiestaStato == StatoRichiesta.Diretta || g.RichiestaStato == StatoRichiesta.Approvata)
                         && g.ToBeDeleted == false
                         )
                .ToList();

            foreach (var giustificativo in giustificativiDelGiorno)
            {
                // cerca il parametro giustificativo per ottenere IdCausale
                var parGiust = orariSchema.Par_Giustificativi
                    .FirstOrDefault(pg => pg.Id == giustificativo.IdPar_Giustificativi);

                if (parGiust == null)
                    continue;

                // giustificativo a importo: l'importo va sulla causale (a importo) e non ha effetti sulle ore del giorno
                if (parGiust.Tipo == TipoValore.Importo)
                {
                    if (parGiust.IdCausale is int idCausaleImporto && idCausaleImporto > 0 && giustificativo.Importo is decimal importo && importo > 0)
                        importiAccumulati[idCausaleImporto] = importiAccumulati.GetValueOrDefault(idCausaleImporto) + importo;
                    continue;
                }

                TimeSpan valoreOre = giustificativo.Hours ?? TimeSpan.Zero;

                // autorizzazione alle ore in piu': non cambia il dovuto e non ha causale, fissa solo il massimo riconosciuto
                if (parGiust.AutorizzaStraordinario)
                {
                    oreAutorizzate = oreAutorizzate.Add(valoreOre);
                    continue;
                }

                // pausa non fruita: annulla solo la detrazione della pausa automatica, nessun effetto sulle ore
                if (parGiust.AnnullaPausa)
                    continue;

                if (parGiust.IdCausale.HasValue)
                    Accumula(parGiust.IdCausale.Value, valoreOre);

                if (parGiust.Segno == SignWithNeutral.Up)
                    oreLavorabili = oreLavorabili.Add(valoreOre);
                else if (parGiust.Segno == SignWithNeutral.Down)
                {
                    oreLavorabili = oreLavorabili.Subtract(valoreOre);
                    oreAssenza = oreAssenza.Add(valoreOre);
                }
            }

            if (oreLavorabili < TimeSpan.Zero)
                oreLavorabili = TimeSpan.Zero;

            // richieste di autorizzazione ancora in approvazione: il giorno le mostra "in attesa di autorizzazione"
            var idGiustAutorizzazione = orariSchema.Par_Giustificativi.Where(pg => pg.AutorizzaStraordinario).Select(pg => pg.Id).ToList();
            if (idGiustAutorizzazione.Count > 0)
            {
                oreInAttesa = allData.Dip_GG_AllData_OutModel.Dip_GG_Giustificativi
                    .Where(g => g.IdDip_RapportoLavoro == IdDip_RapportoLavoro
                             && g.Data.Date == day.Date
                             && g.ToBeDeleted == false
                             && idGiustAutorizzazione.Contains(g.IdPar_Giustificativi)
                             && (g.RichiestaStato == StatoRichiesta.Immessa ||
                                 g.RichiestaStato == StatoRichiesta.ApprovazioneInCorso ||
                                 g.RichiestaStato == StatoRichiesta.ParzialmenteApprovata))
                    .Aggregate(TimeSpan.Zero, (acc, g) => acc + (g.Hours ?? TimeSpan.Zero));
            }

            // -- B) Ore Lavorate per Intervallo ------------------------------------

            var daySlot = orariSchema.DaySlots.FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                    ds.Data.Date == day.Date);
            var profilo = daySlot != null ? orariSchema.Par_ProfiloOrario.FirstOrDefault(p => p.Id == daySlot.IdPar_ProfiloOrario) : null;

            var oreFasce = new List<(int IdCausale, TimeSpan Ore)>();   // ore lavorate dentro ogni coppia dell'orario, in ordine
            int idCausaleOrd = 0;                                        // causale delle ore ordinarie lavorate fuori dalle coppie
            TimeSpan oreFuoriFasceNotte = TimeSpan.Zero;                 // ore lavorate fuori dalle coppie in fascia notturna
            GestionePresenzePause.Esito? esitoPausa = null;             // pausa del giorno (orari a coppie)

            if (daySlot != null && daySlot.Orari.Count > 0)
            {
                var orarioBase = daySlot.Orari.OrderBy(o => o.ZOrder).First();

                var parOrario = orariSchema.ParOrario.FirstOrDefault(o => o.Id == orarioBase.IdPar_Orario);

                if (parOrario != null)
                {
                    var timbrature = Get_Timbrature_Giorno(allData, IdDip_RapportoLavoro, day, parOrario.TimbratureTipo);

                    if (parOrario.TimbratureTipo == OrarioTimbratureTipo.IntervalloOrario)
                    {
                        // coppie dell'orario come intervalli del giorno (un'uscita prima dell'entrata e' del giorno dopo)
                        var fasce = orariSchema.Par_OrarioIntervalloHH.Where(hh => hh.IdPar_Orario == parOrario.Id &&
                                                                                   hh.Dalle.HasValue && hh.Alle.HasValue &&
                                                                                   hh.Dalle.Value != hh.Alle.Value)
                                                                      .OrderBy(hh => hh.NumCoppia)
                                                                      .Select(hh => (IdCausale: hh.IdCausale_HH_Lav,
                                                                                     Dal: day.Date + hh.Dalle!.Value.ToTimeSpan(),
                                                                                     Al: day.Date + hh.Alle!.Value.ToTimeSpan() + (hh.Alle.Value < hh.Dalle.Value ? TimeSpan.FromDays(1) : TimeSpan.Zero)))
                                                                      .ToList();

                        idCausaleOrd = fasce.Select(f => f.IdCausale).FirstOrDefault(id => id > 0);
                        var oreFascia = new TimeSpan[fasce.Count];

                        // segmenti lavorati al netto della pausa (ANALISI_ATTENDANCETRACKING_PAUSE.md 6.4)
                        var segmenti = SegmentiLavorati(allData, IdDip_RapportoLavoro, day, parOrario, timbrature, out var esitoGiorno);
                        foreach (var (tStart, tEnd) in segmenti)
                        {
                            oreLavorate = oreLavorate.Add(tEnd - tStart);

                            for (int k = 0; k < fasce.Count; k++)
                                oreFascia[k] += Sovrapposizione(tStart, tEnd, fasce[k].Dal, fasce[k].Al);

                            foreach (var (s, e) in Sottrai(tStart, tEnd, fasce.Select(f => (f.Dal, f.Al))))
                                oreFuoriFasceNotte += OreNotturne(s, e, profilo);
                        }
                        esitoPausa = esitoGiorno;

                        for (int k = 0; k < fasce.Count; k++)
                            oreFasce.Add((fasce[k].IdCausale, oreFascia[k]));
                    }
                    else if (parOrario.TimbratureTipo == OrarioTimbratureTipo.MonteOre || parOrario.TimbratureTipo == OrarioTimbratureTipo.MonteOreValore)
                    {
                        idCausaleOrd = parOrario.IdCausale_HH_Lav_MonteOre ?? 0;

                        for (int i = 0; i < timbrature.Count; i++)
                        {
                            if (timbrature[i].Timbratura is DateTime valore)
                                oreLavorate += valore.TimeOfDay;
                        }

                        oreFasce.Add((idCausaleOrd, oreLavorate));
                    }
                }
            }

            // -- C) Ordinarie: il lavorato entro il lavorabile ---------------------
            // le ore dentro le coppie oltre il lavorabile (es. permesso) si tolgono dalle ultime coppie,
            // quelle lavorate fuori dalle coppie fino al lavorabile vanno sulla causale della prima coppia
            var oreOrd = oreLavorate < oreLavorabili ? oreLavorate : oreLavorabili;
            var totFasce = oreFasce.Aggregate(TimeSpan.Zero, (acc, f) => acc + f.Ore);

            if (totFasce > oreOrd)
            {
                var daTogliere = totFasce - oreOrd;
                for (int k = oreFasce.Count - 1; k >= 0 && daTogliere > TimeSpan.Zero; k--)
                {
                    var t = oreFasce[k].Ore < daTogliere ? oreFasce[k].Ore : daTogliere;
                    oreFasce[k] = (oreFasce[k].IdCausale, oreFasce[k].Ore - t);
                    daTogliere -= t;
                }
            }
            else if (totFasce < oreOrd && idCausaleOrd > 0)
            {
                oreFasce.Add((idCausaleOrd, oreOrd - totFasce));
            }

            foreach (var (idCausale, ore) in oreFasce)
                Accumula(idCausale, ore);

            // pausa del giorno: minuti sul risultato, detratti sulla causale informativa, avvisi
            dip_GG_Result.Min_PausaRilevata = esitoPausa?.Rilevata ?? 0;
            dip_GG_Result.Min_PausaDetratta = esitoPausa?.Detratta ?? 0;
            dip_GG_Result.Min_PausaRetribuita = esitoPausa?.Retribuita ?? 0;
            if (esitoPausa != null)
            {
                if (esitoPausa.IdCausale_Pausa is int idCausalePausa)
                    Accumula(idCausalePausa, TimeSpan.FromMinutes(esitoPausa.Detratta));
                if (esitoPausa.Insufficiente)
                    Dip_GG_Result_Helper.AddDetail(ref retVal, GG_ResultStato.Warning_3);   // pausa insufficiente o non fruita
                if (esitoPausa.OltreMax)
                    Dip_GG_Result_Helper.AddDetail(ref retVal, GG_ResultStato.Warning_4);   // pausa oltre la durata massima
            }

            // -- D) Eccedenza del giorno ---------------------------------------------
            var differenza = oreLavorate - oreLavorabili;
            int minEcc;
            if (differenza > TimeSpan.Zero)
            {
                int minuti = (int)differenza.TotalMinutes;
                int blocco = profilo != null ? Math.Max(1, (int)profilo.EccArrotondamento) : 1;
                minuti = minuti / blocco * blocco;                          // arrotondamento per difetto a blocchi
                if (profilo != null && minuti < profilo.EccFranchigiaMin)   // sotto la franchigia non si riconosce
                    minuti = 0;
                minEcc = minuti;
            }
            else
            {
                minEcc = (int)Math.Round(differenza.TotalMinutes);         // ore mancanti (negativo)
            }

            // profilo con ore in piu' solo se autorizzate: si riconoscono fino alle ore autorizzate del giorno,
            // il resto non entra nella qualifica (ne' nella soglia) e resta visibile come "non autorizzato"
            int minNonAutorizzate = 0, minDaAutorizzare = 0;
            if (profilo != null && profilo.StraoSoloAutorizzato && minEcc > 0)
            {
                int riconosciute = Math.Min(minEcc, (int)oreAutorizzate.TotalMinutes);
                minNonAutorizzate = minEcc - riconosciute;
                minDaAutorizzare = Math.Min(minNonAutorizzate, (int)oreInAttesa.TotalMinutes);
                minEcc = riconosciute;
            }

            dip_GG_Result.Min_NonAutorizzate = minNonAutorizzate;
            dip_GG_Result.Min_DaAutorizzare = minDaAutorizzare;
            dip_GG_Result.Min_Lavorabili = (int)oreLavorabili.TotalMinutes;
            dip_GG_Result.Min_Assenze = (int)oreAssenza.TotalMinutes;
            dip_GG_Result.Min_Ord = (int)oreOrd.TotalMinutes;
            dip_GG_Result.Min_Ecc = minEcc;
            dip_GG_Result.Min_EccNott = minEcc > 0 ? Math.Min(minEcc, (int)oreFuoriFasceNotte.TotalMinutes) : 0;
            dip_GG_Result.Festivo = (daySlot?.Festivo ?? false) ||
                                    (profilo != null && profilo.DomenicaFestiva && day.DayOfWeek == DayOfWeek.Sunday);

            _causaliGiorno[(IdDip_RapportoLavoro, day.Date)] = causaliAccumulate;
            _importiGiorno[(IdDip_RapportoLavoro, day.Date)] = importiAccumulati;

            if (oreLavorabili != oreLavorate)
            {
                if (oreLavorate < oreLavorabili)
                {
                    Dip_GG_Result_Helper.AddDetail(ref retVal, GG_ResultStato.Err_2);
                }
                else
                {
                    // ore in piu': non e' un errore, le qualifica il calcolo di straordinario/supplementare
                    Dip_GG_Result_Helper.AddDetail(ref retVal, GG_ResultStato.Warning_1);
                }
            }


            return retVal;
        }

        /// <summary>
        /// Scrive le causali del giorno: aggiorna quelle esistenti, aggiunge le nuove, cancella le altre.
        /// Le causali a importo (<paramref name="importi"/>) hanno l'importo in euro e Valore 00:00.
        /// </summary>
        private void ScriviCausali(Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day, Dictionary<int, TimeSpan> causaliAccumulate,
                                   Dictionary<int, decimal>? importi = null)
        {
            // recupera le causali pre-esistenti per questo giorno/rapporto
            var causaliEsistenti = allData.Dip_GG_AllData_OutModel.Dip_GG_Causali.Where(c => c.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                                             c.Data.Date == day.Date)
                                                                                 .ToList();

            // 1) marca tutte le pre-esistenti come ToBeDeleted
            foreach (var esistente in causaliEsistenti)
            {
                esistente.ToBeDeleted = true;
            }

            // 2) per ogni causale calcolata, cerca se esiste già → aggiorna, altrimenti aggiungi
            foreach (var kvp in causaliAccumulate)
            {
                if (kvp.Value <= TimeSpan.Zero)
                    continue;

                var nuovoValore = TimeOnly.FromTimeSpan(kvp.Value);

                var causaleEsistente = causaliEsistenti
                    .FirstOrDefault(c => c.IdPar_Causali == kvp.Key);

                if (causaleEsistente != null)
                {
                    // esiste già: aggiorna il valore e rimuovi il flag ToBeDeleted
                    causaleEsistente.Valore = nuovoValore;
                    causaleEsistente.Importo = null;
                    causaleEsistente.ToBeDeleted = false;
                }
                else
                {
                    // non esiste: crea nuovo record
                    allData.Dip_GG_AllData_OutModel.Dip_GG_Causali.Add(new Dip_GG_CausaliModel
                    {
                        Id = 0,
                        IdDip_RapportoLavoro = IdDip_RapportoLavoro,
                        Data = day.Date,
                        IdPar_Causali = kvp.Key,
                        Valore = nuovoValore,
                        ToBeDeleted = false
                    });
                }
            }

            // 3) causali a importo (giustificativi a importo): stesso criterio, con l'importo al posto delle ore
            foreach (var kvp in importi ?? new Dictionary<int, decimal>())
            {
                if (kvp.Value <= 0)
                    continue;

                var importo = Math.Round(kvp.Value, 2);
                var causaleEsistente = causaliEsistenti.FirstOrDefault(c => c.IdPar_Causali == kvp.Key);
                if (causaleEsistente != null)
                {
                    causaleEsistente.Valore = TimeOnly.MinValue;
                    causaleEsistente.Importo = importo;
                    causaleEsistente.ToBeDeleted = false;
                }
                else
                {
                    allData.Dip_GG_AllData_OutModel.Dip_GG_Causali.Add(new Dip_GG_CausaliModel
                    {
                        Id = 0,
                        IdDip_RapportoLavoro = IdDip_RapportoLavoro,
                        Data = day.Date,
                        IdPar_Causali = kvp.Key,
                        Valore = TimeOnly.MinValue,
                        Importo = importo,
                        ToBeDeleted = false
                    });
                }
            }
        }

        private static TimeSpan Sovrapposizione(DateTime s1, DateTime e1, DateTime s2, DateTime e2)
        {
            var s = s1 > s2 ? s1 : s2;
            var e = e1 < e2 ? e1 : e2;
            return e > s ? e - s : TimeSpan.Zero;
        }

        /// <summary>Parti dell'intervallo [inizio, fine] non coperte dagli intervalli indicati.</summary>
        private static List<(DateTime Dal, DateTime Al)> Sottrai(DateTime inizio, DateTime fine, IEnumerable<(DateTime Dal, DateTime Al)> intervalli)
        {
            var retVal = new List<(DateTime, DateTime)>();
            var corrente = inizio;
            foreach (var (dal, al) in intervalli.Where(x => x.Al > inizio && x.Dal < fine).OrderBy(x => x.Dal))
            {
                if (dal > corrente)
                    retVal.Add((corrente, dal < fine ? dal : fine));
                if (al > corrente)
                    corrente = al;
                if (corrente >= fine)
                    break;
            }
            if (corrente < fine)
                retVal.Add((corrente, fine));
            return retVal;
        }

        /// <summary>Ore dell'intervallo che cadono nella fascia notturna del profilo (es. 22:00-06:00).</summary>
        private static TimeSpan OreNotturne(DateTime inizio, DateTime fine, Par_ProfiloOrarioModel? profilo)
        {
            if (profilo == null || profilo.NotturnoDalle == profilo.NotturnoAlle)
                return TimeSpan.Zero;

            var retVal = TimeSpan.Zero;
            for (var d = inizio.Date.AddDays(-1); d <= fine.Date; d = d.AddDays(1))
            {
                var dal = d + profilo.NotturnoDalle.ToTimeSpan();
                var al = d + profilo.NotturnoAlle.ToTimeSpan();
                if (profilo.NotturnoAlle < profilo.NotturnoDalle)
                    al = al.AddDays(1);
                retVal += Sovrapposizione(inizio, fine, dal, al);
            }
            return retVal;
        }

        private List<Dip_GG_TimbraturaModel> Get_Timbrature_Giorno(Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day, OrarioTimbratureTipo TimbratureTipo)
        {
            var timbrature = allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura.Where(t => t.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                                          t.GiornoCompetenza.Date == day.Date &&
                                                                                         (t.RichiestaStato == StatoRichiesta.Diretta || t.RichiestaStato == StatoRichiesta.Approvata) &&
                                                                                          t.TimbraturaArrotondata.HasValue &&
                                                                                         (TimbratureTipo == OrarioTimbratureTipo.IntervalloOrario ?
                                                                                             (t.TimbraturaTipo == TipoTimbratura.Entrata || t.TimbraturaTipo == TipoTimbratura.Uscita)
                                                                                             :
                                                                                             (t.TimbraturaTipo == TipoTimbratura.SenzaVerso /*|| t.TimbraturaTipo == TipoTimbratura.Attivita*/))
                                                                                     )
                                                                              .OrderBy(t => t.TimbraturaArrotondata)
                                                                              .ToList();

            return timbrature;
        }
    }

    public interface ITimeSheet_EngineService : IServiceBase
    {
        Task<GenericResult<TimeSheet_CalculateOutModel>> Calculate(GenericRequest<TimeSheet_CalculateInModel> model, bool isSubProcess);
        Task<GenericResult<OrariSchema_4User_OutModel>> Get_OrariSchema_4User(GenericRequest<OrariSchema_4User_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_GG_AllData_OutModel>> Dip_GG_AllData_AllData(GenericRequest<Dip_GG_AllData_InModel> model, bool isSubProcess);
        Task<GenericResult<Timesheet_AllData_OutModel>> Get_Timesheet_AllData(GenericRequest<Timesheet_AllData_InModel> model, bool isSubProcess);
        Task<FormulaProvaRisultato> SimulaFormula(string idAspNetUsers, int idDip_RapportoLavoro, DateTime giorno, string testo, FormulaFase fase, int idAz_Anagrafica);
    }




    public interface ITimeSheet_EngineService_OnlyCalculate
    {
        Task<GenericResult<TimeSheet_CalculateOutModel>> Calculate(GenericRequest<TimeSheet_CalculateInModel> model, bool isSubProcess);
    }


    public class TimeSheet_EngineService_OnlyCalculate : ServiceBase, ITimeSheet_EngineService_OnlyCalculate
    {
        private readonly IServiceProvider _serviceProvider;

        public TimeSheet_EngineService_OnlyCalculate(
            IMapper mapper,
            UserManager<ApplicationUser> userManager,
            IAspNetUsersRepository aspNetUsersRepository,
            IOptions<JwtParameter> jwtParameter,
            IConfiguration configuration,
            IHttpContextAccessor httpContextAccessor,
            IServiceProvider serviceProvider)
            : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _serviceProvider = serviceProvider;
        }

        public async Task<GenericResult<TimeSheet_CalculateOutModel>> Calculate(
            GenericRequest<TimeSheet_CalculateInModel> model, bool isSubProcess)
        {
            // risolve ITimeSheet_EngineService a runtime — il ciclo è già spezzato
            // perché questa classe NON dipende da IDip_GG_TimbraturaService
            var engine = _serviceProvider.GetRequiredService<ITimeSheet_EngineService>();
            return await engine.Calculate(model, isSubProcess);
        }
    }




}
