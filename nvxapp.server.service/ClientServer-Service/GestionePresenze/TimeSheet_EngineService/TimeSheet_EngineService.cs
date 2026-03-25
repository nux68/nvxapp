using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
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
                                            await CalcolaGiorno(model.Data.TimeSheet_Calculate, rapporto_calc, giorno, AllData.Data);
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
                        retVal.Dip_GG_Timbratura = res_Timbrature.Data.Dip_GG_Timbratura;

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




        private async Task CalcolaGiorno(TimeSheet_CalculateModel timeSheet_CalculateModel, Dip_RapportoLavoroModel rapporto_calc, DateTime giorno, Timesheet_AllData_OutModel AllData)
        {



            var dip_GG_Result = AllData.Dip_GG_AllData_OutModel.Dip_GG_Result.Where(x => x.Data == giorno).FirstOrDefault();
            if (dip_GG_Result != null)
            {
                dip_GG_Result.HH_Teo = TimeOnly.FromTimeSpan(this.CalcolaOreTeoriche(AllData.OrariSchema_4User_OutModel, rapporto_calc.Id, giorno));
            }


            // DaySlot del giorno corrente per questo rapporto
            var daySlot_calc = AllData.OrariSchema_4User_OutModel.DaySlots
                .FirstOrDefault(ds => ds.IdDip_RapportoLavoro == rapporto_calc.Id
                                    && ds.Data.Date == giorno.Date);



            //// causali del giorno per questo rapporto
            //var causali_calc = AllData.Dip_GG_AllData_OutModel.Dip_GG_Causali
            //    .Where(c => c.IdDip_RapportoLavoro == rapporto_calc.Id
            //                && c.Data.Date == giorno.Date)
            //    .ToList();



            //// richieste che coprono il giorno corrente per questo rapporto
            //var richieste_calc = AllData.Dip_GG_AllData_OutModel.Dip_GG_Richiesta
            //    .Where(r => r.IdDip_RapportoLavoro == rapporto_calc.Id)
            //    .ToList();


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
                GeneraTimbratureMancanti(AllData.OrariSchema_4User_OutModel, AllData, rapporto_calc.Id, giorno);
            }

            AssegnaVersoTimbrature(AllData.OrariSchema_4User_OutModel, AllData, rapporto_calc.Id, giorno);

            return;
        }
        private async Task SaveData(Timesheet_AllData_OutModel AllData)
        {

            foreach (var item in AllData.Dip_GG_AllData_OutModel.Dip_GG_Result)
            {
                if (item.IsHashChanged(item.Hash))
                {
                    var req_1 = new GenericRequest<Dip_GG_ResultPutInModel>();
                    req_1.Data.Dip_GG_Result = item;
                    req_1.Data.IdDip_RapportoLavoro = item.IdDip_RapportoLavoro;
                    await _dip_GG_ResultService.Dip_GG_ResultPut(req_1, true);
                }
            }

            foreach (var item in AllData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura)
            {
                if (item.IsHashChanged(item.Hash))
                {
                    var req_1 = new GenericRequest<Dip_GG_TimbraturaPutInModel>();
                    req_1.Data.Dip_GG_Timbratura = item;
                    req_1.Data.IdDip_RapportoLavoro = item.IdDip_RapportoLavoro;
                    await _dip_GG_TimbraturaService.Dip_GG_TimbraturaPut(req_1, true);
                }
            }

            return;
        }

        private TimeSpan CalcolaOreTeoriche(OrariSchema_4User_OutModel orariSchema, int IdDip_RapportoLavoro, DateTime day)
        {
            // 1. Individua il DaySlot per questo rapporto e questo giorno
            var daySlot = orariSchema.DaySlots
                .FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro
                                   && ds.Data.Date == day.Date);

            if (daySlot == null || daySlot.Orari.Count == 0)
                return TimeSpan.Zero;

            // 2. Prende solo la riga orario con ZOrder più basso (orario base, ZOrder=1).
            //    Gli override (ZOrder > 1) sono applicati a livello di giustificativo, non qui.
            var orarioBase = daySlot.Orari
                .OrderBy(o => o.ZOrder)
                .First();

            // 3. Recupera il Par_Orario corrispondente
            var parOrario = orariSchema.ParOrario
                .FirstOrDefault(o => o.Id == orarioBase.IdPar_Orario);

            if (parOrario == null)
                return TimeSpan.Zero;

            // 4. Recupera tutte le coppie (Par_OrarioIntervalloHH) per questo orario,
            //    ordinate per NumCoppia (es. coppia 1 = 08:00-12:00, coppia 2 = 14:00-18:00)
            var coppie = orariSchema.Par_OrarioIntervalloHH
                .Where(hh => hh.IdPar_Orario == parOrario.Id)
                .OrderBy(hh => hh.NumCoppia)
                .ToList();

            if (coppie.Count == 0)
                return TimeSpan.Zero;

            // 5. Somma la durata di ogni coppia valida (Dalle e Alle devono essere entrambe valorizzate)
            var oreTeoriche = TimeSpan.Zero;

            foreach (var coppia in coppie)
            {
                if (coppia.Dalle.HasValue && coppia.Alle.HasValue)
                {
                    // Alle e Dalle sono TimeOnly: la differenza è sempre positiva se Alle > Dalle
                    var durataCoppia = coppia.Alle.Value.ToTimeSpan() - coppia.Dalle.Value.ToTimeSpan();

                    if (durataCoppia > TimeSpan.Zero)
                        oreTeoriche += durataCoppia;
                }
            }

            return oreTeoriche;
        }
        private async Task ApprovaRichiesta(TipoRichiesta tipoRichiesta, Dip_GG_AllData_OutModel dip_GG_AllData, int IdDip_RapportoLavoro, DateTime day)
        {


            var req_1 = new GenericRequest<Dip_GG_Richiesta_SetState_InModel>();

            req_1.Data.RichiestaStato = StatoRichiesta.Approvata;
            req_1.Data.FromHR = true;
            req_1.Data.IdDip_GG_Richiesta = dip_GG_AllData.Dip_GG_Richiesta.Where(x => (x.RichiestaStato == StatoRichiesta.Immessa || x.RevocaStato == StatoRichiesta.Immessa) &&
                                                                                    x.IdDip_RapportoLavoro == IdDip_RapportoLavoro &&
                                                                                    x.Data == x.DataA &&
                                                                                    DateTime.Parse(x.Data) == day)
                                                                           .Select(x => x.Id)
                                                                           .ToList();

            if (req_1.Data.IdDip_GG_Richiesta.Count == 0)
                return;

            await _dip_GG_RichiestaService.SetState(req_1, true);

        }

        /// <summary>
        /// Genera le timbrature mancanti (Entrata e Uscita) per un giorno passato,
        /// basandosi sulle coppie di orario definite in <see cref="OrariSchema_4User_OutModel"/>.
        /// Per ogni coppia (Dalle/Alle) viene creata una timbratura sintetica solo se
        /// non ne esiste già una reale nel relativo intervallo di tolleranza.
        /// Le timbrature generate vengono aggiunte in-memory ad <paramref name="allData"/>
        /// e persistite direttamente sul repository.
        /// </summary>
        private void GeneraTimbratureMancanti(OrariSchema_4User_OutModel orariSchema,
                                              Timesheet_AllData_OutModel allData,
                                              int IdDip_RapportoLavoro,
                                              DateTime day)
        {
            // Genera solo per giorni precedenti a oggi
            if (day.Date >= DateTime.Today)
                return;

            // 1. Trova il DaySlot del giorno per questo rapporto
            var daySlot = orariSchema.DaySlots
                .FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro
                                   && ds.Data.Date == day.Date);

            if (daySlot == null || daySlot.Orari.Count == 0)
                return;

            // 2. Prende l'orario base (ZOrder minimo)
            var orarioBase = daySlot.Orari
                .OrderBy(o => o.ZOrder)
                .First();

            var parOrario = orariSchema.ParOrario
                .FirstOrDefault(o => o.Id == orarioBase.IdPar_Orario);

            if (parOrario == null)
                return;

            // 3. Recupera le coppie ordinate per NumCoppia
            var coppie = orariSchema.Par_OrarioIntervalloHH
                .Where(hh => hh.IdPar_Orario == parOrario.Id)
                .OrderBy(hh => hh.NumCoppia)
                .ToList();

            if (coppie.Count == 0)
                return;

            // 4. Timbrature già presenti per questo rapporto in questo giorno
            var timbratureEsistenti = allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura
                .Where(t => t.IdDip_RapportoLavoro == IdDip_RapportoLavoro
                         && t.GiornoCompetenza.Date == day.Date)
                .ToList();

            // 5. Per ogni coppia verifica se Entrata e Uscita sono già presenti
            foreach (var coppia in coppie)
            {
                if (!coppia.Dalle.HasValue || !coppia.Alle.HasValue)
                    continue;

                // se la durata della coppia è zero, l'orario non prevede lavoro → nessuna timbratura da generare
                if (coppia.Alle.Value == coppia.Dalle.Value)
                    continue;

                var dalleLimit_SX = (coppia.Dalle_Limite_SX ?? coppia.Dalle).Value;
                var dalleLimit_DX = (coppia.Dalle_Limite_DX ?? coppia.Dalle).Value;
                var alleLimit_SX = (coppia.Alle_Limite_SX ?? coppia.Alle).Value;
                var alleLimit_DX = (coppia.Alle_Limite_DX ?? coppia.Alle).Value;

                // ── Entrata ──────────────────────────────────────────────────────
                bool entrataPresente = timbratureEsistenti.Any(t => t.TimbraturaTipo != TipoTimbratura.Attivita &&
                                                                    TimeOnly.FromDateTime(t.Timbratura) >= dalleLimit_SX &&
                                                                    TimeOnly.FromDateTime(t.Timbratura) <= dalleLimit_DX);

                if (!entrataPresente)
                {
                    var timbraturaUscita = new Dip_GG_Timbratura() { IdDip_RapportoLavoro = IdDip_RapportoLavoro };
                    var timbraturaUscita_VM = _mapper.Map<Dip_GG_TimbraturaModel>(timbraturaUscita);

                    timbraturaUscita_VM.Timbratura = day.Date + coppia.Dalle.Value.ToTimeSpan();
                    timbraturaUscita_VM.TimbraturaOriginale = day.Date + coppia.Dalle.Value.ToTimeSpan();
                    timbraturaUscita_VM.GiornoCompetenza = day.Date;
                    timbraturaUscita_VM.TimbraturaTipo = TipoTimbratura.SenzaVerso;
                    timbraturaUscita_VM.RichiestaStato = StatoRichiesta.Diretta;

                    allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura.Add(timbraturaUscita_VM);
                }

                // ── Uscita ───────────────────────────────────────────────────────
                bool uscitaPresente = timbratureEsistenti.Any(t => t.TimbraturaTipo != TipoTimbratura.Attivita &&
                                                                   TimeOnly.FromDateTime(t.Timbratura) >= alleLimit_SX &&
                                                                   TimeOnly.FromDateTime(t.Timbratura) <= alleLimit_DX);

                if (!uscitaPresente)
                {
                    var timbraturaUscita = new Dip_GG_Timbratura() { IdDip_RapportoLavoro = IdDip_RapportoLavoro };
                    var timbraturaUscita_VM = _mapper.Map<Dip_GG_TimbraturaModel>(timbraturaUscita);

                    timbraturaUscita_VM.Timbratura = day.Date + coppia.Alle.Value.ToTimeSpan();
                    timbraturaUscita_VM.TimbraturaOriginale = day.Date + coppia.Alle.Value.ToTimeSpan();
                    timbraturaUscita_VM.GiornoCompetenza = day.Date;
                    timbraturaUscita_VM.TimbraturaTipo = TipoTimbratura.SenzaVerso;
                    timbraturaUscita_VM.RichiestaStato = StatoRichiesta.Diretta;

                    allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura.Add(timbraturaUscita_VM);
                }


            }

            //await Task.Delay(DelayAsyncMethod);
            //await Task.Delay(0);

        }



        /// <summary>
        /// Assegna il verso (Entrata/Uscita) alle timbrature del giorno per un dato rapporto,
        /// basandosi sulla finestra di tolleranza di ogni coppia definita in <see cref="OrariSchema_4User_OutModel"/>.
        /// Le timbrature che cadono nella finestra Dalle diventano Entrata,
        /// quelle nella finestra Alle diventano Uscita.
        /// Le modifiche vengono persiste sul repository.
        /// </summary>
        private void AssegnaVersoTimbrature(OrariSchema_4User_OutModel orariSchema,
                                            Timesheet_AllData_OutModel allData,
                                            int IdDip_RapportoLavoro,
                                            DateTime day)
        {
            // 1. Trova il DaySlot del giorno per questo rapporto
            var daySlot = orariSchema.DaySlots
                .FirstOrDefault(ds => ds.IdDip_RapportoLavoro == IdDip_RapportoLavoro
                                   && ds.Data.Date == day.Date);

            if (daySlot == null || daySlot.Orari.Count == 0)
                return;

            // 2. Orario base (ZOrder minimo)
            var orarioBase = daySlot.Orari
                .OrderBy(o => o.ZOrder)
                .First();

            var parOrario = orariSchema.ParOrario
                .FirstOrDefault(o => o.Id == orarioBase.IdPar_Orario);

            if (parOrario == null)
                return;

            // 3. Coppie ordinate per NumCoppia
            var coppie = orariSchema.Par_OrarioIntervalloHH
                .Where(hh => hh.IdPar_Orario == parOrario.Id)
                .OrderBy(hh => hh.NumCoppia)
                .ToList();

            if (coppie.Count == 0)
                return;

            // 4. Timbrature del giorno per questo rapporto
            var timbratureDelGiorno = allData.Dip_GG_AllData_OutModel.Dip_GG_Timbratura
                .Where(t => t.IdDip_RapportoLavoro == IdDip_RapportoLavoro
                         && t.GiornoCompetenza.Date == day.Date)
                .ToList();

            if (timbratureDelGiorno.Count == 0)
                return;

            // 5. Per ogni coppia abbina le timbrature alla finestra Dalle (Entrata) o Alle (Uscita)
            foreach (var coppia in coppie)
            {
                if (!coppia.Dalle.HasValue || !coppia.Alle.HasValue)
                    continue;

                if (coppia.Alle.Value == coppia.Dalle.Value)
                    continue;

                var dalleLimit_SX = (coppia.Dalle_Limite_SX ?? coppia.Dalle).Value;
                var dalleLimit_DX = (coppia.Dalle_Limite_DX ?? coppia.Dalle).Value;
                var alleLimit_SX = (coppia.Alle_Limite_SX ?? coppia.Alle).Value;
                var alleLimit_DX = (coppia.Alle_Limite_DX ?? coppia.Alle).Value;

                foreach (var timbratura in timbratureDelGiorno)
                {
                    // salta timbrature con verso già assegnato correttamente
                    if (timbratura.TimbraturaTipo == TipoTimbratura.Entrata ||
                        timbratura.TimbraturaTipo == TipoTimbratura.Uscita)
                        continue;

                    var oraTimbr = TimeOnly.FromDateTime(timbratura.Timbratura);
                    TipoTimbratura? nuovoVerso = null;

                    if (oraTimbr >= dalleLimit_SX && oraTimbr <= dalleLimit_DX)
                        nuovoVerso = TipoTimbratura.Entrata;
                    else if (oraTimbr >= alleLimit_SX && oraTimbr <= alleLimit_DX)
                        nuovoVerso = TipoTimbratura.Uscita;

                    if (nuovoVerso.HasValue && nuovoVerso.Value != timbratura.TimbraturaTipo)
                    {
                        timbratura.TimbraturaTipo = nuovoVerso.Value;
                    }
                }
            }
        }

    }

    public interface ITimeSheet_EngineService : IServiceBase
    {
        Task<GenericResult<TimeSheet_CalculateOutModel>> Calculate(GenericRequest<TimeSheet_CalculateInModel> model, bool isSubProcess);
        Task<GenericResult<OrariSchema_4User_OutModel>> Get_OrariSchema_4User(GenericRequest<OrariSchema_4User_InModel> model, bool isSubProcess);
        Task<GenericResult<Dip_GG_AllData_OutModel>> Dip_GG_AllData_AllData(GenericRequest<Dip_GG_AllData_InModel> model, bool isSubProcess);
        Task<GenericResult<Timesheet_AllData_OutModel>> Get_Timesheet_AllData(GenericRequest<Timesheet_AllData_InModel> model, bool isSubProcess);
    }







}
