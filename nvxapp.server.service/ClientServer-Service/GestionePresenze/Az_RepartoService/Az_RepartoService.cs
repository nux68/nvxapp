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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_RepartoService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_Reparto
{

    public class Az_RepartoService : ServiceBase, IAz_RepartoService
    {
        private readonly IAz_RepartoRepository _az_RepartoRepository;

        public Az_RepartoService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_RepartoRepository az_RepartoRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_RepartoRepository = az_RepartoRepository;
        }

        public virtual async Task<GenericResult<Az_RepartoOutModel>> GetAll(GenericRequest<Az_RepartoInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_RepartoOutModel retVal = new Az_RepartoOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_RepartoService : IServiceBase
    {
        public Task<GenericResult<Az_RepartoOutModel>> GetAll( GenericRequest<Az_RepartoInModel> model, Boolean isSubProcess);
    }
}
