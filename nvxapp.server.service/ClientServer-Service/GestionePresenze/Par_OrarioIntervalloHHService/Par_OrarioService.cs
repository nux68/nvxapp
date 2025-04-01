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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHH
{

    public class Par_OrarioIntervalloHHService : ServiceBase, IPar_OrarioIntervalloHHService
    {
        private readonly IPar_OrarioIntervalloHHRepository _par_OrarioIntervalloHHRepository;

        public Par_OrarioIntervalloHHService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_OrarioIntervalloHHRepository par_OrarioIntervalloHHRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_OrarioIntervalloHHRepository = par_OrarioIntervalloHHRepository;
        }

        public virtual async Task<GenericResult<Par_OrarioIntervalloHHOutModel>> GetAll(GenericRequest<Par_OrarioIntervalloHHInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_OrarioIntervalloHHOutModel retVal = new Par_OrarioIntervalloHHOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IPar_OrarioIntervalloHHService : IServiceBase
    {
        public Task<GenericResult<Par_OrarioIntervalloHHOutModel>> GetAll( GenericRequest<Par_OrarioIntervalloHHInModel> model, Boolean isSubProcess);
    }
}
