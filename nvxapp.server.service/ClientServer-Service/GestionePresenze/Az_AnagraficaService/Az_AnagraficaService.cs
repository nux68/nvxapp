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
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AnagraficaService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AnagraficaService
{

    public class Az_AnagraficaService : ServiceBase, IAz_AnagraficaService
    {
        private readonly IAz_AnagraficaRepository _az_AnagraficaRepository;

        public Az_AnagraficaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_AnagraficaRepository az_AnagraficaRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_AnagraficaRepository = az_AnagraficaRepository;
        }

        public virtual async Task<GenericResult<Az_AnagraficaOutModel>> GetAll(GenericRequest<Az_AnagraficaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_AnagraficaOutModel retVal = new Az_AnagraficaOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_AnagraficaService : IServiceBase
    {
        public Task<GenericResult<Az_AnagraficaOutModel>> GetAll( GenericRequest<Az_AnagraficaInModel> model, Boolean isSubProcess);
    }
}
