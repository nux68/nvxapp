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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGGService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrario;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;
namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioGG
{

    public class Par_ProfiloOrarioGGService : ServiceBase, IPar_ProfiloOrarioGGService
    {
        private readonly IPar_ProfiloOrarioGGRepository _par_ProfiloOrarioGGRepository;

        public Par_ProfiloOrarioGGService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_ProfiloOrarioGGRepository par_ProfiloOrarioGGRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_ProfiloOrarioGGRepository = par_ProfiloOrarioGGRepository;
        }

        public virtual async Task<GenericResult<Par_ProfiloOrarioGGOutModel>> GetAll(GenericRequest<Par_ProfiloOrarioGGInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_ProfiloOrarioGGOutModel retVal = new Par_ProfiloOrarioGGOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IPar_ProfiloOrarioGGService : IServiceBase
    {
        public Task<GenericResult<Par_ProfiloOrarioGGOutModel>> GetAll( GenericRequest<Par_ProfiloOrarioGGInModel> model, Boolean isSubProcess);
    }
}
