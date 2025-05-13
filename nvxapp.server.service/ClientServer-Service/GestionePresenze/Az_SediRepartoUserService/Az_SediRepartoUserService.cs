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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService
{

    public class Az_SediRepartoUserService : ServiceBase, IAz_SediRepartoUserService
    {
        private readonly IAz_SediRepartoUserRepository _az_RepartoUserRepository;

        public Az_SediRepartoUserService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_SediRepartoUserRepository az_RepartoUserRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_RepartoUserRepository = az_RepartoUserRepository;
        }

        public virtual async Task<GenericResult<Az_SediRepartoUser_GetAll_OutModel>> GetAll(GenericRequest<Az_SediRepartoUser_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediRepartoUser_GetAll_OutModel retVal = new Az_SediRepartoUser_GetAll_OutModel();

                var RepUser =  _az_RepartoUserRepository.FindAll(x=> x.IdAz_SediReparto == model.Data.IdAz_SediReparto).ToList();
                retVal.Az_RepartoUser = _mapper.Map<List<Az_SediRepartoUserModel>>(RepUser);
                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

        public virtual async Task<GenericResult<Az_SediRepartoUser_GetAll_OutModel>> GetAllPeriod(GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediRepartoUser_GetAll_OutModel retVal = new Az_SediRepartoUser_GetAll_OutModel();

                var RepUser = _az_RepartoUserRepository.FindAll(x => x.IdAz_SediReparto == model.Data.IdAz_SediReparto).ToList();
                retVal.Az_RepartoUser = _mapper.Map<List<Az_SediRepartoUserModel>>(RepUser);
                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_SediRepartoUserService : IServiceBase
    {
        public Task<GenericResult<Az_SediRepartoUser_GetAll_OutModel>> GetAll( GenericRequest<Az_SediRepartoUser_GetAll_InModel> model, Boolean isSubProcess);
        public Task<GenericResult<Az_SediRepartoUser_GetAll_OutModel>> GetAllPeriod(GenericRequest<Az_SediRepartoUser_GetAll_Period_InModel> model, Boolean isSubProcess);
    }
}
