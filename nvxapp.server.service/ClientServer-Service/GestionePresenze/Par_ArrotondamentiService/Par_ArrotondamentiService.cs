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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ArrotondamentiService.Models;
namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_Arrotondamenti
{

    public class Par_ArrotondamentiService : ServiceBase, IPar_ArrotondamentiService
    {
        private readonly IPar_ArrotondamentiRepository _Par_ArrotondamentiRepository;

        public Par_ArrotondamentiService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_ArrotondamentiRepository Par_ArrotondamentiRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Par_ArrotondamentiRepository = Par_ArrotondamentiRepository;
        }

        public virtual async Task<GenericResult<Par_ArrotondamentiOutModel>> GetAll(GenericRequest<Par_ArrotondamentiInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_ArrotondamentiOutModel retVal = new Par_ArrotondamentiOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IPar_ArrotondamentiService : IServiceBase
    {
        public Task<GenericResult<Par_ArrotondamentiOutModel>> GetAll( GenericRequest<Par_ArrotondamentiInModel> model, Boolean isSubProcess);
    }
}
