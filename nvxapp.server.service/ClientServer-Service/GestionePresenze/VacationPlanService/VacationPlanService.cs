using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.VacationPlanService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.VacationPlanService
{
    public class VacationPlanService : ServiceBase, IVacationPlanService
    {
        public VacationPlanService(IMapper mapper,
                                   UserManager<ApplicationUser> userManager,
                                   IAspNetUsersRepository aspNetUsersRepository,
                                   IOptions<JwtParameter> jwtParameter,
                                   IHttpContextAccessor httpContextAccessor,
                                   IConfiguration configuration
                                   ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
        }

        public virtual async Task<GenericResult<VacationPlan_GetOutModel>> VacationPlanGet(GenericRequest<VacationPlan_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                VacationPlan_GetOutModel retVal = new VacationPlan_GetOutModel();

                // TODO: implementare la logica di recupero del piano ferie

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IVacationPlanService : IServiceBase
    {
        Task<GenericResult<VacationPlan_GetOutModel>> VacationPlanGet(GenericRequest<VacationPlan_GetInModel> model, bool isSubProcess);
    }
}
