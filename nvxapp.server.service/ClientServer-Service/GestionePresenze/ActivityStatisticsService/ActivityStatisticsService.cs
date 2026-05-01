using AutoMapper;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Identity;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Options;
using nvxapp.server.Base;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Interfaces;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ActivityStatisticsService
{
    public class ActivityStatisticsService : ServiceBase, IActivityStatisticsService
    {

        private readonly ITimeSheet_EngineService _timeSheet_EngineService;

        public ActivityStatisticsService(IMapper mapper,
                                         UserManager<ApplicationUser> userManager,
                                         IAspNetUsersRepository aspNetUsersRepository,
                                         IOptions<JwtParameter> jwtParameter,
                                         IHttpContextAccessor httpContextAccessor,
                                         IConfiguration configuration,
                                         ITimeSheet_EngineService timeSheet_EngineService

                                         ) : base(mapper, userManager, aspNetUsersRepository, jwtParameter, configuration, httpContextAccessor)
        {
        
            _timeSheet_EngineService = timeSheet_EngineService;
        }

        public virtual async Task<GenericResult<ActivityStatistics_GetOutModel>> ActivityStatisticsGet(GenericRequest<ActivityStatistics_GetInModel> model, bool isSubProcess)
        {
            return await ExecuteAction(model, async () =>
            {
                ActivityStatistics_GetOutModel retVal = new ActivityStatistics_GetOutModel();

                DateTime dal = new DateTime(model.Data.ActivityStatistics.Year, model.Data.ActivityStatistics.Month, 1);
                DateTime al = new DateTime(model.Data.ActivityStatistics.Year, model.Data.ActivityStatistics.Month, DateTime.DaysInMonth(model.Data.ActivityStatistics.Year, model.Data.ActivityStatistics.Month));

                // TODO: implementare la logica di recupero delle statistiche attività
                var req_AllData_4User = new GenericRequest<Timesheet_AllData_InModel>();
                req_AllData_4User.Data.Dal = dal;
                req_AllData_4User.Data.Al = al;
                req_AllData_4User.Data.UsersId = model.Data.ActivityStatistics.SelectedUserId;
                var cc = await _timeSheet_EngineService.Get_Timesheet_AllData(req_AllData_4User,true);

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
