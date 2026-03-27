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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService
{

    public class Dip_GG_GiustificativiService : ServiceBase, IDip_GG_GiustificativiService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_GG_GiustificativiRepository _Dip_GG_GiustificativiRepository;

        public Dip_GG_GiustificativiService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IDip_GG_GiustificativiRepository Dip_GG_GiustificativiRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _Dip_GG_GiustificativiRepository = Dip_GG_GiustificativiRepository;
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


                
                Dip_GG_Giustificativi? dip_GG_Giustificativi = await _Dip_GG_GiustificativiRepository.FindByIdAsync(model.Data.Dip_GG_Giustificativi.Id);
                if (dip_GG_Giustificativi == null)
                {
                    dip_GG_Giustificativi = _mapper.Map<Dip_GG_Giustificativi>(model.Data.Dip_GG_Giustificativi);
                    dip_GG_Giustificativi.IdDip_RapportoLavoro = model.Data.IdDip_RapportoLavoro;
                }
                else
                {
                    dip_GG_Giustificativi = _mapper.Map<Dip_GG_Giustificativi>(model.Data.Dip_GG_Giustificativi);
                }

                dip_GG_Giustificativi = await _Dip_GG_GiustificativiRepository.UpsertAsync(dip_GG_Giustificativi);
                retVal.Dip_GG_Giustificativi = _mapper.Map<Dip_GG_GiustificativiModel>(dip_GG_Giustificativi);

                

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IDip_GG_GiustificativiService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_Giustificativi_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Giustificativi_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Giustificativi_Get_4Calculation_OutModel>> Dip_GG_Giustificativi_Get_4Calculation(GenericRequest<Dip_GG_Giustificativi_Get_4Calculation_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_GiustificativiPutOutModel>> Dip_GG_GiustificativiPut(GenericRequest<Dip_GG_GiustificativiPutInModel> model, bool isSubProcess);    
    }
}
