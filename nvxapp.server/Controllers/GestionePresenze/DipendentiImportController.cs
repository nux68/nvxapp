using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.DipendentiImportService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.DipendentiImportService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    [ApiController]
    [Route("api/[controller]")]
    public class DipendentiImportController : NvxControllerBase
    {
        private readonly IDipendentiImportService _dipendentiImportService;

        public DipendentiImportController(
            IHttpContextAccessor httpContextAccessor,
            IDipendentiImportService dipendentiImportService
        ) : base(httpContextAccessor)
        {
            _dipendentiImportService = dipendentiImportService;
        }

        // dipendenti letti dal file con il LLM (il file arriva in base64: fino a 5 MB)
        [Authorize]
        [HttpPost]
        [Route("DipendentiImportEstrai")]
        public async Task<GenericResult<DipendentiImport_EstraiOutModel>> DipendentiImportEstrai(GenericRequest<DipendentiImport_EstraiInModel> inModel)
        {
            var res = await _dipendentiImportService.DipendentiImportEstrai(inModel, false);
            return res;
        }

        // inserimento dei dipendenti corretti nel dialog: avvia il job, l'avanzamento arriva con LongJobProgress
        [Authorize]
        [HttpPost]
        [Route("DipendentiImportEsegui")]
        public async Task<GenericResult<DipendentiImport_EseguiOutModel>> DipendentiImportEsegui(GenericRequest<DipendentiImport_EseguiInModel> inModel)
        {
            var res = await _dipendentiImportService.DipendentiImportEsegui(inModel, false);
            return res;
        }
    }
}
