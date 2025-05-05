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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_RepartoUserService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_RepartoUserService
{

    public class Az_RepartoUserService : ServiceBase, IAz_RepartoUserService
    {
        private readonly IAz_RepartoUserRepository _az_RepartoUserRepository;

        public Az_RepartoUserService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_RepartoUserRepository az_RepartoUserRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_RepartoUserRepository = az_RepartoUserRepository;
        }

        public virtual async Task<GenericResult<Az_RepartoUser_GetAll_OutModel>> GetAll(GenericRequest<Az_RepartoUser_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_RepartoUser_GetAll_OutModel retVal = new Az_RepartoUser_GetAll_OutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_RepartoUserService : IServiceBase
    {
        public Task<GenericResult<Az_RepartoUser_GetAll_OutModel>> GetAll( GenericRequest<Az_RepartoUser_GetAll_InModel> model, Boolean isSubProcess);
    }
}
