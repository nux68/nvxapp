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
                                      IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                      IDip_GG_TimbraturaService dip_GG_TimbraturaService,
                                      IDip_GG_CausaliService dip_GG_CausaliService,
                                      IDip_GG_GiustificativiService dip_GG_GiustificativiService,
                                      IDip_GG_ResultService dip_GG_ResultService,
                                      IPar_GiustificativiService par_GiustificativiService,
                                      IDip_GG_RichiestaService dip_GG_RichiestaService

                                      ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _longJobNotifier = longJobNotifier;
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
                        outModel.Messages.Add(new Message("User not identified; cannot start job.", MessageType.Error));
                    }
                    else
                    {
                        try
                        {
                            Log.Information("Starting TimeSheet calculation for users {UserIds} from {Dal} to {Al}. jobID = {JobId}", model.Data.TimeSheet_Calculate.SelectedUserId, model.Data.TimeSheet_Calculate.Dal, model.Data.TimeSheet_Calculate.Al, jobId);

                            await _longJobNotifier.LongJobProgressAsync(userId,
                                                                        new LongJobProgressUpdate
                                                                        {
                                                                            JobId = jobId.ToString(),
                                                                            JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                                                                            Payload = model.Data.TimeSheet_Calculate,
                                                                            ProgressPercentage = 0,
                                                                            Message = new Message { Text = "Calcolo presenze avviato...", MsgType = MessageType.Information }
                                                                        }
                                                                        );

                            var req_OrariSchema_4User = new GenericRequest<Timesheet_AllData_InModel>();
                            req_OrariSchema_4User.Data.Dal = model.Data.TimeSheet_Calculate.Dal;
                            req_OrariSchema_4User.Data.Al = model.Data.TimeSheet_Calculate.Al;
                            req_OrariSchema_4User.Data.UsersId = model.Data.TimeSheet_Calculate.SelectedUserId;

                            var AllData = await Get_Timesheet_AllData(req_OrariSchema_4User, true);

                            if (AllData.Success && AllData.Data != null)
                            {
                                // ciclo su ogni utente selezionato
                                int idxUser = 0;
                                foreach (var userId_calc in model.Data.TimeSheet_Calculate.SelectedUserId)
                                {




                                    var progress = (int)((idxUser / (double)model.Data.TimeSheet_Calculate.SelectedUserId.Count) * 100);
                                    await _longJobNotifier.LongJobProgressAsync(userId,
                                                                                new LongJobProgressUpdate
                                                                                {
                                                                                    JobId = jobId.ToString(),
                                                                                    JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                                                                                    Payload = model.Data.TimeSheet_Calculate,
                                                                                    ProgressPercentage = progress,
                                                                                    Message = new Message { Text = $"Calcolo presenze step {idxUser + 1} of {model.Data.TimeSheet_Calculate.SelectedUserId.Count}", MsgType = MessageType.Information }
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
                                        // limita il ciclo al range effettivo del rapporto
                                        var giornoInizio_calc = model.Data.TimeSheet_Calculate.Dal > rapporto_calc.DataAss!.Value
                                                                ? model.Data.TimeSheet_Calculate.Dal
                                                                : rapporto_calc.DataAss!.Value;

                                        var giornoFine_calc = (rapporto_calc.DataLic == null || rapporto_calc.DataLic.Value > model.Data.TimeSheet_Calculate.Al)
                                                               ? model.Data.TimeSheet_Calculate.Al
                                                               : rapporto_calc.DataLic.Value;

                                        // ciclo su ogni giorno del periodo richiesto
                                        for (var giorno = giornoInizio_calc; giorno <= giornoFine_calc; giorno = giorno.AddDays(1))
                                        {
                                            await CalcolaGiorno(anagrafica_calc, model.Data.TimeSheet_Calculate, rapporto_calc, giorno, AllData.Data);
                                        }
                                    }

                                    idxUser++;
                                    //await Task.Delay(1000);
                                }


                                await this.SaveData(AllData.Data);

                                Log.Information("Background task for job {JobId} has finished successfully.", jobId);
                                await _longJobNotifier.LongJobProgressAsync(userId,
                                                                            new LongJobProgressUpdate
                                                                            {
                                                                                JobId = jobId.ToString(),
                                                                                JobType = GestionePresenze_JobType.TimeSheet_Engine_Calculate,
                                                                                Payload = model.Data.TimeSheet_Calculate,
                                                                                ProgressPercentage = 100,
                                                                                Message = new Message { Text = "Calcolo presenze completato", MsgType = MessageType.Information },
                                                                                IsFinished = true
                                                                            }
                                                                           );


                            }

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
                                                                            Message = new Message { Text = $"Calcolo presenze fallito: {ex.Message}", MsgType = MessageType.Exception },
                                                                            IsFinished = true
                                                                        }
                                                                        );
                        }
                    }




                }

                //eliminare
                // Nessun 'await' qui
                //await Task.Delay(DelayAsyncMethod);
                await Task.Delay(0);

                return retVal;
            }, isSubProcess);
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

                            retVal.DaySlots.Add(new Dip_ProfiloOrario_DaySlot
                            {
                                IdAspNetUsers = anagrafica.IdAspNetUsers,
                                IdDip_RapportoLavoro = rapporto.Id,
                                Data = giorno,
                                IdPar_ProfiloOrario = profilo.IdPar_ProfiloOrario!.Value,
                                Orari = orariDelGiorno
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
                                if (timbraturaItem.TimbraturaTipo != TipoTimbratura.Attivita)
                                {
                                    //timbraturaItem.TimbraturaTipo = (i % 2 == 0) ? TipoTimbratura.Entrata : TipoTimbratura.Uscita;
                                }
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




        private async Task CalcolaGiorno(Dip_AnagraficaModel Dip_Anagrafica, TimeSheet_CalculateModel timeSheet_CalculateModel, Dip_RapportoLavoroModel rapporto_calc, DateTime giorno, Timesheet_AllData_OutModel AllData)
        {
            if (timeSheet_CalculateModel.Approva_Richieste_Giustificativo)
            {
                await this.ApprovaRichiesta(TipoRichiesta.Giustificativo, AllData.Dip_GG_AllData_OutModel, rapporto_calc.Id, giorno);
            }

            if (timeSheet_CalculateModel.Approva_Richieste_Timbrature)
            {
                await this.ApprovaRichiesta(TipoRichiesta.Timbratura, AllData.Dip_GG_AllData_OutModel, rapporto_calc.Id, giorno);
            }

            if (timeSheet_CalculateModel.Genera_Timbrature_Mancanti)
            {
                GeneraTimbratureMancanti(AllData, rapporto_calc.Id, giorno);
            }

            GG_ResultStato Calcolo1Result = GG_ResultStato.OK;
            GG_ResultStato Calcolo2Result = GG_ResultStato.OK;

            GG_ResultStato VersoTimbratureResult = AssegnaVersoTimbrature(AllData, rapporto_calc.Id, giorno);

            var dip_GG_Result = AllData.Dip_GG_AllData_OutModel.Dip_GG_Result.Where(x => x.Data == giorno).FirstOrDefault();
            if (dip_GG_Result != null)
            {
                dip_GG_Result.HH_Teo = TimeOnly.FromTimeSpan(this.CalcolaOreTeoriche(AllData, rapporto_calc.Id, giorno));
                dip_GG_Result.HH_Lav = TimeOnly.FromTimeSpan(this.CalcolaOreLavorate(AllData, rapporto_calc.Id, giorno));
            }

            if (dip_GG_Result != null)
            {
                if (timeSheet_CalculateModel.Genera_Giustificativo_Assenza)
                {
                    await GeneraGiustificativoAssenza(Dip_Anagrafica, AllData, rapporto_calc.Id, giorno, dip_GG_Result);
                }

                /*await*/
                GeneraCausali(Dip_Anagrafica, AllData, rapporto_calc.Id, giorno, dip_GG_Result);



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

            return;
        }
        private async Task ApprovaRichiesta(TipoRichiesta tipoRichiesta, Dip_GG_AllData_OutModel dip_GG_AllData, int IdDip_RapportoLavoro, DateTime day)
        {


            var req_1 = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>();

            req_1.Data.RichiestaStato = StatoRichiesta.Approvata;
            req_1.Data.FromHR = true;
            req_1.Data.IdDip_GG_Richiesta = dip_GG_AllData.Dip_GG_Richiesta.Where(x => x.RichiestaTipo == tipoRichiesta &&
                                                                                    (x.RichiestaStato == StatoRichiesta.Immessa || x.RevocaStato == StatoRichiesta.Immessa) &&
                                                                                    x.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                                    x.Data == x.DataA &&
                                                                                    DateTime.Parse(x.Data) == day)
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

            var coppie = orariSchema.Par_OrarioIntervalloHH
                .Where(hh => hh.IdPar_Orario == parOrario.Id)
                .OrderBy(hh => hh.NumCoppia)
                .ToList();

            if (coppie.Count == 0)
                return TimeSpan.Zero;

            var oreTeoriche = TimeSpan.Zero;
            foreach (var coppia in coppie)
            {
                if (coppia.Dalle.HasValue && coppia.Alle.HasValue)
                {
                    var durataCoppia = coppia.Alle.Value.ToTimeSpan() - coppia.Dalle.Value.ToTimeSpan();
                    if (durataCoppia > TimeSpan.Zero)
                        oreTeoriche += durataCoppia;
                }
            }

            return oreTeoriche;
        }
        private TimeSpan CalcolaOreLavorate(Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day)
        {
            var timbrature = Get_Timbrature_Giorno(allData, IdDip_RapportoLavoro, day);

            if (timbrature.Count < 2)
                return TimeSpan.Zero;

            var oreLavorate = TimeSpan.Zero;

            // scorre le coppie in sequenza: pos pari = Entrata, pos dispari = Uscita
            for (int i = 0; i + 1 < timbrature.Count; i += 2)
            {
                var entrata = timbrature[i];
                var uscita = timbrature[i + 1];

                // sicurezza: la sequenza deve essere E poi U
                if (entrata.TimbraturaTipo != TipoTimbratura.Entrata ||
                    uscita.TimbraturaTipo != TipoTimbratura.Uscita)
                    continue;

                var durata = uscita.TimbraturaArrotondata!.Value - entrata.TimbraturaArrotondata!.Value;
                if (durata > TimeSpan.Zero)
                    oreLavorate += durata;
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

            var coppie = orariSchema.Par_OrarioIntervalloHH
                .Where(hh => hh.IdPar_Orario == parOrario.Id)
                .OrderBy(hh => hh.NumCoppia)
                .ToList();

            if (coppie.Count == 0)
                return retVal;

            // solo SenzaVerso, ordinate per orario crescente — Attivita escluse
            var timbratureSenzaVerso = allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura.Where(t => t.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                                                    t.GiornoCompetenza.Date == day.Date &&
                                                                                                    t.TimbraturaTipo != TipoTimbratura.Attivita)
                                                                                        .OrderBy(t => t.Timbratura)
                                                                                        .ToList();

            //if (timbratureSenzaVerso.Count == 0)
            //    return retVal;


            List<CoppiaTMP> coppieTMP = new List<CoppiaTMP>();
            foreach (var coppia in coppie)
            {
                if (!coppia.Dalle.HasValue || !coppia.Alle.HasValue)
                    continue;
                if (coppia.Alle.Value == coppia.Dalle.Value)
                    continue;

                if (coppia.Dalle_Use_4_Match)
                {
                    var dalleLimit_SX = (coppia.Dalle_Limite_SX ?? coppia.Dalle).Value;
                    var dalleLimit_DX = (coppia.Dalle_Limite_DX ?? coppia.Dalle).Value;
                    coppieTMP.Add(new CoppiaTMP { HH = coppia.Dalle, HH_Limite_SX = dalleLimit_SX, HH_Limite_DX = dalleLimit_DX, Arrotondamento = coppia.Dalle_Arrotondamento, Arrotondamento_Verso = coppia.Dalle_Arrotondamento_Verso });
                }

                if (coppia.Alle_Use_4_Match)
                {
                    var alleLimit_SX = (coppia.Alle_Limite_SX ?? coppia.Alle).Value;
                    var alleLimit_DX = (coppia.Alle_Limite_DX ?? coppia.Alle).Value;
                    coppieTMP.Add(new CoppiaTMP { HH = coppia.Alle, HH_Limite_SX = alleLimit_SX, HH_Limite_DX = alleLimit_DX, Arrotondamento = coppia.Alle_Arrotondamento, Arrotondamento_Verso = coppia.Alle_Arrotondamento_Verso });
                }
            }

            if (timbratureSenzaVerso.Count < coppieTMP.Count)
            {
                Dip_GG_Result_Helper.AddDetail(ref retVal, GG_ResultStato.Err_1);
                return retVal;
            }



            foreach (var timbratura in timbratureSenzaVerso)
            {
                var oraTimbr = TimeOnly.FromDateTime(timbratura.Timbratura);
                var coppiaMatch = coppieTMP.FirstOrDefault(c => !c.Check &&
                                                                oraTimbr >= c.HH_Limite_SX &&
                                                                oraTimbr <= c.HH_Limite_DX);

                if (coppiaMatch == null)
                {
                    // SE NO MATCH: coppiaTMP non checkata con HH più vicino
                    coppiaMatch = coppieTMP
                        .Where(c => !c.Check && c.HH.HasValue)
                        .OrderBy(c => Math.Abs((c.HH!.Value.ToTimeSpan() - oraTimbr.ToTimeSpan()).Ticks))
                        .FirstOrDefault();
                }

                if (coppiaMatch == null)
                    continue;

                coppiaMatch.Check = true;

                // assegna il verso in sequenza alternata E U E U ...
                // la posizione nella lista coppieTMP determina pari=Entrata, dispari=Uscita
                int idx = coppieTMP.IndexOf(coppiaMatch);
                bool isEntrata = (idx % 2 == 0);

                timbratura.TimbraturaTipo = isEntrata ? TipoTimbratura.Entrata : TipoTimbratura.Uscita;

                var roundOptions = new TimeRoundOptions(coppiaMatch.Arrotondamento, coppiaMatch.Arrotondamento_Verso);
                timbratura.TimbraturaArrotondata = Roundings.RoundDateTime(timbratura.Timbratura, roundOptions);
            }



            //// assegna in sequenza alternata E U E U ..
            //// la posizione i determina:
            ////   pari    → Entrata, usa arrotondamento Dalle della coppia i/2
            ////   dispari → Uscita,  usa arrotondamento Alle  della coppia i/2
            //for (int i = 0; i < timbratureSenzaVerso.Count; i++)
            //{
            //    var timbratura = timbratureSenzaVerso[i];
            //    int idxCoppia = i / 2;
            //    bool isEntrata = (i % 2 == 0);


            //    // se abbiamo più timbrature delle coppie previste, usa l'ultima coppia disponibile
            //    if (idxCoppia >= coppie.Count)
            //        idxCoppia = coppie.Count - 1;

            //    var coppia = coppie[idxCoppia];

            //    timbratura.TimbraturaTipo = isEntrata ? TipoTimbratura.Entrata : TipoTimbratura.Uscita;

            //    var roundOptions = isEntrata
            //        ? new TimeRoundOptions(coppia.Dalle_Arrotondamento, coppia.Dalle_Arrotondamento_Verso)
            //        : new TimeRoundOptions(coppia.Alle_Arrotondamento, coppia.Alle_Arrotondamento_Verso);

            //    timbratura.TimbraturaArrotondata = Roundings.RoundDateTime(timbratura.Timbratura, roundOptions);
            //}

            return retVal;

        }
        private void GeneraTimbratureMancanti(Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day)
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

            var coppie = orariSchema.Par_OrarioIntervalloHH.Where(hh => hh.IdPar_Orario == parOrario.Id)
                                                           .OrderBy(hh => hh.NumCoppia)
                                                           .ToList();

            if (coppie.Count == 0)
                return;

            var timbratureEsistenti = allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura.Where(t => t.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                                                   t.GiornoCompetenza.Date == day.Date)
                                                                                       .ToList();

            List<CoppiaTMP> coppieTMP = new List<CoppiaTMP>();
            foreach (var coppia in coppie)
            {
                if (!coppia.Dalle.HasValue || !coppia.Alle.HasValue)
                    continue;
                if (coppia.Alle.Value == coppia.Dalle.Value)
                    continue;

                if (coppia.Dalle_Use_4_Match)
                {
                    var dalleLimit_SX = (coppia.Dalle_Limite_SX ?? coppia.Dalle).Value;
                    var dalleLimit_DX = (coppia.Dalle_Limite_DX ?? coppia.Dalle).Value;
                    coppieTMP.Add(new CoppiaTMP { HH = coppia.Dalle, HH_Limite_SX = dalleLimit_SX, HH_Limite_DX = dalleLimit_DX, Arrotondamento = coppia.Dalle_Arrotondamento, Arrotondamento_Verso = coppia.Dalle_Arrotondamento_Verso });
                }

                if (coppia.Alle_Use_4_Match)
                {
                    var alleLimit_SX = (coppia.Alle_Limite_SX ?? coppia.Alle).Value;
                    var alleLimit_DX = (coppia.Alle_Limite_DX ?? coppia.Alle).Value;
                    coppieTMP.Add(new CoppiaTMP { HH = coppia.Alle, HH_Limite_SX = alleLimit_SX, HH_Limite_DX = alleLimit_DX, Arrotondamento = coppia.Alle_Arrotondamento, Arrotondamento_Verso = coppia.Alle_Arrotondamento_Verso });
                }


            }

            if (coppieTMP.Count > timbratureEsistenti.Count)
            {
                foreach (var itemTimr in timbratureEsistenti)
                {
                    var oraTimbr = TimeOnly.FromDateTime(itemTimr.Timbratura);
                    var coppiaMatch = coppieTMP.FirstOrDefault(c => oraTimbr >= c.HH_Limite_SX && oraTimbr <= c.HH_Limite_DX);
                    if (coppiaMatch != null)
                    {
                        coppiaMatch.Check = true;
                    }
                    else
                    {
                        // SE NO MATCH: cerca la coppiaTMP non checkata con HH più vicino alla timbratura
                        var coppiaVicina = coppieTMP
                            .Where(c => !c.Check && c.HH.HasValue)
                            .OrderBy(c => Math.Abs((c.HH!.Value.ToTimeSpan() - oraTimbr.ToTimeSpan()).Ticks))
                            .FirstOrDefault();

                        if (coppiaVicina != null)
                            coppiaVicina.Check = true;
                    }
                }


                foreach (var coppia in coppieTMP.Where(c => !c.Check && c.HH != null))
                {
                    var entity = new Dip_GG_Timbratura { IdDip_RapportoLavoro = IdDip_RapportoLavoro };
                    var vm = _mapper.Map<Dip_GG_TimbraturaModel>(entity);
                    vm.Timbratura = day.Date + (coppia.HH != null ? coppia.HH.Value : new TimeOnly()).ToTimeSpan();
                    vm.TimbraturaOriginale = day.Date + (coppia.HH != null ? coppia.HH.Value : new TimeOnly()).ToTimeSpan();
                    vm.GiornoCompetenza = day.Date;
                    vm.TimbraturaTipo = TipoTimbratura.SenzaVerso;
                    vm.RichiestaStato = StatoRichiesta.Diretta;
                    allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura.Add(vm);
                }
            }

        }
        private async Task GeneraGiustificativoAssenza(Dip_AnagraficaModel Dip_Anagrafica, Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day, Dip_GG_ResultModel dip_GG_Result)
        {

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
                            // necessario ed assente, lo aggiungo con il service delle richieste
                            Dip_GG_Richiesta_Body_Giustificativo dip_GG_Richiesta_Body_Giustificativo = new Dip_GG_Richiesta_Body_Giustificativo()
                            {
                                IdPar_Giustificativi = par_ProfiloOrario.IdGiustificativo_Assenza_Ingiust,
                                AllDay = false,
                                hhmm = diff.ToString(@"hh\:mm"),
                            };

                            var req_1 = new GenericRequest<Dip_GG_Richiesta_Send_InModel>();

                            req_1.Data.IdAspNetUsers = Dip_Anagrafica.IdAspNetUsers;
                            req_1.Data.FromHR = true;
                            req_1.Data.Dip_GG_Richiesta = new Dip_GG_RichiestaModel()
                            {
                                Id = 0,
                                IdDip_RapportoLavoro = IdDip_RapportoLavoro,
                                Dati = JsonSerializer.Serialize(dip_GG_Richiesta_Body_Giustificativo)
                            };
                            req_1.Data.Dip_GG_Richiesta.RichiestaStato = StatoRichiesta.Approvata;
                            req_1.Data.Dip_GG_Richiesta.RichiestaTipo = TipoRichiesta.Giustificativo;
                            req_1.Data.Dip_GG_Richiesta.Data = day.ToString("dd/MM/yyyy");
                            req_1.Data.Dip_GG_Richiesta.DataA = day.ToString("dd/MM/yyyy");

                            var res_1 = await _dip_GG_RichiestaService.Send(req_1, true);
                            if (res_1.Success && res_1.Data != null)
                            {
                                Merge_RichiestaService_Result(allData.Dip_GG_AllData_OutModel, res_1.Data.Dip_GG_Richiesta, res_1.Data.Dip_GG_Timbratura, res_1.Data.Dip_GG_Giustificativi);
                            }
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

                                if (dip_GG_RichiestaModel != null)
                                {
                                    // aggiorno la richiesta
                                    var body = JsonSerializer.Deserialize<Dip_GG_Richiesta_Body_Giustificativo>(dip_GG_RichiestaModel.Dati);
                                    if (body != null)
                                    {
                                        body.hhmm = diff.ToString(@"hh\:mm");
                                        dip_GG_RichiestaModel.Dati = JsonSerializer.Serialize(body);
                                    }
                                }
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
        private void GeneraCausali(Dip_AnagraficaModel Dip_Anagrafica, Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day, Dip_GG_ResultModel dip_GG_Result)
        {
            // dizionario temporaneo IdCausale ? TimeSpan accumulato
            var causaliAccumulate = new Dictionary<int, TimeSpan>();

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
                var parGiust = allData.OrariSchema_4User_OutModel.Par_Giustificativi
                    .FirstOrDefault(pg => pg.Id == giustificativo.IdPar_Giustificativi);

                if (parGiust == null || !parGiust.IdCausale.HasValue || parGiust.IdCausale.Value == 0)
                    continue;

                int idCausale = parGiust.IdCausale.Value;

                // determina il valore ore del giustificativo
                TimeSpan valoreOre = TimeSpan.Zero;

                switch (giustificativo.InputType)
                {
                    case JustificationInputType.Manual:
                        // ore inserite manualmente
                        valoreOre = giustificativo.Hours ?? TimeSpan.Zero;
                        break;

                    case JustificationInputType.AllDay:
                        // intera giornata: prende le ore teoriche del giorno
                        valoreOre = this.CalcolaOreTeoriche(allData, IdDip_RapportoLavoro, day);
                        break;

                    case JustificationInputType.IntegrateDay:
                        // integra la giornata: differenza tra ore teoriche e ore reali
                        var oreTeoInt = this.CalcolaOreTeoriche(allData, IdDip_RapportoLavoro, day);
                        var oreRealiInt = this.CalcolaOreLavorate(allData, IdDip_RapportoLavoro, day);
                        valoreOre = oreTeoInt - oreRealiInt;
                        if (valoreOre < TimeSpan.Zero)
                            valoreOre = TimeSpan.Zero;
                        break;
                }

                // applica il segno del giustificativo
                if (parGiust.Segno == SignWithNeutral.Down)
                    valoreOre = valoreOre.Negate();

                // accumula
                if (causaliAccumulate.ContainsKey(idCausale))
                    causaliAccumulate[idCausale] += valoreOre;
                else
                    causaliAccumulate[idCausale] = valoreOre;
            }



            // -- B) Causali da Ore Lavorate per Intervallo ------------------------

            // -- B) Causali da Ore Lavorate per Intervallo ------------------------

            var daySlot = allData.OrariSchema_4User_OutModel.DaySlots
                .FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro
                                   && ds.Data.Date == day.Date);

            if (daySlot != null && daySlot.Orari.Count > 0)
            {
                var orarioBase = daySlot.Orari.OrderBy(o => o.ZOrder).First();

                var parOrario = allData.OrariSchema_4User_OutModel.ParOrario
                    .FirstOrDefault(o => o.Id == orarioBase.IdPar_Orario);

                if (parOrario != null)
                {
                    var coppie = allData.OrariSchema_4User_OutModel.Par_OrarioIntervalloHH
                        .Where(hh => hh.IdPar_Orario == parOrario.Id)
                        .OrderBy(hh => hh.NumCoppia)
                        .ToList();

                    var timbrature = Get_Timbrature_Giorno(allData, IdDip_RapportoLavoro, day);

                    // scorre le coppie timbratura E/U in sequenza
                    for (int i = 0; i + 1 < timbrature.Count; i += 2)
                    {
                        var entrata = timbrature[i];
                        var uscita = timbrature[i + 1];

                        if (entrata.TimbraturaTipo != TipoTimbratura.Entrata ||
                            uscita.TimbraturaTipo != TipoTimbratura.Uscita)
                            continue;

                        if (!entrata.TimbraturaArrotondata.HasValue || !uscita.TimbraturaArrotondata.HasValue)
                            continue;

                        // intervallo reale della timbratura (in minuti dal giorno)
                        var tStart = entrata.TimbraturaArrotondata.Value.TimeOfDay;
                        var tEnd = uscita.TimbraturaArrotondata.Value.TimeOfDay;

                        if (tEnd <= tStart)
                            continue;

                        // per ogni coppia del profilo, calcola la sovrapposizione con l'intervallo reale
                        foreach (var coppia in coppie)
                        {
                            if (coppia.IdCausale_HH_Lav == 0)
                                continue;

                            if (!coppia.Dalle.HasValue || !coppia.Alle.HasValue)
                                continue;

                            if (coppia.Alle.Value == coppia.Dalle.Value)
                                continue;

                            var cStart = coppia.Dalle.Value.ToTimeSpan();
                            var cEnd = coppia.Alle.Value.ToTimeSpan();

                            // sovrapposizione tra [tStart, tEnd] e [cStart, cEnd]
                            var overlapStart = tStart > cStart ? tStart : cStart;
                            var overlapEnd = tEnd < cEnd ? tEnd : cEnd;

                            if (overlapEnd <= overlapStart)
                                continue; // nessuna sovrapposizione

                            var oreLavorate = overlapEnd - overlapStart;

                            if (oreLavorate > TimeSpan.Zero)
                            {
                                int idCausale = coppia.IdCausale_HH_Lav;

                                if (causaliAccumulate.ContainsKey(idCausale))
                                    causaliAccumulate[idCausale] += oreLavorate;
                                else
                                    causaliAccumulate[idCausale] = oreLavorate;
                            }
                        }
                    }
                }
            }


            //var daySlot = allData.OrariSchema_4User_OutModel.DaySlots.FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro && 
            //                                                                               ds.Data.Date == day.Date);

            //if (daySlot != null && daySlot.Orari.Count > 0)
            //{
            //    var orarioBase = daySlot.Orari.OrderBy(o => o.ZOrder).First();

            //    var parOrario = allData.OrariSchema_4User_OutModel.ParOrario.FirstOrDefault(o => o.Id == orarioBase.IdPar_Orario);

            //    if (parOrario != null)
            //    {
            //        var coppie = allData.OrariSchema_4User_OutModel.Par_OrarioIntervalloHH.Where(hh => hh.IdPar_Orario == parOrario.Id)
            //                                                                              .OrderBy(hh => hh.NumCoppia)
            //                                                                              .ToList();

            //        // Entrata/Uscita ordinate e abbinale in coppie sequenziali
            //        var timbrature = Get_Timbrature_Giorno(allData, IdDip_RapportoLavoro, day);

            //        // scorre le coppie in sequenza: pos pari = Entrata, pos dispari = Uscita
            //        for (int i = 0; i + 1 < timbrature.Count; i += 2)
            //        {
            //            var entrata = timbrature[i];
            //            var uscita = timbrature[i + 1];

            //            if (entrata.TimbraturaTipo != TipoTimbratura.Entrata ||
            //                uscita.TimbraturaTipo != TipoTimbratura.Uscita)
            //                continue;

            //            // la coppia del profilo per questa posizione (i/2)
            //            int idxCoppia = i / 2;
            //            if (idxCoppia >= coppie.Count)
            //                idxCoppia = coppie.Count - 1;

            //            var coppia = coppie[idxCoppia];

            //            if (coppia.IdCausale_HH_Lav == 0)
            //                continue;

            //            var oreLavorate = uscita.TimbraturaArrotondata!.Value - entrata.TimbraturaArrotondata!.Value;

            //            if (oreLavorate > TimeSpan.Zero)
            //            {
            //                int idCausale = coppia.IdCausale_HH_Lav;

            //                if (causaliAccumulate.ContainsKey(idCausale))
            //                    causaliAccumulate[idCausale] += oreLavorate;
            //                else
            //                    causaliAccumulate[idCausale] = oreLavorate;
            //            }
            //        }
            //    }
            //}
            // -- Scrittura causali in allData -------------------------------------


            // recupera le causali pre-esistenti per questo giorno/rapporto
            var causaliEsistenti = allData.Dip_GG_AllData_OutModel.Dip_GG_Causali
                .Where(c => c.IdDip_RapportoLavoro == IdDip_RapportoLavoro
                         && c.Data.Date == day.Date)
                .ToList();

            // 1) marca tutte le pre-esistenti come ToBeDeleted
            foreach (var esistente in causaliEsistenti)
            {
                esistente.ToBeDeleted = true;
            }

            // 2) per ogni causale calcolata, cerca se esiste già → aggiorna, altrimenti aggiungi
            foreach (var kvp in causaliAccumulate)
            {
                var durataAssoluta = kvp.Value.Duration(); // valore assoluto per TimeOnly
                var nuovoValore = TimeOnly.FromTimeSpan(durataAssoluta);

                var causaleEsistente = causaliEsistenti
                    .FirstOrDefault(c => c.IdPar_Causali == kvp.Key);

                if (causaleEsistente != null)
                {
                    // esiste già: aggiorna il valore e rimuovi il flag ToBeDeleted
                    causaleEsistente.Valore = nuovoValore;
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
        }
        private List<Dip_GG_TimbraturaModel> Get_Timbrature_Giorno(Timesheet_AllData_OutModel allData, int IdDip_RapportoLavoro, DateTime day)
        {
            var timbrature = allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura.Where(t => t.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                                          t.GiornoCompetenza.Date == day.Date &&
                                                                                          t.TimbraturaArrotondata.HasValue && (t.TimbraturaTipo == TipoTimbratura.Entrata || t.TimbraturaTipo == TipoTimbratura.Uscita))
                                                                              .OrderBy(t => t.TimbraturaArrotondata)
                                                                              .ToList();

            return timbrature;
        }


        private class CoppiaTMP
        {
            public TimeOnly? HH { get; set; }
            public TimeOnly HH_Limite_SX { get; set; }
            public TimeOnly HH_Limite_DX { get; set; }
            public Boolean Check { get; set; }
            public TimeRoundInterval Arrotondamento { get; set; }
            public RoundDirection Arrotondamento_Verso { get; set; }
        }
    }

    public interface ITimeSheet_EngineService : IServiceBase
    {
        Task<GenericResult<TimeSheet_CalculateOutModel>> Calculate(GenericRequest<TimeSheet_CalculateInModel> model, bool isSubProcess);
        Task<GenericResult<OrariSchema_4User_OutModel>> Get_OrariSchema_4User(GenericRequest<OrariSchema_4User_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_GG_AllData_OutModel>> Dip_GG_AllData_AllData(GenericRequest<Dip_GG_AllData_InModel> model, bool isSubProcess);
        Task<GenericResult<Timesheet_AllData_OutModel>> Get_Timesheet_AllData(GenericRequest<Timesheet_AllData_InModel> model, bool isSubProcess);
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
