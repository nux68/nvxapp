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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_Causali
{

    public class Dip_GG_CausaliService : ServiceBase, IDip_GG_CausaliService
    {
        private readonly IDip_GG_CausaliRepository _Dip_GG_CausaliRepository;

        public Dip_GG_CausaliService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IDip_GG_CausaliRepository Dip_GG_CausaliRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _Dip_GG_CausaliRepository = Dip_GG_CausaliRepository;
        }

        public virtual async Task<GenericResult<Dip_GG_CausaliOutModel>> GetAll(GenericRequest<Dip_GG_CausaliInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_CausaliOutModel retVal = new Dip_GG_CausaliOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IDip_GG_CausaliService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_CausaliOutModel>> GetAll( GenericRequest<Dip_GG_CausaliInModel> model, Boolean isSubProcess);
    }
}
