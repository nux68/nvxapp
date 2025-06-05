using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.data.Repositories.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaSediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;
using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaSediRepartoService
{
    public class Az_SubCommessaSediRepartoService : ServiceBase, IAz_SubCommessaSediRepartoService
    {
        private readonly IAz_SubCommessaSediRepartoRepository _az_SubCommessaSediRepartoRepository;

        public Az_SubCommessaSediRepartoService(IMapper mapper,
                                  UserManager<ApplicationUser> userManager,
                                  IAspNetUsersRepository aspNetUsersRepository,
                                  IOptions<JwtParameter> jwtParameter,
                                  IHttpContextAccessor httpContextAccessor,
                                  IConfiguration configuration,
                                  IAz_SubCommessaSediRepartoRepository az_SubCommessaSediRepartoRepository) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
            _az_SubCommessaSediRepartoRepository = az_SubCommessaSediRepartoRepository;
        }

        public virtual async Task<GenericResult<Az_SubCommessaSediReparto_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaSediReparto_GetAll_InModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                Az_SubCommessaSediReparto_GetAll_OutModel retVal = new Az_SubCommessaSediReparto_GetAll_OutModel();
                var entities = await _az_SubCommessaSediRepartoRepository.FindAll();
                retVal.Az_SubCommessaSediReparto = _mapper.Map<List<Az_SubCommessaSediRepartoModel>>(entities);
                return retVal;
            }, isSubProcess);
        }
    }

    public interface IAz_SubCommessaSediRepartoService : IServiceBase
    {
        Task<GenericResult<Az_SubCommessaSediReparto_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaSediReparto_GetAll_InModel> model, bool isSubProcess);
    }
}
