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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService
{

    public class Par_GiustificativiService : ServiceBase, IPar_GiustificativiService
    {
        private readonly IPar_GiustificativiRepository _par_GiustificativiRepository;

        public Par_GiustificativiService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IPar_GiustificativiRepository par_GiustificativiRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _par_GiustificativiRepository = par_GiustificativiRepository;
        }

        public virtual async Task<GenericResult<Par_GiustificativiOutModel>> GetAll(GenericRequest<Par_GiustificativiInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Par_GiustificativiOutModel retVal = new Par_GiustificativiOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IPar_GiustificativiService : IServiceBase
    {
        public Task<GenericResult<Par_GiustificativiOutModel>> GetAll( GenericRequest<Par_GiustificativiInModel> model, Boolean isSubProcess);
    }
}
