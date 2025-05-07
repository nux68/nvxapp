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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.data.Entities.Tenant;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService
{

    public class Az_CfgService : ServiceBase, IAz_CfgService
    {
        private readonly IAz_CfgRepository _az_CfgRepository;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;

        public Az_CfgService(IMapper mapper,
                             UserManager<ApplicationUser> userManager,
                             IAspNetUsersRepository aspNetUsersRepository,
                             IOptions<JwtParameter> jwtParameter,
                             IHttpContextAccessor httpContextAccessor,
                             IConfiguration configuration,
                             IGestionePresenzeUserUtility gestionePresenzeUserUtility,

                             IAz_CfgRepository az_CfgRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_CfgRepository = az_CfgRepository;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
        }

        public virtual async Task<GenericResult<Az_Cfg_GetAll_OutModel>> GetAll(GenericRequest<Az_Cfg_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_Cfg_GetAll_OutModel retVal = new Az_Cfg_GetAll_OutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_Cfg_Get_OutModel>> Az_CfgGet(GenericRequest<Az_Cfg_Get_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_Cfg_Get_OutModel retVal = new Az_Cfg_Get_OutModel();

                int IdCompany;
                int.TryParse(this.CurrentCompany, out IdCompany);

                Company_DATA_COMB_AzAna_AzSedi_AzReparto_Az_Cfg company_DATA_COMB_AzAna_AzSedi_AzReparto = await _gestionePresenzeUserUtility.Get_AzAna_AzSedi_AzReparto_Az_Cfg(IdCompany, true);
                if (company_DATA_COMB_AzAna_AzSedi_AzReparto != null && company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Cfg != null)
                {
                    retVal.Az_Cfg =  _mapper.Map<Az_CfgModel>( company_DATA_COMB_AzAna_AzSedi_AzReparto.az_Cfg);
                }    
                    

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
        public virtual async Task<GenericResult<Az_Cfg_Put_OutModel>> Az_CfgPut(GenericRequest<Az_Cfg_Put_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_Cfg_Put_OutModel retVal = new Az_Cfg_Put_OutModel();


                var azCfg =  _mapper.Map<Az_Cfg>(model.Data.Az_Cfg);

                await _az_CfgRepository.UpsertAsync(azCfg);

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_CfgService : IServiceBase
    {
        public Task<GenericResult<Az_Cfg_GetAll_OutModel>> GetAll( GenericRequest<Az_Cfg_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_Cfg_Get_OutModel>> Az_CfgGet(GenericRequest<Az_Cfg_Get_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_Cfg_Put_OutModel>> Az_CfgPut(GenericRequest<Az_Cfg_Put_InModel> model, Boolean isSubProcess);
    }
}
