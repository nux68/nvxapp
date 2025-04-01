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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_Orario
{

    public class Par_OrarioService : ServiceBase, IPar_OrarioService
    {
        private readonly IPar_OrarioRepository _par_OrarioRepository;

        public Par_OrarioService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_OrarioRepository par_OrarioRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_OrarioRepository = par_OrarioRepository;
        }

        public virtual async Task<GenericResult<Par_OrarioOutModel>> GetAll(GenericRequest<Par_OrarioInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_OrarioOutModel retVal = new Par_OrarioOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IPar_OrarioService : IServiceBase
    {
        public Task<GenericResult<Par_OrarioOutModel>> GetAll( GenericRequest<Par_OrarioInModel> model, Boolean isSubProcess);
    }
}
