using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService
{

    public class Dip_GG_GiustificativiService : ServiceBase, IDip_GG_GiustificativiService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_GG_GiustificativiRepository _Dip_GG_GiustificativiRepository;
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;
        private ITimeSheet_EngineService_OnlyCalculate _timeSheet_EngineService;
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;
        private readonly IAccountService _accountService;
        private readonly IGiustificativiNotifier _giustificativiNotifier;

        public Dip_GG_GiustificativiService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IPar_GiustificativiRepository par_GiustificativiRepository,
                                  ITimeSheet_EngineService_OnlyCalculate timeSheet_EngineService,

                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IDip_GG_GiustificativiRepository Dip_GG_GiustificativiRepository,
                                  IDip_AnagraficaRepository dip_AnagraficaRepository,
                                  IDip_RapportoLavoroRepository dip_RapportoLavoroRepository,
                                  IAccountService accountService,
                                  IGiustificativiNotifier giustificativiNotifier) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _Dip_GG_GiustificativiRepository = Dip_GG_GiustificativiRepository;
            _par_GiustificativiRepository = par_GiustificativiRepository;
            _timeSheet_EngineService = timeSheet_EngineService;
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
            _accountService = accountService;
            _giustificativiNotifier = giustificativiNotifier;
        }

        public virtual async Task<GenericResult<Dip_GG_Giustificativi_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Giustificativi_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Giustificativi_GetAll_OutModel retVal = new Dip_GG_Giustificativi_GetAll_OutModel();

                string userId = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? this.CurrentUserId : model.Data.IdAspNetUsers;

                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userId, true);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {

                    List<Dip_GG_Giustificativi> just = _Dip_GG_GiustificativiRepository.FindAll(x => x.Data.Year == model.Data.Year &&
                                                                                              x.Data.Month == model.Data.Month &&
                                                                                              x.IdDip_RapportoLavoro == user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id)
                                                                                          //.OrderBy(x => x.TimbraturaOriginale)
                                                                                          .ToList();



                    retVal.Dip_GG_Giustificativi = _mapper.Map<List<Dip_GG_GiustificativiModel>>(just);

                }




                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_Giustificativi_Get_4Calculation_OutModel>> Dip_GG_Giustificativi_Get_4Calculation(GenericRequest<Dip_GG_Giustificativi_Get_4Calculation_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Giustificativi_Get_4Calculation_OutModel retVal = new Dip_GG_Giustificativi_Get_4Calculation_OutModel();

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
                    var giustificativi = _Dip_GG_GiustificativiRepository
                        .FindAll(x => idRapportoLavoroList.Contains(x.IdDip_RapportoLavoro) &&
                                      x.Data >= model.Data.Dal &&
                                      x.Data <= model.Data.Al)
                        .OrderBy(x => x.IdDip_RapportoLavoro)
                        .ThenBy(x => x.Data)
                        .ToList();

                    retVal.Dip_GG_Giustificativi = _mapper.Map<List<Dip_GG_GiustificativiModel>>(giustificativi);
                }

                //eliminare
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_GiustificativiPutOutModel>> Dip_GG_GiustificativiPut(GenericRequest<Dip_GG_GiustificativiPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_GiustificativiPutOutModel retVal = new Dip_GG_GiustificativiPutOutModel();
                retVal.Dip_GG_Giustificativi = model.Data.Dip_GG_Giustificativi;

                int idCompany;
                int.TryParse(this.CurrentCompany, out idCompany);



                // giustificativo a importo: un solo importo, niente ore; orario: niente importo
                var giust = model.Data.Dip_GG_Giustificativi;
                var tipo = _par_GiustificativiRepository.FindAll(x => x.Id == giust.IdPar_Giustificativi).Select(x => x.Tipo).FirstOrDefault();
                if (tipo == TipoValore.Importo)
                {
                    if (giust.Importo is not decimal importo || importo <= 0 || importo >= 100_000_000m)
                    {
                        retVal.Messages.Add(new Message(AttendanceTrackingMessages.Get("GiustificativoImportoObbligatorio"), MessageType.Exception));
                        return retVal;
                    }
                    giust.Importo = Math.Round(importo, 2);
                    giust.Hours = null;
                    giust.From = null;
                    giust.InputType = JustificationInputType.Manual;
                }
                else
                    giust.Importo = null;

                Dip_GG_Giustificativi? dip_GG_Giustificativi = await _Dip_GG_GiustificativiRepository.FindByIdAsync(model.Data.Dip_GG_Giustificativi.Id);
                if (dip_GG_Giustificativi == null)
                {
                    dip_GG_Giustificativi = _mapper.Map<Dip_GG_Giustificativi>(model.Data.Dip_GG_Giustificativi);
                    if (model.Data.IdDip_RapportoLavoro > 0)
                        dip_GG_Giustificativi.IdDip_RapportoLavoro = model.Data.IdDip_RapportoLavoro;
                }
                else
                {
                    dip_GG_Giustificativi = _mapper.Map<Dip_GG_Giustificativi>(model.Data.Dip_GG_Giustificativi);
                }

                // dal client non si movimentano i giorni fuori dal rapporto di lavoro (le chiamate interne del motore sono gia' nel rapporto)
                if (!isSubProcess)
                {
                    var fuoriRapporto = _gestionePresenzeUserUtility.VerificaGiorniNelRapporto(dip_GG_Giustificativi.IdDip_RapportoLavoro, dip_GG_Giustificativi.Data, dip_GG_Giustificativi.Data);
                    if (fuoriRapporto != null)
                    {
                        retVal.Messages.Add(new Message(fuoriRapporto, MessageType.Exception));
                        return retVal;
                    }
                }

                dip_GG_Giustificativi = await _Dip_GG_GiustificativiRepository.UpsertAsync(dip_GG_Giustificativi);
                retVal.Dip_GG_Giustificativi = _mapper.Map<Dip_GG_GiustificativiModel>(dip_GG_Giustificativi);

                if(!model.Data.ExcludeRicalc)
                    await CalculateGiorno(dip_GG_Giustificativi.IdDip_RapportoLavoro, dip_GG_Giustificativi.Data);

                // il motore del cartellino e le richieste (subprocess) salvano giorno per giorno: si avvisa solo per le azioni dal client
                if (!isSubProcess)
                    await NotifyGiustificativi(dip_GG_Giustificativi.IdDip_RapportoLavoro, dip_GG_Giustificativi.Data);

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Dip_GG_Giustificativi_DeleteOutModel>> Dip_GG_GiustificativiDelete(GenericRequest<Dip_GG_Giustificativi_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                var entity = await _Dip_GG_GiustificativiRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    var IdDip_RapportoLavoro = entity.IdDip_RapportoLavoro;
                    var GiornoCompetenza = entity.Data;

                    await _Dip_GG_GiustificativiRepository.DeleteAsync(entity);

                    if(!model.Data.ExcludeRicalc)
                        await CalculateGiorno(IdDip_RapportoLavoro, GiornoCompetenza);

                    if (!isSubProcess)
                        await NotifyGiustificativi(IdDip_RapportoLavoro, GiornoCompetenza);
                }

                

                return new Dip_GG_Giustificativi_DeleteOutModel();
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_GiustificativiGetOutModel>> Dip_GG_GiustificativiGet(GenericRequest<Dip_GG_GiustificativiGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_GiustificativiGetOutModel retVal = new Dip_GG_GiustificativiGetOutModel();



                int IdCompany, IdAnagrafica = 0;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                    IdAnagrafica = company_DATA.az_Anagrafica.Id;

                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipRapp_DipAna(model.Data.IdDip_RapportoLavoro);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {
                    Dip_GG_Giustificativi? dip_GG_Giustificativi = await _Dip_GG_GiustificativiRepository.FindByIdAsync(model.Data.Id);
                    int IdPar_Giustificativi = 0;
                    // proposta per un nuovo giustificativo: il primo a ore (con 1 ora)
                    var cau = _par_GiustificativiRepository.FindAll(x => x.IdAz_Anagrafica == IdAnagrafica && x.Tipo == TipoValore.Orario).FirstOrDefault()
                              ?? _par_GiustificativiRepository.FindAll(x => x.IdAz_Anagrafica == IdAnagrafica).FirstOrDefault();
                    if (cau != null)
                        IdPar_Giustificativi = cau.Id;

                    if (dip_GG_Giustificativi == null)
                        dip_GG_Giustificativi = new Dip_GG_Giustificativi()
                        {
                            Id = 0,
                            IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id,
                            Data = model.Data.Data != null ? model.Data.Data.Value : DateTime.Now,
                            IdPar_Giustificativi = IdPar_Giustificativi,
                            Hours = new TimeSpan(1, 0, 0)
                        };


                    retVal.Dip_GG_Giustificativi = _mapper.Map<Dip_GG_GiustificativiModel>(dip_GG_Giustificativi);
                }

                return retVal;
            }, isSubProcess);
        }


        // avvisa (SignalR) chi vede i giustificativi del dipendente: il dipendente e gli amministratori dell'azienda
        private async Task NotifyGiustificativi(int idDip_RapportoLavoro, DateTime giorno)
        {
            try
            {
                var recipients = new List<string>();

                var idDip_Anagrafica = _dip_RapportoLavoroRepository.FindAll(x => x.Id == idDip_RapportoLavoro).Select(x => x.IdDip_Anagrafica).ToList();
                recipients.AddRange(_dip_AnagraficaRepository.FindAll(x => idDip_Anagrafica.Contains(x.Id)).Select(x => x.IdAspNetUsers).ToList());

                var reqAdmin = new GenericRequest<UserCompanyListInModel>();
                reqAdmin.Data.FilteredRoles = new List<string> { "CompanyAdmin", "CompanyPowerAdmin" };
                var resAdmin = await _accountService.UserCompanyList(reqAdmin, true);
                if (resAdmin.Success && resAdmin.Data != null)
                    recipients.AddRange(resAdmin.Data.UserCompanyList.Select(x => x.IdAspNetUsers ?? string.Empty));

                await _giustificativiNotifier.GiustificativiNotifierAsync(recipients, new GiustificativiNotifierData
                {
                    IdDip_RapportoLavoro = idDip_RapportoLavoro,
                    Data = giorno.ToString("yyyy-MM-dd"),
                    Year = giorno.Year,
                    Month = giorno.Month
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Giustificativi del rapporto {IdDip_RapportoLavoro} giorno {Giorno}: notifica non inviata", idDip_RapportoLavoro, giorno);
            }
        }

        private async Task CalculateGiorno(int IdDip_RapportoLavoro, DateTime GiornoCompetenza)
        {
            User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipRapp_DipAna(IdDip_RapportoLavoro);

            if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null && user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica != null)
            {
                var TimeSheet_Calculate = new GenericRequest<TimeSheet_CalculateInModel>();
                TimeSheet_Calculate.Data = new TimeSheet_CalculateInModel()
                {
                    TimeSheet_Calculate = new TimeSheet_CalculateModel()
                    {
                        SelectedUserId = new List<string>() { user_DATA_COMB_DipAna_DipRapp.dip_Anagrafica.IdAspNetUsers },
                        Dal = GiornoCompetenza,
                        Al = GiornoCompetenza,
                        Month = GiornoCompetenza.Month,
                        Year = GiornoCompetenza.Year,
                    }
                };

                await _timeSheet_EngineService.Calculate(TimeSheet_Calculate, true);
            }
        }

    }

    public interface IDip_GG_GiustificativiService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_Giustificativi_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Giustificativi_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Giustificativi_Get_4Calculation_OutModel>> Dip_GG_Giustificativi_Get_4Calculation(GenericRequest<Dip_GG_Giustificativi_Get_4Calculation_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_GiustificativiPutOutModel>> Dip_GG_GiustificativiPut(GenericRequest<Dip_GG_GiustificativiPutInModel> model, bool isSubProcess);
        public Task<GenericResult<Dip_GG_Giustificativi_DeleteOutModel>> Dip_GG_GiustificativiDelete(GenericRequest<Dip_GG_Giustificativi_DeleteInModel> model, bool isSubProcess);
        public Task<GenericResult<Dip_GG_GiustificativiGetOutModel>> Dip_GG_GiustificativiGet(GenericRequest<Dip_GG_GiustificativiGetInModel> model, bool isSubProcess);
    }
}
