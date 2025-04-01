using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Infrastructure;
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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService
{

    public class Par_CausaliService : ServiceBase, IPar_CausaliService
    {
        private readonly IPar_CausaliRepository _par_CausaliRepository;

        public Par_CausaliService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_CausaliRepository par_CausaliRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_CausaliRepository = par_CausaliRepository;
        }

        public virtual async Task<GenericResult<Par_CausaliOutModel>> GetAll(GenericRequest<Par_CausaliInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_CausaliOutModel retVal = new Par_CausaliOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IPar_CausaliService : IServiceBase
    {
        public Task<GenericResult<Par_CausaliOutModel>> GetAll( GenericRequest<Par_CausaliInModel> model, Boolean isSubProcess);
    }
}
