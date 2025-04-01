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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_Anagrafica
{

    public class Dip_AnagraficaService : ServiceBase, IDip_AnagraficaService
    {
        private readonly IDip_AnagraficaRepository _dip_AnagraficaRepository;

        public Dip_AnagraficaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IDip_AnagraficaRepository dip_AnagraficaRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_AnagraficaRepository = dip_AnagraficaRepository;
        }

        public virtual async Task<GenericResult<Dip_AnagraficaOutModel>> GetAll(GenericRequest<Dip_AnagraficaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_AnagraficaOutModel retVal = new Dip_AnagraficaOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IDip_AnagraficaService : IServiceBase
    {
        public Task<GenericResult<Dip_AnagraficaOutModel>> GetAll( GenericRequest<Dip_AnagraficaInModel> model, Boolean isSubProcess);
    }
}
