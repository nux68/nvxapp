using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    [ApiController]
    [Route("api/[controller]")]
    public class ActivityStatisticsController : NvxControllerBase
    {
        private readonly IActivityStatisticsService _activityStatisticsService;

        public ActivityStatisticsController(
            IHttpContextAccessor httpContextAccessor,
            IActivityStatisticsService activityStatisticsService
        ) : base(httpContextAccessor)
        {
            _activityStatisticsService = activityStatisticsService;
        }

        [Authorize]
        [HttpPost]
        [Route("ActivityStatisticsGet")]
        public async Task<GenericResult<ActivityStatistics_GetOutModel>> ActivityStatisticsGet(GenericRequest<ActivityStatistics_GetInModel> inModel)
        {
            var res = await _activityStatisticsService.ActivityStatisticsGet(inModel, false);
            return res;
        }
    }
}
