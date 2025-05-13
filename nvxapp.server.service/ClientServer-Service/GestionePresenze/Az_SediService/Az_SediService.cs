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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService
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

        public virtual async Task<GenericResult<Az_Sedi_GetAll_OutModel>> GetAll(GenericRequest<Az_Sedi_GetAll_InModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_Sedi_GetAll_OutModel retVal = new Az_Sedi_GetAll_OutModel();


                var az_Sedi = _az_SediRepository.FindAll(x => x.Id > 0).ToList();

                retVal.Az_Sedi = _mapper.Map<List<Az_SediModel>>(az_Sedi);

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_SediService : IServiceBase
    {
        public Task<GenericResult<Az_Sedi_GetAll_OutModel>> GetAll( GenericRequest<Az_Sedi_GetAll_InModel> model, Boolean isSubProcess);
    }
}
