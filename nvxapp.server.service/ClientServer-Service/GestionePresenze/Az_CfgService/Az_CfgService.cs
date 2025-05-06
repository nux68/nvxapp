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

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService
{

    public class Az_CfgService : ServiceBase, IAz_CfgService
    {
        private readonly IAz_CfgRepository _az_CfgRepository;

        public Az_CfgService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_CfgRepository az_CfgRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_CfgRepository = az_CfgRepository;
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

    }

    public interface IAz_CfgService : IServiceBase
    {
        public Task<GenericResult<Az_Cfg_GetAll_OutModel>> GetAll( GenericRequest<Az_Cfg_GetAll_InModel> model, Boolean isSubProcess);
    }
}
