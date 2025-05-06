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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoAttivitaService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoAttivitaService
{

    public class Az_SediRepartoAttivitaService : ServiceBase, IAz_SediRepartoAttivitaService
    {
        private readonly IAz_SediRepartoAttivitaRepository _az_SediRepartoAttivitaRepository;

        public Az_SediRepartoAttivitaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IAz_SediRepartoAttivitaRepository az_SediRepartoAttivitaRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SediRepartoAttivitaRepository = az_SediRepartoAttivitaRepository;
        }

        public virtual async Task<GenericResult<Az_SediRepartoAttivitaOutModel>> GetAll(GenericRequest<Az_SediRepartoAttivitaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SediRepartoAttivitaOutModel retVal = new Az_SediRepartoAttivitaOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IAz_SediRepartoAttivitaService : IServiceBase
    {
        public Task<GenericResult<Az_SediRepartoAttivitaOutModel>> GetAll( GenericRequest<Az_SediRepartoAttivitaInModel> model, Boolean isSubProcess);
    }
}
