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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrarioService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_ProfiloOrario
{

    public class Dip_ProfiloOrarioService : ServiceBase, IDip_ProfiloOrarioService
    {
        private readonly IDip_ProfiloOrarioRepository _dip_ProfiloOrarioRepository;

        public Dip_ProfiloOrarioService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IDip_ProfiloOrarioRepository dip_ProfiloOrarioRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_ProfiloOrarioRepository = dip_ProfiloOrarioRepository;
        }

        public virtual async Task<GenericResult<Dip_ProfiloOrarioOutModel>> GetAll(GenericRequest<Dip_ProfiloOrarioInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_ProfiloOrarioOutModel retVal = new Dip_ProfiloOrarioOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IDip_ProfiloOrarioService : IServiceBase
    {
        public Task<GenericResult<Dip_ProfiloOrarioOutModel>> GetAll( GenericRequest<Dip_ProfiloOrarioInModel> model, Boolean isSubProcess);
    }
}
