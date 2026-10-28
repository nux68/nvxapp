using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ReportService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ReportService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    [ApiController]
    [Route("api/[controller]")]
    public class ReportController : NvxControllerBase
    {
        private readonly IReportService _reportService;

        public ReportController(
            IHttpContextAccessor httpContextAccessor,
            IReportService reportService
        ) : base(httpContextAccessor)
        {
            _reportService = reportService;
        }

        // report PDF delle pagine HR per i dipendenti selezionati (il servizio controlla ruolo e reparti):
        // avvia il job, il file arriva con la notifica di fine job
        [Authorize]
        [HttpPost]
        [Route("ReportExportPdf")]
        public async Task<GenericResult<Report_OutModel>> ReportExportPdf(GenericRequest<Report_InModel> inModel)
        {
            var res = await _reportService.ReportExportPdf(inModel, false);
            return res;
        }
    }
}
