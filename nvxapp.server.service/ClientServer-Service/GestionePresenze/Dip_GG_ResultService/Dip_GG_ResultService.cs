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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService
{

    public class Dip_GG_ResultService : ServiceBase, IDip_GG_ResultService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_GG_ResultRepository _dip_GG_ResultRepository;

        public Dip_GG_ResultService(IMapper mapper,
                                          UserManager<ApplicationUser> userManager,
                                          IAspNetUsersRepository aspNetUsersRepository,
                                          IOptions<JwtParameter> jwtParameter,
                                          IHttpContextAccessor httpContextAccessor,
                                          IConfiguration configuration,

                                          IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                          IDip_GG_ResultRepository dip_GG_ResultRepository
                                  ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _dip_GG_ResultRepository = dip_GG_ResultRepository;

        }

        public virtual async Task<GenericResult<Dip_GG_Result_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Result_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Result_GetAll_OutModel retVal = new Dip_GG_Result_GetAll_OutModel();

                string userId = string.IsNullOrEmpty(model.Data.IdAspNetUsers) ? this.CurrentUserId : model.Data.IdAspNetUsers;

                User_DATA_COMB_DipAna_DipRapp user_DATA_COMB_DipAna_DipRapp = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(userId, true);

                if (user_DATA_COMB_DipAna_DipRapp != null && user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro != null)
                {

                    List<Dip_GG_Result> timbratura = _dip_GG_ResultRepository.FindAll(x => x.Data.Year == model.Data.Year &&
                                                                                              x.Data.Month == model.Data.Month &&
                                                                                              x.IdDip_RapportoLavoro == user_DATA_COMB_DipAna_DipRapp.dip_RapportoLavoro.Id)
                                                                                     //.OrderBy(x => x.TimbraturaOriginale)
                                                                                     .ToList();


                    retVal.Dip_GG_Result = _mapper.Map<List<Dip_GG_ResultModel>>(timbratura);

                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Dip_GG_Result_Get_4Calculation_OutModel>> Dip_GG_Result_Get_4Calculation(GenericRequest<Dip_GG_Result_Get_4Calculation_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Result_Get_4Calculation_OutModel retVal = new Dip_GG_Result_Get_4Calculation_OutModel();

                // Recupera i IdDip_RapportoLavoro per tutti gli utenti richiesti
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
                    List<Dip_GG_Result> timbrature = _dip_GG_ResultRepository
                        .FindAll(x => idRapportoLavoroList.Contains(x.IdDip_RapportoLavoro) &&
                                      x.Data >= model.Data.Dal &&
                                      x.Data <= model.Data.Al)
                        .OrderBy(x => x.IdDip_RapportoLavoro)
                        .ToList();

                    retVal.Dip_GG_Result = _mapper.Map<List<Dip_GG_ResultModel>>(timbrature);

                    #region "controllo init"
                    TimeSpan diff = model.Data.Al - model.Data.Dal;
                    foreach (var item in idRapportoLavoroList)
                    {
                        if (retVal.Dip_GG_Result.Select(x => x.IdDip_RapportoLavoro == item).Count() != diff.Days + 1)
                        {
                            List<DateTime> DayToAdd = new List<DateTime>();
                            for (var giorno = model.Data.Dal; giorno <= model.Data.Al; giorno = giorno.AddDays(1))
                            {
                                var currDay = retVal.Dip_GG_Result.Where(x => x.Data.Date == giorno).FirstOrDefault();
                                if (currDay == null)
                                    DayToAdd.Add(giorno);
                            }

                            var req_Init = new GenericRequest<Dip_GG_Result_Init_InModel>();
                            req_Init.Data.IdDip_RapportoLavoro = item;
                            req_Init.Data.Date = DayToAdd;
                            var res_Init = await Dip_GG_Result_Init(req_Init, true);
                            if (res_Init.Success && res_Init.Data != null)
                            {
                                retVal.Dip_GG_Result.AddRange(_mapper.Map<List<Dip_GG_ResultModel>>(res_Init.Data.Dip_GG_Result));
                            }

                        }

                        var prevDay = retVal.Dip_GG_Result.Where(x => x.Data < DateTime.Now.AddDays(-1) &&
                                                                     (x.Stato & GG_ResultStato.STATE_MASK) == GG_ResultStato.Init).ToList();
                        
                        prevDay.ForEach(x => x.Stato |= GG_ResultStato.Warning);

                    }
                    #endregion

                }

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Dip_GG_Result_Init_OutModel>> Dip_GG_Result_Init(GenericRequest<Dip_GG_Result_Init_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_Result_Init_OutModel retVal = new Dip_GG_Result_Init_OutModel();
                retVal.Dip_GG_Result = new List<Dip_GG_Result>();

                foreach (var item in model.Data.Date)
                {
                    var gg_res = _dip_GG_ResultRepository.FindAll(x => x.Data == item &&
                                                                       x.IdDip_RapportoLavoro == model.Data.IdDip_RapportoLavoro)
                                                         .FirstOrDefault();
                    if (gg_res == null)
                    {
                        gg_res = new Dip_GG_Result
                        {
                            IdDip_RapportoLavoro = model.Data.IdDip_RapportoLavoro,
                            Data = item,
                            HH_Teo = TimeOnly.FromTimeSpan(TimeSpan.Zero),
                            HH_Lav = TimeOnly.FromTimeSpan(TimeSpan.Zero),
                            Stato =  GG_ResultStato.Init
                        };
                        var newDay = await _dip_GG_ResultRepository.UpdateAsync(gg_res);
                        retVal.Dip_GG_Result.Add(newDay);
                    }
                }


                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }


        public virtual async Task<GenericResult<Dip_GG_ResultPutOutModel>> Dip_GG_ResultPut(GenericRequest<Dip_GG_ResultPutInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_ResultPutOutModel retVal = new Dip_GG_ResultPutOutModel();
                retVal.Dip_GG_Result = model.Data.Dip_GG_Result;

                int idCompany;
                int.TryParse(this.CurrentCompany, out idCompany);


                
                Dip_GG_Result? dip_GG_Result = await _dip_GG_ResultRepository.FindByIdAsync(model.Data.Dip_GG_Result.Id);
                if (dip_GG_Result == null)
                {
                    dip_GG_Result = _mapper.Map<Dip_GG_Result>(model.Data.Dip_GG_Result);
                    dip_GG_Result.IdDip_RapportoLavoro = model.Data.IdDip_RapportoLavoro;
                }
                else
                {
                    //update 
                    dip_GG_Result = _mapper.Map<Dip_GG_Result>(model.Data.Dip_GG_Result);
                    
                }

                dip_GG_Result = await _dip_GG_ResultRepository.UpsertAsync(dip_GG_Result);
                retVal.Dip_GG_Result = _mapper.Map<Dip_GG_ResultModel>(dip_GG_Result);

                

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IDip_GG_ResultService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_Result_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Result_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Result_Get_4Calculation_OutModel>> Dip_GG_Result_Get_4Calculation(GenericRequest<Dip_GG_Result_Get_4Calculation_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Result_Init_OutModel>> Dip_GG_Result_Init(GenericRequest<Dip_GG_Result_Init_InModel> model, Boolean isSubProcess);

        public Task<GenericResult<Dip_GG_ResultPutOutModel>> Dip_GG_ResultPut(GenericRequest<Dip_GG_ResultPutInModel> model, bool isSubProcess);

    }


}
