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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_Richiesta
{

    public class Dip_GG_RichiestaService : ServiceBase, IDip_GG_RichiestaService
    {
        private readonly IDip_GG_RichiestaRepository _dip_GG_RichiestaRepository;

        public Dip_GG_RichiestaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IDip_GG_RichiestaRepository dip_GG_RichiestaRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_GG_RichiestaRepository = dip_GG_RichiestaRepository;
        }

        public virtual async Task<GenericResult<Dip_GG_RichiestaOutModel>> GetAll(GenericRequest<Dip_GG_RichiestaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_RichiestaOutModel retVal = new Dip_GG_RichiestaOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IDip_GG_RichiestaService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_RichiestaOutModel>> GetAll( GenericRequest<Dip_GG_RichiestaInModel> model, Boolean isSubProcess);
    }
}
