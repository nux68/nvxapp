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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_RepartoAttivitaService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_RepartoAttivita
{

    public class Az_RepartoAttivitaService : ServiceBase, IAz_RepartoAttivitaService
    {
        private readonly IAz_RepartoAttivitaRepository _az_RepartoAttivitaRepository;

        public Az_RepartoAttivitaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_RepartoAttivitaRepository az_RepartoAttivitaRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_RepartoAttivitaRepository = az_RepartoAttivitaRepository;
        }

        public virtual async Task<GenericResult<Az_RepartoAttivitaOutModel>> GetAll(GenericRequest<Az_RepartoAttivitaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_RepartoAttivitaOutModel retVal = new Az_RepartoAttivitaOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_RepartoAttivitaService : IServiceBase
    {
        public Task<GenericResult<Az_RepartoAttivitaOutModel>> GetAll( GenericRequest<Az_RepartoAttivitaInModel> model, Boolean isSubProcess);
    }
}
