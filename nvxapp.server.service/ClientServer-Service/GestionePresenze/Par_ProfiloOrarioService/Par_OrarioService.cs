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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;
namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrario
{

    public class Par_ProfiloOrarioService : ServiceBase, IPar_ProfiloOrarioService
    {
        private readonly IPar_ProfiloOrarioRepository _par_ProfiloOrarioRepository;

        public Par_ProfiloOrarioService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_ProfiloOrarioRepository par_ProfiloOrarioRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_ProfiloOrarioRepository = par_ProfiloOrarioRepository;
        }

        public virtual async Task<GenericResult<Par_ProfiloOrarioOutModel>> GetAll(GenericRequest<Par_ProfiloOrarioInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_ProfiloOrarioOutModel retVal = new Par_ProfiloOrarioOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IPar_ProfiloOrarioService : IServiceBase
    {
        public Task<GenericResult<Par_ProfiloOrarioOutModel>> GetAll( GenericRequest<Par_ProfiloOrarioInModel> model, Boolean isSubProcess);
    }
}
