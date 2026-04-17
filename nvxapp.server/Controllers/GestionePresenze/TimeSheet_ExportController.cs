using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_ExportService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    [ApiController]
    [Route("api/[controller]")]
    public class TimeSheet_ExportController : NvxControllerBase
    {
        private readonly ITimeSheet_ExportService _timeSheet_ExportService;

        public TimeSheet_ExportController(
            IHttpContextAccessor httpContextAccessor,
            ITimeSheet_ExportService timeSheet_ExportService
        ) : base(httpContextAccessor)
        {
            _timeSheet_ExportService = timeSheet_ExportService;
        }

        [Authorize]
        [HttpPost]
        [Route("Export")]
        public async Task<GenericResult<TimeSheet_ExportOutModel>> Export(GenericRequest<TimeSheet_ExportInModel> inModel)
        {
            var res = await _timeSheet_ExportService.Export(inModel, false);
            return res;
        }
    }
}
