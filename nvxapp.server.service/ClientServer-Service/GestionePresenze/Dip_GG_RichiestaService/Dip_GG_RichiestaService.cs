using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using Newtonsoft.Json;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

/*  chi chiama cosa

USER     
	certellino
		GetAll4User
	richiesta
		GetAll4User
		
ADMIN		
	certellino
		GetAll4User
	richiesta
		GetAll4Admin
		
POWERADMIN			
	certellino
		GetAll4User
	richiesta
		GetAll4Admin	 

 */



namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService
{

    public class Dip_GG_RichiestaService : ServiceBase, IDip_GG_RichiestaService
    {
        private readonly IAccountService _accountService;
        private readonly IAz_CfgService _az_CfgService;
        private readonly IAz_SediRepartoService _az_SediRepartoService;


        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_GG_RichiestaRepository _dip_GG_RichiestaRepository;
        private readonly IDip_GG_TimbraturaRepository _dip_GG_TimbraturaRepository;
        private readonly IDip_GG_GiustificativiRepository _dip_GG_GiustificativiRepository;
        private readonly IDip_GG_NotaSpesaRepository _dip_GG_NotaSpesaRepository;

        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IAz_SediRepartoUserRepository _az_SediRepartoUserRepository;

        public Dip_GG_RichiestaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_CfgService az_CfgService,
                                  IAccountService accountService,
                                  IAz_SediRepartoService az_SediRepartoService,
                                  IAz_SediRepartoUserRepository az_SediRepartoUserRepository,
                                  IDip_AnagraficaRepository dip_AnagraficaRepository,
                                  IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IDip_GG_RichiestaRepository dip_GG_RichiestaRepository,
                                  IDip_GG_TimbraturaRepository dip_GG_TimbraturaRepository,
                                  IDip_GG_NotaSpesaRepository dip_GG_NotaSpesaRepository,
                                  IDip_GG_GiustificativiRepository dip_GG_GiustificativiRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _accountService = accountService;
            _az_CfgService = az_CfgService;
            _az_SediRepartoService = az_SediRepartoService;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _dip_GG_RichiestaRepository = dip_GG_RichiestaRepository;
            _dip_GG_TimbraturaRepository = dip_GG_TimbraturaRepository;
            _dip_GG_GiustificativiRepository = dip_GG_GiustificativiRepository;
            _dip_GG_NotaSpesaRepository = dip_GG_NotaSpesaRepository;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _az_SediRepartoUserRepository = az_SediRepartoUserRepository;

        }

        /// <summary>
        /// Ritorna la lista di Dip_GG_Richiesta NON filtrata secondo i criteri di approvazione, utile per una view utente e HR che deve vedere tutto
        /// </summary>
        /// <param name="model"></param>
        /// <param name="isSubProcess"></param>
        /// <returns></returns>
        public virtual async Task<GenericResult<Dip_GG_Richiesta_GetAll4User_OutModel>> GetAll4User(GenericRequest<Dip_GG_Richiesta_GetAll4User_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_GG_Richiesta_GetAll4User_OutModel();

                string userId = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? this.CurrentUserId : model.Data.IdAspNetUsers;

                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userId, true);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {
                    var idDip_RapportoLavoro = new List<int>();
                    idDip_RapportoLavoro.Add(user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id);

                    List<Dip_GG_Richiesta> richiesta = await Get_Dip_GG_Richiesta(idDip_RapportoLavoro, model.Data.Year, model.Data.Month);


                    retVal.Dip_GG_Richiesta = _mapper.Map<List<Dip_GG_RichiestaModel>>(richiesta);
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        /// <summary>
        /// Ritorna la lista di Dip_GG_Richiesta FILTRATA secondo i criteri di approvazione, utile per una view di amministratore con approvazione gerarchica
        /// </summary>
        /// <param name="model"></param>
        /// <param name="isSubProcess"></param>
        /// <returns></returns>
        public virtual async Task<GenericResult<Dip_GG_Richiesta_GetAll4Admin_OutModel>> GetAll4Admin(GenericRequest<Dip_GG_Richiesta_GetAll4Admin_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_GG_Richiesta_GetAll4Admin_OutModel();

                var req = new GenericRequest<UserCompanyListInModel>();
                var res = await _accountService.UserCompanyList(req, true);
                if (res.Success && res.Data != null)
                {
                    var UserCompanyList = res.Data.UserCompanyList;
                    List<string?> idAspNetUsers = UserCompanyList.Select(x => x.IdAspNetUsers).ToList();
                    List<int> idDip_Anagrafica = _dip_AnagraficaRepository.FindAll(x => idAspNetUsers.Contains(x.IdAspNetUsers)).Select(x => x.Id).ToList();
                    List<int> idDip_RapportoLavoro = _dip_RapportoLavoroRepository.FindAll(x => idDip_Anagrafica.Contains(x.IdDip_Anagrafica)).Select(x => x.Id).ToList();

                    List<Dip_GG_Richiesta> richiesta = await Get_Dip_GG_Richiesta(idDip_RapportoLavoro, model.Data.Year, model.Data.Month);


                    retVal.Dip_GG_Richiesta = _mapper.Map<List<Dip_GG_RichiestaModel>>(richiesta);
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Dip_GG_Richiesta_Send_OutModel>> Send(GenericRequest<Dip_GG_Richiesta_Send_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_GG_Richiesta_Send_OutModel();

                string userId = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? this.CurrentUserId : model.Data.IdAspNetUsers;

                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userId, true);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {
                    Dip_GG_Richiesta dip_GG_Richiesta = _mapper.Map<Dip_GG_Richiesta>(model.Data.Dip_GG_Richiesta);
                    dip_GG_Richiesta.IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id;

                    dip_GG_Richiesta.RichiestaStato = model.Data.FromHR ? StatoRichiesta.Approvata : StatoRichiesta.Immessa;

                    var RichiestaApprovazioneData = new List<Dip_GG_Richiesta_Stato_Cronology>();

                    var reqSediRep = new GenericRequest<Az_SediReparto_Get4User_InModel>();
                    reqSediRep.Data.IdAspNetUsers = userId;
                    var resSediRep = await _az_SediRepartoService.Get4User(reqSediRep, true);
                    if (resSediRep.Success && resSediRep.Data != null)
                    {
                        List<int> az_SediReparto = resSediRep.Data.Az_SediReparto.Select(x => x.Id).ToList();

                        var sediRepartoUser = _az_SediRepartoUserRepository.FindAll(x => az_SediReparto.Contains(x.IdAz_SediReparto) && x.EnabledToAdmin && x.EnabledToApproval)
                                                                           .GroupBy(x => x.IdAspNetUsers)
                                                                           .Select(g => new
                                                                           {
                                                                               IdAspNetUsers = g.Key,
                                                                               ApprovalZOrder = g.Min(x => x.ApprovalZOrder) // oppure Max, Average, First, ecc.
                                                                           })
                                                                           .OrderBy(x => x.ApprovalZOrder)
                                                                           .ToList();

                        if (sediRepartoUser.Count > 0)
                        {
                            foreach (var item in sediRepartoUser)
                            {
                                //TODO ELIMINARE serve x debug
                                var applicationUser = await _userManager.FindByIdAsync(item.IdAspNetUsers);
                                if (applicationUser != null)
                                {
                                    StatoRichiesta statoRichiesta = StatoRichiesta.Immessa;

                                    if (model.Data.FromHR)
                                        if (item.IdAspNetUsers == CurrentUserId)
                                            statoRichiesta = StatoRichiesta.Approvata;



                                    RichiestaApprovazioneData.Add(new Dip_GG_Richiesta_Stato_Cronology()
                                    {
                                        IdAspNetUsers = item.IdAspNetUsers,
                                        RichiestaStato = statoRichiesta,
                                        Data = DateTime.Now,
                                        //TODO ELIMINARE serve x debug
                                        UsrName_DEB = applicationUser.UserName
                                    });
                                }

                            }

                            var RevocaApprovazioneData = new List<Dip_GG_Richiesta_Stato_Cronology>();

                            dip_GG_Richiesta.RichiestaApprovazioneData = JsonConvert.SerializeObject(RichiestaApprovazioneData, Formatting.Indented);
                            dip_GG_Richiesta.RevocaApprovazioneData = JsonConvert.SerializeObject(RevocaApprovazioneData, Formatting.Indented);

                            dip_GG_Richiesta = await _dip_GG_RichiestaRepository.UpsertAsync(dip_GG_Richiesta);

                            switch (dip_GG_Richiesta.RichiestaTipo)
                            {
                                case TipoRichiesta.Timbratura:
                                    await Add_Dip_GG_Timbratura(dip_GG_Richiesta, user_DATA_COMB_DipAna_DipRapp);
                                    break;

                                case TipoRichiesta.Giustificativo:
                                    await Add_Dip_GG_Giustificativi(dip_GG_Richiesta, user_DATA_COMB_DipAna_DipRapp);

                                    break;
                                case TipoRichiesta.NotaSpesa:
                                    break;
                            }
                        }
                        else
                        {
                            //TODO GESTIRE COMUNICAZIONE SERVER CON ERRORI != EXCEPTION
                            retVal.Messages.Add(new Message("Nessun amministratore abilitato all'approvazione", MessageType.Exception));
                        }

                    }

                }



                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_Richiesta_SetState_OutModel>> SetState(GenericRequest<Dip_GG_Richiesta_SetState_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var retVal = new Dip_GG_Richiesta_SetState_OutModel();




                var reqAz_Cfg = new GenericRequest<Az_Cfg_Get_InModel>();
                var resAz_Cfg = await _az_CfgService.Az_CfgGet(reqAz_Cfg, true);

                if (resAz_Cfg.Success && resAz_Cfg.Data != null && resAz_Cfg.Data.Az_Cfg != null)
                {
                    var richieste = _dip_GG_RichiestaRepository.FindAll(x => model.Data.IdDip_GG_Richiesta.Contains(x.Id)).ToList();
                    foreach (var curr_richiesta in richieste)
                    {

                        for (var idxStato = 0; idxStato <= 1; idxStato++) // 0= richiesta, 1=revoca
                        {
                            StatoRichiesta? Stato = idxStato == 0 ? curr_richiesta.RichiestaStato : curr_richiesta.RevocaStato;
                            string? jsonReq = idxStato == 0 ? curr_richiesta.RichiestaApprovazioneData : curr_richiesta.RevocaApprovazioneData;

                            switch (model.Data.RichiestaStato)
                            {
                                case StatoRichiesta.Immessa:
                                    if (idxStato == 0)
                                    {  //qui gestisco solo 'immissione della revoca
                                        continue;
                                    }
                                    else
                                    {
                                        // prendo la lista che ho usato nell'approvazione , azzero lo stato e la metto in RevocaApprovazioneData
                                        List<Dip_GG_Richiesta_Stato_Cronology> listaAppr = !string.IsNullOrEmpty(curr_richiesta.RichiestaApprovazioneData) ? JsonConvert.DeserializeObject<List<Dip_GG_Richiesta_Stato_Cronology>>(curr_richiesta.RichiestaApprovazioneData) ?? new List<Dip_GG_Richiesta_Stato_Cronology>() : new List<Dip_GG_Richiesta_Stato_Cronology>();

                                        foreach (var o in listaAppr)
                                        {
                                            o.RichiestaStato = StatoRichiesta.Immessa;
                                            o.Data = DateTime.Now;
                                        }

                                        curr_richiesta.RevocaApprovazioneData = JsonConvert.SerializeObject(listaAppr, Formatting.Indented);
                                        curr_richiesta.RevocaStato = model.Data.RichiestaStato;
                                        await _dip_GG_RichiestaRepository.UpdateAsync(curr_richiesta);
                                        continue;
                                    }
                                case StatoRichiesta.Cancellata:
                                    // la cancellazione è possibile solo se la richiesta non è stata approvata


                                    if (idxStato == 0)
                                    {
                                        if (Stato == StatoRichiesta.Immessa || Stato == StatoRichiesta.ApprovazioneInCorso || Stato == StatoRichiesta.ParzialmenteApprovata)
                                        {
                                            curr_richiesta.RichiestaStato = model.Data.RichiestaStato;
                                            await _dip_GG_RichiestaRepository.UpdateAsync(curr_richiesta);

                                            //posso cancellare i dettagli
                                            await Dip_GG_Richiesta_Canc_Dettaglio(curr_richiesta);
                                        }
                                    }
                                    else
                                    {

                                        curr_richiesta.RevocaStato = null;
                                        curr_richiesta.RevocaApprovazioneData = JsonConvert.SerializeObject(new List<Dip_GG_Richiesta_Stato_Cronology>(), Formatting.Indented); ;
                                        await _dip_GG_RichiestaRepository.UpdateAsync(curr_richiesta);
                                    }



                                    break;

                                case StatoRichiesta.Approvata:
                                    if (Stato == StatoRichiesta.Immessa || Stato == StatoRichiesta.ApprovazioneInCorso || Stato == StatoRichiesta.ParzialmenteApprovata)
                                    {
                                        List<Dip_GG_Richiesta_Stato_Cronology> listaAppr = !string.IsNullOrEmpty(jsonReq) ? JsonConvert.DeserializeObject<List<Dip_GG_Richiesta_Stato_Cronology>>(jsonReq) ?? new List<Dip_GG_Richiesta_Stato_Cronology>() : new List<Dip_GG_Richiesta_Stato_Cronology>();

                                        var iAppr = listaAppr.FirstOrDefault(x => x.IdAspNetUsers == this.CurrentUserId);
                                        if (iAppr != null)
                                        {
                                            iAppr.RichiestaStato = StatoRichiesta.Approvata;
                                            iAppr.Data = DateTime.Now;
                                        }

                                        StatoRichiesta? RichiestaStato_Orig;
                                        if (idxStato == 0)
                                            RichiestaStato_Orig = curr_richiesta.RichiestaStato;
                                        else
                                            RichiestaStato_Orig = curr_richiesta.RevocaStato;

                                        switch (resAz_Cfg.Data.Az_Cfg.ApprovazioneTipo)
                                        {
                                            case TipoApprovazione.SigleAdmin:
                                                if (idxStato == 0)
                                                    curr_richiesta.RichiestaStato = model.Data.RichiestaStato;
                                                else
                                                    curr_richiesta.RevocaStato = model.Data.RichiestaStato;
                                                break;
                                            case TipoApprovazione.AllAdmin:
                                            case TipoApprovazione.AllAdminHierarchy:
                                                if (listaAppr.Where(x => x.RichiestaStato != StatoRichiesta.Approvata).Any())
                                                    //se FromHR forzatura
                                                    if (idxStato == 0)
                                                        curr_richiesta.RichiestaStato = model.Data.FromHR ? model.Data.RichiestaStato : StatoRichiesta.ApprovazioneInCorso;
                                                    else
                                                        curr_richiesta.RevocaStato = model.Data.FromHR ? model.Data.RichiestaStato : StatoRichiesta.ApprovazioneInCorso;
                                                else
                                                    if (idxStato == 0)
                                                    curr_richiesta.RichiestaStato = model.Data.RichiestaStato;
                                                else
                                                    curr_richiesta.RevocaStato = model.Data.RichiestaStato;


                                                break;
                                        }
                                        if (idxStato == 0)
                                            curr_richiesta.RichiestaApprovazioneData = JsonConvert.SerializeObject(listaAppr, Formatting.Indented);
                                        else
                                            curr_richiesta.RevocaApprovazioneData = JsonConvert.SerializeObject(listaAppr, Formatting.Indented);

                                        await _dip_GG_RichiestaRepository.UpdateAsync(curr_richiesta);

                                        if (idxStato == 0 && RichiestaStato_Orig != curr_richiesta.RichiestaStato) // Aggiorno i dettagli solo se lo stato è cambiato  e solo per le richieste approvazioni
                                        {
                                            switch (curr_richiesta.RichiestaTipo)
                                            {
                                                case TipoRichiesta.Timbratura:
                                                    var timbr = _dip_GG_TimbraturaRepository.FindAll(x => x.IdDip_GG_Richiesta == curr_richiesta.Id).ToList();
                                                    foreach (var idemDett in timbr)
                                                    {
                                                        idemDett.RichiestaStato = curr_richiesta.RichiestaStato;
                                                        await _dip_GG_TimbraturaRepository.UpsertAsync(idemDett);
                                                    }


                                                    break;
                                                case TipoRichiesta.Giustificativo:

                                                    var just = _dip_GG_GiustificativiRepository.FindAll(x => x.IdDip_GG_Richiesta == curr_richiesta.Id).ToList();
                                                    foreach (var idemDett in just)
                                                    {
                                                        idemDett.RichiestaStato = curr_richiesta.RichiestaStato;
                                                        await _dip_GG_GiustificativiRepository.UpsertAsync(idemDett);
                                                    }

                                                    break;
                                                case TipoRichiesta.NotaSpesa:

                                                    var nota = _dip_GG_NotaSpesaRepository.FindAll(x => x.IdDip_GG_Richiesta == curr_richiesta.Id).ToList();
                                                    foreach (var idemDett in nota)
                                                    {
                                                        idemDett.RichiestaStato = curr_richiesta.RichiestaStato;
                                                        await _dip_GG_NotaSpesaRepository.UpsertAsync(idemDett);
                                                    }

                                                    break;
                                            }
                                        }
                                        else
                                        {
                                            switch (resAz_Cfg.Data.Az_Cfg.ApprovazioneTipo)
                                            {
                                                case TipoApprovazione.SigleAdmin:
                                                        await Dip_GG_Richiesta_Canc_Dettaglio(curr_richiesta);
                                                    break;
                                                case TipoApprovazione.AllAdmin:
                                                case TipoApprovazione.AllAdminHierarchy:
                                                       //approvazione revoca, cancello i dettagli
                                                        if (listaAppr.Where(x => x.RichiestaStato != StatoRichiesta.Approvata).Count() == 0)
                                                            await Dip_GG_Richiesta_Canc_Dettaglio(curr_richiesta);
                                                    break;
                                            }

                                         
                                        }

                                    }
                                    break;
                                case StatoRichiesta.Rifiutata:
                                    if (Stato == StatoRichiesta.Immessa || Stato == StatoRichiesta.ApprovazioneInCorso || Stato == StatoRichiesta.ParzialmenteApprovata)
                                    {
                                        List<Dip_GG_Richiesta_Stato_Cronology> listaAppr = !string.IsNullOrEmpty(jsonReq) ? JsonConvert.DeserializeObject<List<Dip_GG_Richiesta_Stato_Cronology>>(jsonReq) ?? new List<Dip_GG_Richiesta_Stato_Cronology>() : new List<Dip_GG_Richiesta_Stato_Cronology>();

                                        var iAppr = listaAppr.FirstOrDefault(x => x.IdAspNetUsers == this.CurrentUserId);
                                        if (iAppr != null)
                                        {
                                            iAppr.RichiestaStato = StatoRichiesta.Rifiutata;
                                            iAppr.Data = DateTime.Now;
                                        }

                                        StatoRichiesta? RichiestaStato_Orig;
                                        if (idxStato == 0)
                                            RichiestaStato_Orig = curr_richiesta.RichiestaStato;
                                        else
                                            RichiestaStato_Orig = curr_richiesta.RevocaStato;

                                        switch (resAz_Cfg.Data.Az_Cfg.ApprovazioneTipo)
                                        {
                                            case TipoApprovazione.SigleAdmin:
                                                if (idxStato == 0)
                                                    curr_richiesta.RichiestaStato = model.Data.RichiestaStato;
                                                else
                                                    curr_richiesta.RevocaStato = model.Data.RichiestaStato;
                                                break;
                                            case TipoApprovazione.AllAdmin:
                                            case TipoApprovazione.AllAdminHierarchy:
                                                if (listaAppr.Where(x => x.RichiestaStato == StatoRichiesta.Rifiutata).Any())
                                                    // se almeno uno ha rifiutato, allora la richiesta è rifiutata
                                                    if (idxStato == 0)
                                                        curr_richiesta.RichiestaStato = StatoRichiesta.Rifiutata;
                                                    else
                                                        curr_richiesta.RevocaStato = StatoRichiesta.Rifiutata;
                                                else
                                                    if (idxStato == 0)
                                                    curr_richiesta.RichiestaStato = StatoRichiesta.ApprovazioneInCorso;
                                                else
                                                    curr_richiesta.RevocaStato = StatoRichiesta.ApprovazioneInCorso;

                                                break;
                                        }
                                        if (idxStato == 0)
                                            curr_richiesta.RichiestaApprovazioneData = JsonConvert.SerializeObject(listaAppr, Formatting.Indented);
                                        else
                                            curr_richiesta.RevocaApprovazioneData = JsonConvert.SerializeObject(listaAppr, Formatting.Indented);

                                        await _dip_GG_RichiestaRepository.UpdateAsync(curr_richiesta);


                                        if (idxStato == 0 && RichiestaStato_Orig != curr_richiesta.RichiestaStato) // Aggiorno i dettagli solo se lo stato è cambiato  e solo per le richieste approvazioni
                                        {
                                            //posso cancellare i dettagli
                                            await Dip_GG_Richiesta_Canc_Dettaglio(curr_richiesta);

                                        }
                                    }
                                    break;
                            }
                        }

                    }
                }


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


        private async Task Dip_GG_Richiesta_Canc_Dettaglio(Dip_GG_Richiesta item)
        {
            switch (item.RichiestaTipo)
            {
                case TipoRichiesta.Timbratura:
                    var timbr = _dip_GG_TimbraturaRepository.FindAll(x => x.IdDip_GG_Richiesta == item.Id).ToList();
                    await _dip_GG_TimbraturaRepository.DeleteRangeAsync(timbr);

                    break;
                case TipoRichiesta.Giustificativo:

                    var just = _dip_GG_GiustificativiRepository.FindAll(x => x.IdDip_GG_Richiesta == item.Id).ToList();
                    await _dip_GG_GiustificativiRepository.DeleteRangeAsync(just);

                    break;
                case TipoRichiesta.NotaSpesa:

                    var nota = _dip_GG_NotaSpesaRepository.FindAll(x => x.IdDip_GG_Richiesta == item.Id).ToList();
                    await _dip_GG_NotaSpesaRepository.DeleteRangeAsync(nota);

                    break;
            }
        }


        private async Task Add_Dip_GG_Timbratura(Dip_GG_Richiesta dip_GG_Richiesta,
                                                 User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp)
        {
            if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
            {

                if (dip_GG_Richiesta.Dati != null)
                {
                    Dip_GG_Richiesta_Body_Timbratura? richiesta = JsonConvert.DeserializeObject<Dip_GG_Richiesta_Body_Timbratura>(dip_GG_Richiesta.Dati);
                    if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                    {
                        if (richiesta != null)
                        {
                            string fullDateTime = $"{dip_GG_Richiesta.Data.ToString("dd/MM/yyyy")} {richiesta.hhmm}";

                            // Fai il parsing della stringa completa
                            DateTime parsedDateTime = DateTime.ParseExact(fullDateTime, "dd/MM/yyyy HH:mm", System.Globalization.CultureInfo.InvariantCulture);

                            var dip_GG_Timbratura = new Dip_GG_Timbratura()
                            {
                                IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id,
                                IdDip_GG_Richiesta = dip_GG_Richiesta.Id,
                                Timbratura = parsedDateTime,
                                TimbraturaOriginale = parsedDateTime,
                                TimbraturaArrotondata = parsedDateTime,
                                GiornoCompetenza = new DateTime(parsedDateTime.Year, parsedDateTime.Month, parsedDateTime.Day),
                                TimbraturaTipo = TipoTimbratura.SenzaVerso,
                                RichiestaStato = StatoRichiesta.Immessa,
                            };
                            await _dip_GG_TimbraturaRepository.UpsertAsync(dip_GG_Timbratura);
                        }
                    }
                }
            }
        }
        private async Task Add_Dip_GG_Giustificativi(Dip_GG_Richiesta dip_GG_Richiesta,
                                                     User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp)
        {
            if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
            {
                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {

                    if (dip_GG_Richiesta.Dati != null)
                    {
                        Dip_GG_Richiesta_Body_Giustificativo? richiesta = JsonConvert.DeserializeObject<Dip_GG_Richiesta_Body_Giustificativo>(dip_GG_Richiesta.Dati);
                        if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                        {
                            if (richiesta != null)
                            {

                                for (DateTime date = dip_GG_Richiesta.Data; date <= dip_GG_Richiesta.DataA; date = date.AddDays(1))
                                {
                                    // string fullDateTime = $"{dip_GG_Richiesta.Data.ToString("dd/MM/yyyy")} {richiesta.hhmm}";

                                    var dip_GG_Giustificativi = new Dip_GG_Giustificativi()
                                    {
                                        Data = date,
                                        IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id,
                                        IdDip_GG_Richiesta = dip_GG_Richiesta.Id,
                                        IdPar_Giustificativi = richiesta.IdPar_Giustificativi,
                                        RichiestaStato = StatoRichiesta.Immessa,
                                        InputType = richiesta.AllDay ? JustificationInputType.AllDay : JustificationInputType.Manual,
                                        Hours = TimeSpan.Parse(richiesta.hhmm)
                                    };
                                    await _dip_GG_GiustificativiRepository.UpsertAsync(dip_GG_Giustificativi);
                                }
                            }
                        }
                    }
                }


            }
        }
        private async Task<List<Dip_GG_Richiesta>> Get_Dip_GG_Richiesta(List<int> idDipRappList, int Year, int Month)
        {

            var richiesta = new List<Dip_GG_Richiesta>();

            var reqAz_Cfg = new GenericRequest<Az_Cfg_Get_InModel>();

            var resAz_Cfg = await _az_CfgService.Az_CfgGet(reqAz_Cfg, true);

            if (resAz_Cfg.Success && resAz_Cfg.Data != null && resAz_Cfg.Data.Az_Cfg != null)
            {
                richiesta = _dip_GG_RichiestaRepository.FindAll(x => idDipRappList.Contains(x.IdDip_RapportoLavoro)
                                                                                  // &&
                                                                                  //(
                                                                                  //    (x.Data.Year == Year && x.Data.Month == Month) ||
                                                                                  //    (x.DataA.Year == Year && x.DataA.Month == Month) ||
                                                                                  //    (x.Data.Year < Year || (x.Data.Year == Year && x.Data.Month < Month)) && (x.DataA.Year > Year || (x.DataA.Year == Year && x.DataA.Month > Month))
                                                                                  //)
                                                                                  )
                                                                          .ToList();

                if (richiesta.Count > 0)
                {
                    if (resAz_Cfg.Data.Az_Cfg.ApprovazioneTipo == TipoApprovazione.AllAdminHierarchy || resAz_Cfg.Data.Az_Cfg.ApprovazioneTipo == TipoApprovazione.AllAdmin)
                    {
                        var richiesta_tmp = new List<Dip_GG_Richiesta>();

                        foreach (var item in richiesta)
                        {
                            for (var idxStato = 0; idxStato <= 1; idxStato++) // 0= richiesta, 1=revoca
                            {
                                StatoRichiesta? Stato = idxStato == 0 ? item.RichiestaStato : item.RevocaStato;
                                string? jsonReq = idxStato == 0 ? item.RichiestaApprovazioneData : item.RevocaApprovazioneData;

                                if (Stato != null)
                                {
                                    // è attiva la fase di revoca, quindi non esamino la fase di approvazione e continuo
                                    if (idxStato == 0 && Stato == StatoRichiesta.Approvata && item.RevocaStato != null)
                                        continue;

                                    List<Dip_GG_Richiesta_Stato_Cronology> lista = !string.IsNullOrEmpty(jsonReq)
                                            ? JsonConvert.DeserializeObject<List<Dip_GG_Richiesta_Stato_Cronology>>(jsonReq) ?? new List<Dip_GG_Richiesta_Stato_Cronology>()
                                            : new List<Dip_GG_Richiesta_Stato_Cronology>();

                                    int idxReq = lista.FindIndex(x => x.IdAspNetUsers == this.CurrentUserId);

                                    if (idxReq >= 0)
                                    {
                                        if (Stato == StatoRichiesta.Immessa || Stato == StatoRichiesta.ApprovazioneInCorso || Stato == StatoRichiesta.ParzialmenteApprovata)
                                        {
                                            Boolean showReq = false;

                                            switch (resAz_Cfg.Data.Az_Cfg.ApprovazioneTipo)
                                            {
                                                case TipoApprovazione.AllAdminHierarchy:
                                                    if (lista[idxReq - 1].RichiestaStato == StatoRichiesta.Approvata)
                                                    {
                                                        showReq = true;
                                                        //forzatura 
                                                        if (idxStato == 0)
                                                            item.RichiestaStato = (StatoRichiesta)lista[idxReq].RichiestaStato;
                                                        else
                                                            item.RevocaStato = (StatoRichiesta)lista[idxReq].RichiestaStato;
                                                    }
                                                    break;
                                                case TipoApprovazione.AllAdmin:
                                                case TipoApprovazione.SigleAdmin:
                                                    showReq = true;
                                                    if (lista[idxReq].RichiestaStato == StatoRichiesta.Immessa)
                                                    {
                                                        //forzatura 
                                                        if (idxStato == 0)
                                                            item.RichiestaStato = (StatoRichiesta)lista[idxReq].RichiestaStato;
                                                        else
                                                            item.RevocaStato = (StatoRichiesta)lista[idxReq].RichiestaStato;
                                                    }
                                                    break;
                                            }

                                            //////if (resAz_Cfg.Data.Az_Cfg.ApprovazioneTipo == TipoApprovazione.AllAdminHierarchy)
                                            //////{
                                            //////    if (lista[idxReq - 1].RichiestaStato == StatoRichiesta.Approvata)
                                            //////    {
                                            //////        showReq = true;
                                            //////        //forzatura 
                                            //////        item.RichiestaStato = (StatoRichiesta)lista[idxReq].RichiestaStato; //StatoRichiesta.Immessa;
                                            //////    }
                                            //////}
                                            //////else if (resAz_Cfg.Data.Az_Cfg.ApprovazioneTipo == TipoApprovazione.AllAdmin)
                                            //////{
                                            //////    showReq = true;
                                            //////    if (lista[idxReq].RichiestaStato == StatoRichiesta.Immessa)
                                            //////    {
                                            //////        //forzatura 
                                            //////        item.RichiestaStato = (StatoRichiesta)lista[idxReq].RichiestaStato; //StatoRichiesta.Immessa;
                                            //////    }
                                            //////}
                                            //////if (resAz_Cfg.Data.Az_Cfg.ApprovazioneTipo == TipoApprovazione.SigleAdmin)
                                            //////{
                                            //////    showReq = true;
                                            //////    if (lista[idxReq].RichiestaStato == StatoRichiesta.Immessa)
                                            //////    {
                                            //////        //forzatura 
                                            //////        item.RichiestaStato = (StatoRichiesta)lista[idxReq].RichiestaStato; //StatoRichiesta.Immessa;
                                            //////    }
                                            //////}

                                            if (showReq)
                                            {
                                                richiesta_tmp.Add(item);
                                                continue;
                                            }
                                        }
                                        else
                                        {   // Diretta,Cancellata,Rifiutata,Approvata la visualizzo
                                            richiesta_tmp.Add(item);
                                            continue;
                                        }
                                    }
                                    else
                                    {
                                        //passo da qui x gli user
                                        richiesta_tmp.Add(item);
                                    }

                                }
                            }


                        }

                        richiesta = richiesta_tmp;
                    }
                }
            }


        richiesta = richiesta.OrderByDescending(x => x.Data).ToList();

            return richiesta;
        }

        public virtual async Task<GenericResult<Dip_GG_Richiesta_Get_4Calculation_OutModel>> Dip_GG_Richiesta_Get_4Calculation(GenericRequest<Dip_GG_Richiesta_Get_4Calculation_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Richiesta_Get_4Calculation_OutModel retVal = new Dip_GG_Richiesta_Get_4Calculation_OutModel();

                List<int> idRapportoLavoroList = new List<int>();

                foreach (string userId in model.Data.UsersId)
                {
                    User_DATA_COMB_DipAna_DipRapp userData = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userId, false);
                    if (userData?.dip_RapportoLavoro != null)
                    {
                        idRapportoLavoroList.Add(userData.dip_RapportoLavoro.Id);
                    }
                }

                if (idRapportoLavoroList.Count > 0)
                {
                    var richieste = _dip_GG_RichiestaRepository
                        .FindAll(x => idRapportoLavoroList.Contains(x.IdDip_RapportoLavoro) &&
                                      x.Data >= model.Data.Dal &&
                                      x.Data <= model.Data.Al)
                        .OrderBy(x => x.IdDip_RapportoLavoro)
                        .ThenBy(x => x.Data)
                        .ToList();

                    retVal.Dip_GG_Richiesta = _mapper.Map<List<Dip_GG_RichiestaModel>>(richieste);
                }

                //eliminare
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


    }

    public interface IDip_GG_RichiestaService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_Richiesta_GetAll4User_OutModel>> GetAll4User(GenericRequest<Dip_GG_Richiesta_GetAll4User_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Richiesta_GetAll4Admin_OutModel>> GetAll4Admin(GenericRequest<Dip_GG_Richiesta_GetAll4Admin_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Richiesta_Send_OutModel>> Send(GenericRequest<Dip_GG_Richiesta_Send_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Richiesta_SetState_OutModel>> SetState(GenericRequest<Dip_GG_Richiesta_SetState_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Richiesta_Get_4Calculation_OutModel>> Dip_GG_Richiesta_Get_4Calculation(GenericRequest<Dip_GG_Richiesta_Get_4Calculation_InModel> model, Boolean isSubProcess);
    }
}
