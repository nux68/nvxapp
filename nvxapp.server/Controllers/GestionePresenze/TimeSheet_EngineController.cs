using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeSheet_EngineController : NvxControllerBase
    {
        private readonly ITimeSheet_EngineService _timeSheet_EngineService;

        public TimeSheet_EngineController(
                                        IHttpContextAccessor httpContextAccessor,
                                        ITimeSheet_EngineService timeSheet_EngineService
        ) : base(httpContextAccessor)
        {
            _timeSheet_EngineService = timeSheet_EngineService;
        }

        [Authorize]
        [HttpPost]
        [Route("Calculate")]
        public async Task<GenericResult<TimeSheet_CalculateOutModel>> Calculate(GenericRequest<TimeSheet_CalculateInModel> inModel)
        {
            var res = await _timeSheet_EngineService.Calculate(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Get_OrariSchema_4User")]
        public async Task<GenericResult<OrariSchema_4User_OutModel>> Get_OrariSchema_4User(GenericRequest<OrariSchema_4User_InModel> inModel)
        {
            var res = await _timeSheet_EngineService.Get_OrariSchema_4User(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_GG_AllData_AllData")]
        public async Task<GenericResult<Dip_GG_AllData_OutModel>> Dip_GG_AllData_AllData(GenericRequest<Dip_GG_AllData_InModel> inModel)
        {
            var res = await _timeSheet_EngineService.Dip_GG_AllData_AllData(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Get_Timesheet_AllData")]
        public async Task<GenericResult<Timesheet_AllData_OutModel>> Get_Timesheet_AllData(GenericRequest<Timesheet_AllData_InModel> inModel)
        {
            var res = await _timeSheet_EngineService.Get_Timesheet_AllData(inModel, false);
            return res;
        }
    }
}
