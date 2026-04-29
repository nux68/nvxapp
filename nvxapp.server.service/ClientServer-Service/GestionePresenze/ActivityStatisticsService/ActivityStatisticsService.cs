using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService
{
    public class ActivityStatisticsService : ServiceBase, IActivityStatisticsService
    {
        public ActivityStatisticsService(IMapper mapper,
                                         UserManager<ApplicationUser> userManager,
                                         IAspNetUsersRepository aspNetUsersRepository,
                                         IOptions<JwtParameter> jwtParameter,
                                         IHttpContextAccessor httpContextAccessor,
                                         IConfiguration configuration
                                         ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
        }

        public virtual async Task<GenericResult<ActivityStatistics_GetOutModel>> ActivityStatisticsGet(GenericRequest<ActivityStatistics_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                ActivityStatistics_GetOutModel retVal = new ActivityStatistics_GetOutModel();

                // TODO: implementare la logica di recupero delle statistiche attività

                await Task.Delay(DelayAsyncMethod);

                return retVal;
            }, isSubProcess);
        }
    }

    public interface IActivityStatisticsService : IServiceBase
    {
        Task<GenericResult<ActivityStatistics_GetOutModel>> ActivityStatisticsGet(GenericRequest<ActivityStatistics_GetInModel> model, bool isSubProcess);
    }
}
