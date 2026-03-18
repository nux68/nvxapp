using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.Base;
using nvxapp.server.service.Interfaces;
using Microsoft.AspNetCore.Identity;
using AutoMapper;
using Microsoft.Extensions.Options;
using nvxapp.server.service.ServerModels;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using Microsoft.AspNetCore.Http;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService
{

    public class Dip_GG_CausaliService : ServiceBase, IDip_GG_CausaliService
    {
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IDip_GG_CausaliRepository _Dip_GG_CausaliRepository;

        public Dip_GG_CausaliService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                  IDip_GG_CausaliRepository Dip_GG_CausaliRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _Dip_GG_CausaliRepository = Dip_GG_CausaliRepository;
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

    }

    public interface IDip_GG_CausaliService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_Causali_GetAll_OutModel>> GetAll( GenericRequest<Dip_GG_Causali_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Dip_GG_Causali_Get_4Calculation_OutModel>> Dip_GG_Causali_Get_4Calculation(GenericRequest<Dip_GG_Causali_Get_4Calculation_InModel> model, Boolean isSubProcess);
    }
}
