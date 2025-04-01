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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_Timbratura
{

    public class Dip_GG_TimbraturaService : ServiceBase, IDip_GG_TimbraturaService
    {
        private readonly IDip_GG_TimbraturaRepository _dip_GG_TimbraturaRepository;

        public Dip_GG_TimbraturaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IDip_GG_TimbraturaRepository dip_GG_TimbraturaRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_GG_TimbraturaRepository = dip_GG_TimbraturaRepository;
        }

        public virtual async Task<GenericResult<Dip_GG_TimbraturaOutModel>> GetAll(GenericRequest<Dip_GG_TimbraturaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_TimbraturaOutModel retVal = new Dip_GG_TimbraturaOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IDip_GG_TimbraturaService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_TimbraturaOutModel>> GetAll( GenericRequest<Dip_GG_TimbraturaInModel> model, Boolean isSubProcess);
    }
}
