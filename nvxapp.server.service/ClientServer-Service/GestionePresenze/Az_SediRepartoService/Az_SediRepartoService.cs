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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService
{

    public class Az_SediRepartoService : ServiceBase, IAz_SediRepartoService
    {
        private readonly IAz_SediRepartoRepository _az_RepartoRepository;

        public Az_SediRepartoService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_SediRepartoRepository az_RepartoRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_RepartoRepository = az_RepartoRepository;
        }

        public virtual async Task<GenericResult<Az_SediRepartoOutModel>> GetAll(GenericRequest<Az_SediRepartoInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediRepartoOutModel retVal = new Az_SediRepartoOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_SediRepartoService : IServiceBase
    {
        public Task<GenericResult<Az_SediRepartoOutModel>> GetAll( GenericRequest<Az_SediRepartoInModel> model, Boolean isSubProcess);
    }
}
