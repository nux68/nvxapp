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

        // cartellino PDF del mese dell'utente collegato
        [Authorize]
        [HttpPost]
        [Route("ExportPdf_4User")]
        public async Task<GenericResult<TimeSheet_ExportPdf_4User_OutModel>> ExportPdf_4User(GenericRequest<TimeSheet_ExportPdf_4User_InModel> inModel)
        {
            var res = await _timeSheet_ExportService.ExportPdf_4User(inModel, false);
            return res;
        }

        // cartellini PDF del mese dei dipendenti scelti dall'admin (il servizio controlla ruolo e reparti)
        [Authorize]
        [HttpPost]
        [Route("ExportPdf_4Admin")]
        public async Task<GenericResult<TimeSheet_ExportPdf_4Admin_OutModel>> ExportPdf_4Admin(GenericRequest<TimeSheet_ExportPdf_4Admin_InModel> inModel)
        {
            var res = await _timeSheet_ExportService.ExportPdf_4Admin(inModel, false);
            return res;
        }
    }
}
