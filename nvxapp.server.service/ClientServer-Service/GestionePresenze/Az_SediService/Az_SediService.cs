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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_Sedi
{

    public class Az_SediService : ServiceBase, IAz_SediService
    {
        private readonly IAz_SediRepository _az_SediRepository;

        public Az_SediService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_SediRepository az_SediRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SediRepository = az_SediRepository;
        }

        public virtual async Task<GenericResult<Az_SediOutModel>> GetAll(GenericRequest<Az_SediInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediOutModel retVal = new Az_SediOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_SediService : IServiceBase
    {
        public Task<GenericResult<Az_SediOutModel>> GetAll( GenericRequest<Az_SediInModel> model, Boolean isSubProcess);
    }
}
