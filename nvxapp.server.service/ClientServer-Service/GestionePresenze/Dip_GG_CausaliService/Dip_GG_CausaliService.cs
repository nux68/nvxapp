using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService
{

    public class Dip_GG_CausaliService : ServiceBase, IDip_GG_CausaliService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_GG_CausaliRepository _Dip_GG_CausaliRepository;
        private readonly IPar_CausaliRepository _par_CausaliRepository;
        private readonly ITimeSheet_EngineService_OnlyCalculate _timeSheet_EngineService;

        public Dip_GG_CausaliService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IPar_CausaliRepository par_CausaliRepository,
                                  ITimeSheet_EngineService_OnlyCalculate timeSheet_EngineService,

                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IDip_GG_CausaliRepository Dip_GG_CausaliRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _Dip_GG_CausaliRepository = Dip_GG_CausaliRepository;
            _par_CausaliRepository = par_CausaliRepository;
            _timeSheet_EngineService = timeSheet_EngineService;
        }

        public virtual async Task<GenericResult<Dip_GG_Causali_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Causali_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Causali_GetAll_OutModel retVal = new Dip_GG_Causali_GetAll_OutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Dip_GG_Causali_Get_4Calculation_OutModel>> Dip_GG_Causali_Get_4Calculation(GenericRequest<Dip_GG_Causali_Get_4Calculation_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Causali_Get_4Calculation_OutModel retVal = new Dip_GG_Causali_Get_4Calculation_OutModel();

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
                    var causali = _Dip_GG_CausaliRepository
                        .FindAll(x => idRapportoLavoroList.Contains(x.IdDip_RapportoLavoro) &&
                                      x.Data >= model.Data.Dal &&
                                      x.Data <= model.Data.Al)
                        .OrderBy(x => x.IdDip_RapportoLavoro)
                        .ThenBy(x => x.Data)
                        .ToList();

                    retVal.Dip_GG_Causali = _mapper.Map<List<Dip_GG_CausaliModel>>(causali);
                }

                //eliminare
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Dip_GG_Causali_DeleteOutModel>> Dip_GG_CausaliDelete(GenericRequest<Dip_GG_Causali_DeleteInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Causali_DeleteOutModel retVal = new Dip_GG_Causali_DeleteOutModel();

                var entity = await _Dip_GG_CausaliRepository.FindByIdAsync(model.Data.Id);
                if (entity != null)
                {
                    var IdDip_RapportoLavoro = entity.IdDip_RapportoLavoro;
                    var GiornoCompetenza = entity.Data;

                    await _Dip_GG_CausaliRepository.DeleteAsync(entity);

                    if(!model.Data.ExcludeRicalc)
                        await CalculateGiorno(IdDip_RapportoLavoro, GiornoCompetenza);
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Dip_GG_CausaliPutOutModel>> Dip_GG_CausaliPut(GenericRequest<Dip_GG_CausaliPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_CausaliPutOutModel retVal = new Dip_GG_CausaliPutOutModel();


                retVal.Dip_GG_Causali = model.Data.Dip_GG_Causali;

                int idCompany;
                int.TryParse(this.CurrentCompany, out idCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(idCompany, true);

                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Anagrafica != null)
                {
                    Dip_GG_Causali? par_Causale = await _Dip_GG_CausaliRepository.FindByIdAsync(model.Data.Dip_GG_Causali.Id);
                    if (par_Causale == null)
                    {
                        par_Causale = new Dip_GG_Causali() { IdDip_RapportoLavoro = 0 };
                        par_Causale = _mapper.Map<Dip_GG_Causali>(model.Data.Dip_GG_Causali);

                        if (model.Data.IdDip_RapportoLavoro > 0)
                            par_Causale.IdDip_RapportoLavoro = model.Data.IdDip_RapportoLavoro;

                    }
                    else
                    {
                        //update 
                        par_Causale = _mapper.Map<Dip_GG_Causali>(model.Data.Dip_GG_Causali);
                    }

                    //aggiurna il valore ritornato al client
                    par_Causale = await _Dip_GG_CausaliRepository.UpsertAsync(par_Causale);
                    retVal.Dip_GG_Causali = _mapper.Map<Dip_GG_CausaliModel>(par_Causale);

                    if(!model.Data.ExcludeRicalc)
                        await CalculateGiorno(retVal.Dip_GG_Causali.IdDip_RapportoLavoro, retVal.Dip_GG_Causali.Data);
                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Dip_GG_CausaliGetOutModel>> Dip_GG_CausaliGet(GenericRequest<Dip_GG_CausaliGetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_CausaliGetOutModel retVal = new Dip_GG_CausaliGetOutModel();

                int IdCompany, IdAnagrafica = 0;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA != null && company_DATA.az_Anagrafica != null)
                    IdAnagrafica = company_DATA.az_Anagrafica.Id;

                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipRapp_DipAna(model.Data.IdDip_RapportoLavoro);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {
                    Dip_GG_Causali? dip_GG_Causali = await _Dip_GG_CausaliRepository.FindByIdAsync(model.Data.Id);
                    int IdPar_Causali = 0;
                    var cau = _par_CausaliRepository.FindAll(x => x.IdAz_Anagrafica == IdAnagrafica).FirstOrDefault();
                    if (cau != null)
                        IdPar_Causali = cau.Id;

                    if (dip_GG_Causali == null)
                        dip_GG_Causali = new Dip_GG_Causali()
                        {
                            Id = 0,
                            IdDip_RapportoLavoro = user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id,
                            Data = model.Data.Data != null ? model.Data.Data.Value : DateTime.Now,
                            Valore = new TimeOnly(1, 0, 0),
                            IdPar_Causali = IdPar_Causali
                        };


                    retVal.Dip_GG_Causali = _mapper.Map<Dip_GG_CausaliModel>(dip_GG_Causali);
                }

                return retVal;
            }, isSubProcess);
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

    public interface IDip_GG_CausaliService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_Causali_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Causali_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Causali_Get_4Calculation_OutModel>> Dip_GG_Causali_Get_4Calculation(GenericRequest<Dip_GG_Causali_Get_4Calculation_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Causali_DeleteOutModel>> Dip_GG_CausaliDelete(GenericRequest<Dip_GG_Causali_DeleteInModel> model, bool isSubProcess);
        public Task<GenericResult<Dip_GG_CausaliPutOutModel>> Dip_GG_CausaliPut(GenericRequest<Dip_GG_CausaliPutInModel> model, bool isSubProcess);
        public Task<GenericResult<Dip_GG_CausaliGetOutModel>> Dip_GG_CausaliGet(GenericRequest<Dip_GG_CausaliGetInModel> model, bool isSubProcess);
    }


}
