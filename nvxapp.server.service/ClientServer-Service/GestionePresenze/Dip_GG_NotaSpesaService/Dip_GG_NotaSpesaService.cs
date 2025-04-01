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

using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_NotaSpesaService.Models;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_NotaSpesa
{

    public class Dip_GG_NotaSpesaService : ServiceBase, IDip_GG_NotaSpesaService
    {
        private readonly IDip_GG_NotaSpesaRepository _dip_GG_NotaSpesaRepository;

        public Dip_GG_NotaSpesaService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,

                                  IDip_GG_NotaSpesaRepository dip_GG_NotaSpesaRepository) : base(mapper , userManager  , aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _dip_GG_NotaSpesaRepository = dip_GG_NotaSpesaRepository;
        }

        public virtual async Task<GenericResult<Dip_GG_NotaSpesaOutModel>> GetAll(GenericRequest<Dip_GG_NotaSpesaInModel> model, Boolean isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Dip_GG_NotaSpesaOutModel retVal = new Dip_GG_NotaSpesaOutModel();

                //eliminare
                // Nessun 'await' qui
                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }

    }

    public interface IDip_GG_NotaSpesaService : IServiceBase
    {
        public Task<GenericResult<Dip_GG_NotaSpesaOutModel>> GetAll( GenericRequest<Dip_GG_NotaSpesaInModel> model, Boolean isSubProcess);
    }
}
