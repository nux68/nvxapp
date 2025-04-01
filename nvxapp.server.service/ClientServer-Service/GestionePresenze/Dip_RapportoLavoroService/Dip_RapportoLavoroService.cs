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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoro
{

    public class Dip_RapportoLavoroService : ServiceBase, IDip_RapportoLavoroService
    {
        private readonly IDip_RapportoLavoroRepository _dip_RapportoLavoroRepository;

        public Dip_RapportoLavoroService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IDip_RapportoLavoroRepository dip_RapportoLavoroRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_RapportoLavoroRepository = dip_RapportoLavoroRepository;
        }

        public virtual async Task<GenericResult<Dip_RapportoLavoroOutModel>> GetAll(GenericRequest<Dip_RapportoLavoroInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_RapportoLavoroOutModel retVal = new Dip_RapportoLavoroOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IDip_RapportoLavoroService : IServiceBase
    {
        public Task<GenericResult<Dip_RapportoLavoroOutModel>> GetAll( GenericRequest<Dip_RapportoLavoroInModel> model, Boolean isSubProcess);
    }
}
