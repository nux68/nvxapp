using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService.Models;
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


    }



}
