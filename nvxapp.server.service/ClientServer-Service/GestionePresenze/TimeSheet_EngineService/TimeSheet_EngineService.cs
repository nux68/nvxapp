using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService;
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
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IDip_ProfiloOrarioRepository _dip_ProfiloOrarioRepository;
        private readonly IPar_OrarioService _par_OrarioService;
        private readonly IPar_ProfiloOrarioService _par_ProfiloOrarioService;
        private readonly IDip_RapportoLavoroService _dip_RapportoLavoroService;

        public TimeSheet_EngineService(IMapper mapper,
                                      UserManager<ApplicationUser> userManager,
                                      IAspNetUsersRepository aspNetUsersRepository,
                                      IOptions<JwtParameter> jwtParameter,
                                      IHttpContextAccessor httpContextAccessor,
                                      IConfiguration configuration,

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
            _par_ProfiloOrarioService = par_ProfiloOrarioService;
            _dip_RapportoLavoroService = dip_RapportoLavoroService;
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
                    var AllData = await PrepareAllData(model.Data.TimeSheet_Calculate.SelectedUserId, model.Data.TimeSheet_Calculate.Dal, model.Data.TimeSheet_Calculate.Al);

                    // ciclo su ogni utente selezionato
                    foreach (var userId_calc in model.Data.TimeSheet_Calculate.SelectedUserId)
                    {
                        // anagrafica dell'utente corrente
                        var anagrafica_calc = AllData.Dip_ProfiloOrario_Calculate.Dip_Anagrafica
                            .FirstOrDefault(a => a.IdAspNetUsers == userId_calc);

                        if (anagrafica_calc == null) continue;

                        // rapporti di lavoro dell'utente corrente
                        var rapporti_calc = AllData.Dip_ProfiloOrario_Calculate.Dip_RapportoLavoro
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
                               CalcolaGiorno(rapporto_calc, giorno, AllData);
                            }
                        }
                    }

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
        public virtual async Task<GenericResult<TimeSheet_All_Data_Container>> GetAllData(GenericRequest<TimeSheet_Calculate_GetAllData_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new TimeSheet_All_Data_Container();

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

                await Task.Delay(DelayAsyncMethod);
                return retVal;

            }, isSubProcess);
        }
        private async Task<AllData> PrepareAllData(List<string> UsersId, DateTime Dal, DateTime Al)
        {
            AllData data = new AllData();

            // 1) Recupera profili orario per il calcolo
            var req_ProfHHDip = new GenericRequest<TimeSheet_Calculate_GetAllData_InModel>();
            req_ProfHHDip.Data.Dal = Dal;
            req_ProfHHDip.Data.Al = Al;
            req_ProfHHDip.Data.UsersId = UsersId;

            var res_ProfHHDip = await this.GetAllData(req_ProfHHDip, true);
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
        private async void CalcolaGiorno(Dip_RapportoLavoroModel rapporto_calc ,DateTime giorno, AllData AllData)
        {
             // DaySlot del giorno corrente per questo rapporto
            var daySlot_calc = AllData.Dip_ProfiloOrario_Calculate.DaySlots
                .FirstOrDefault(ds => ds.IdDip_RapportoLavoro == rapporto_calc.Id
                                    && ds.Data.Date == giorno.Date);

            // timbrature del giorno per questo rapporto
            var timbrature_calc = AllData.Dip_GG_Timbratura
                .Where(t => t.IdDip_RapportoLavoro == rapporto_calc.Id
                            && t.GiornoCompetenza.Date == giorno.Date)
                .OrderBy(t => t.TimbraturaOriginale)
                .ToList();

            // causali del giorno per questo rapporto
            var causali_calc = AllData.Dip_GG_Causali
                .Where(c => c.IdDip_RapportoLavoro == rapporto_calc.Id
                            && c.Data.Date == giorno.Date)
                .ToList();

            // giustificativi del giorno per questo rapporto
            var giustificativi_calc = AllData.Dip_GG_Giustificativi
                .Where(g => g.IdDip_RapportoLavoro == rapporto_calc.Id
                            && g.Data.Date == giorno.Date)
                .ToList();

            // richieste che coprono il giorno corrente per questo rapporto
            var richieste_calc = AllData.Dip_GG_Richiesta
                .Where(r => r.IdDip_RapportoLavoro == rapporto_calc.Id)
                .ToList();
        }
        

    }

    public interface ITimeSheet_EngineService : IServiceBase
    {
        Task<GenericResult<TimeSheet_CalculateOutModel>> Calculate(GenericRequest<TimeSheet_CalculateInModel> model, bool isSubProcess);
        Task<GenericResult<TimeSheet_All_Data_Container>> GetAllData(GenericRequest<TimeSheet_Calculate_GetAllData_InModel> model, bool isSubProcess);
    }
}
