using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Crm;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.Controllers.Crm
{
    // I permessi (ruoli e visibilita' dei commerciali) li controllano i servizi: qui solo l'autenticazione.

    [ApiController]
    [Route("api/[controller]")]
    public class Crm_ConfigController : NvxControllerBase
    {
        private readonly ICrm_ConfigService _service;
        public Crm_ConfigController(IHttpContextAccessor httpContextAccessor, ICrm_ConfigService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Crm_SetupGet")]
        public async Task<GenericResult<Crm_SetupGetOutModel>> Crm_SetupGet(GenericRequest<Crm_EmptyInModel> inModel) => await _service.Crm_SetupGet(inModel, false);
        [Authorize, HttpPost, Route("Crm_CfgPut")]
        public async Task<GenericResult<Crm_CfgPutOutModel>> Crm_CfgPut(GenericRequest<Crm_CfgPutInModel> inModel) => await _service.Crm_CfgPut(inModel, false);
        [Authorize, HttpPost, Route("Crm_ParPut")]
        public async Task<GenericResult<Crm_ParPutOutModel>> Crm_ParPut(GenericRequest<Crm_ParPutInModel> inModel) => await _service.Crm_ParPut(inModel, false);
        [Authorize, HttpPost, Route("Crm_ParDelete")]
        public async Task<GenericResult<Crm_IdOutModel>> Crm_ParDelete(GenericRequest<Crm_ParDeleteInModel> inModel) => await _service.Crm_ParDelete(inModel, false);
        [Authorize, HttpPost, Route("Crm_Riassegna")]
        public async Task<GenericResult<Crm_RiassegnaOutModel>> Crm_Riassegna(GenericRequest<Crm_RiassegnaInModel> inModel) => await _service.Crm_Riassegna(inModel, false);
    }

    [ApiController]
    [Route("api/[controller]")]
    public class Crm_ClienteController : NvxControllerBase
    {
        private readonly ICrm_ClienteService _service;
        public Crm_ClienteController(IHttpContextAccessor httpContextAccessor, ICrm_ClienteService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Crm_ClienteList")]
        public async Task<GenericResult<Crm_ClienteListOutModel>> Crm_ClienteList(GenericRequest<Crm_ClienteListInModel> inModel) => await _service.Crm_ClienteList(inModel, false);
        [Authorize, HttpPost, Route("Crm_ClienteGet")]
        public async Task<GenericResult<Crm_ClienteGetOutModel>> Crm_ClienteGet(GenericRequest<Crm_IdInModel> inModel) => await _service.Crm_ClienteGet(inModel, false);
        [Authorize, HttpPost, Route("Crm_ClientePut")]
        public async Task<GenericResult<Crm_ClientePutOutModel>> Crm_ClientePut(GenericRequest<Crm_ClientePutInModel> inModel) => await _service.Crm_ClientePut(inModel, false);
        [Authorize, HttpPost, Route("Crm_ContattoPut")]
        public async Task<GenericResult<Crm_ContattoPutOutModel>> Crm_ContattoPut(GenericRequest<Crm_ContattoPutInModel> inModel) => await _service.Crm_ContattoPut(inModel, false);
        [Authorize, HttpPost, Route("Crm_ContattoDelete")]
        public async Task<GenericResult<Crm_IdOutModel>> Crm_ContattoDelete(GenericRequest<Crm_IdInModel> inModel) => await _service.Crm_ContattoDelete(inModel, false);

        [Authorize, HttpPost, Route("Crm_ClienteExport")]
        public async Task<IActionResult> Crm_ClienteExport(GenericRequest<Crm_ClienteListInModel> inModel)
        {
            try
            {
                var file = await _service.Crm_ClienteExport(inModel.Data ?? new Crm_ClienteListInModel());
                return File(file.Content, file.ContentType, file.NomeFile);
            }
            catch (UserMessageException ex)
            {
                return BadRequest(ex.Message);
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class Crm_LeadController : NvxControllerBase
    {
        private readonly ICrm_LeadService _service;
        public Crm_LeadController(IHttpContextAccessor httpContextAccessor, ICrm_LeadService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Crm_LeadList")]
        public async Task<GenericResult<Crm_LeadListOutModel>> Crm_LeadList(GenericRequest<Crm_LeadListInModel> inModel) => await _service.Crm_LeadList(inModel, false);
        [Authorize, HttpPost, Route("Crm_LeadGet")]
        public async Task<GenericResult<Crm_LeadGetOutModel>> Crm_LeadGet(GenericRequest<Crm_IdInModel> inModel) => await _service.Crm_LeadGet(inModel, false);
        [Authorize, HttpPost, Route("Crm_LeadPut")]
        public async Task<GenericResult<Crm_LeadPutOutModel>> Crm_LeadPut(GenericRequest<Crm_LeadPutInModel> inModel) => await _service.Crm_LeadPut(inModel, false);
        [Authorize, HttpPost, Route("Crm_LeadDelete")]
        public async Task<GenericResult<Crm_IdOutModel>> Crm_LeadDelete(GenericRequest<Crm_IdInModel> inModel) => await _service.Crm_LeadDelete(inModel, false);
        [Authorize, HttpPost, Route("Crm_LeadConverti")]
        public async Task<GenericResult<Crm_LeadConvertiOutModel>> Crm_LeadConverti(GenericRequest<Crm_LeadConvertiInModel> inModel) => await _service.Crm_LeadConverti(inModel, false);
    }

    [ApiController]
    [Route("api/[controller]")]
    public class Crm_OpportunitaController : NvxControllerBase
    {
        private readonly ICrm_OpportunitaService _service;
        public Crm_OpportunitaController(IHttpContextAccessor httpContextAccessor, ICrm_OpportunitaService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Crm_OpportunitaList")]
        public async Task<GenericResult<Crm_OpportunitaListOutModel>> Crm_OpportunitaList(GenericRequest<Crm_OpportunitaListInModel> inModel) => await _service.Crm_OpportunitaList(inModel, false);
        [Authorize, HttpPost, Route("Crm_OpportunitaGet")]
        public async Task<GenericResult<Crm_OpportunitaGetOutModel>> Crm_OpportunitaGet(GenericRequest<Crm_IdInModel> inModel) => await _service.Crm_OpportunitaGet(inModel, false);
        [Authorize, HttpPost, Route("Crm_OpportunitaPut")]
        public async Task<GenericResult<Crm_OpportunitaPutOutModel>> Crm_OpportunitaPut(GenericRequest<Crm_OpportunitaPutInModel> inModel) => await _service.Crm_OpportunitaPut(inModel, false);
        [Authorize, HttpPost, Route("Crm_OpportunitaCambiaFase")]
        public async Task<GenericResult<Crm_OpportunitaPutOutModel>> Crm_OpportunitaCambiaFase(GenericRequest<Crm_CambiaFaseInModel> inModel) => await _service.Crm_OpportunitaCambiaFase(inModel, false);
        [Authorize, HttpPost, Route("Crm_OpportunitaDelete")]
        public async Task<GenericResult<Crm_IdOutModel>> Crm_OpportunitaDelete(GenericRequest<Crm_IdInModel> inModel) => await _service.Crm_OpportunitaDelete(inModel, false);
    }

    [ApiController]
    [Route("api/[controller]")]
    public class Crm_AttivitaController : NvxControllerBase
    {
        private readonly ICrm_AttivitaService _service;
        public Crm_AttivitaController(IHttpContextAccessor httpContextAccessor, ICrm_AttivitaService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Crm_AttivitaList")]
        public async Task<GenericResult<Crm_AttivitaListOutModel>> Crm_AttivitaList(GenericRequest<Crm_AttivitaListInModel> inModel) => await _service.Crm_AttivitaList(inModel, false);
        [Authorize, HttpPost, Route("Crm_AttivitaGet")]
        public async Task<GenericResult<Crm_AttivitaGetOutModel>> Crm_AttivitaGet(GenericRequest<Crm_IdInModel> inModel) => await _service.Crm_AttivitaGet(inModel, false);
        [Authorize, HttpPost, Route("Crm_AttivitaPut")]
        public async Task<GenericResult<Crm_AttivitaPutOutModel>> Crm_AttivitaPut(GenericRequest<Crm_AttivitaPutInModel> inModel) => await _service.Crm_AttivitaPut(inModel, false);
        [Authorize, HttpPost, Route("Crm_AttivitaCompleta")]
        public async Task<GenericResult<Crm_AttivitaPutOutModel>> Crm_AttivitaCompleta(GenericRequest<Crm_AttivitaCompletaInModel> inModel) => await _service.Crm_AttivitaCompleta(inModel, false);
        [Authorize, HttpPost, Route("Crm_AttivitaDelete")]
        public async Task<GenericResult<Crm_IdOutModel>> Crm_AttivitaDelete(GenericRequest<Crm_IdInModel> inModel) => await _service.Crm_AttivitaDelete(inModel, false);
    }

    [ApiController]
    [Route("api/[controller]")]
    public class Crm_OffertaController : NvxControllerBase
    {
        private readonly ICrm_OffertaService _service;
        public Crm_OffertaController(IHttpContextAccessor httpContextAccessor, ICrm_OffertaService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Crm_OffertaList")]
        public async Task<GenericResult<Crm_OffertaListOutModel>> Crm_OffertaList(GenericRequest<Crm_OffertaListInModel> inModel) => await _service.Crm_OffertaList(inModel, false);
        [Authorize, HttpPost, Route("Crm_OffertaGet")]
        public async Task<GenericResult<Crm_OffertaGetOutModel>> Crm_OffertaGet(GenericRequest<Crm_IdInModel> inModel) => await _service.Crm_OffertaGet(inModel, false);
        [Authorize, HttpPost, Route("Crm_OffertaPut")]
        public async Task<GenericResult<Crm_OffertaPutOutModel>> Crm_OffertaPut(GenericRequest<Crm_OffertaPutInModel> inModel) => await _service.Crm_OffertaPut(inModel, false);
        [Authorize, HttpPost, Route("Crm_OffertaDelete")]
        public async Task<GenericResult<Crm_IdOutModel>> Crm_OffertaDelete(GenericRequest<Crm_IdInModel> inModel) => await _service.Crm_OffertaDelete(inModel, false);
        [Authorize, HttpPost, Route("Crm_OffertaInvia")]
        public async Task<GenericResult<Crm_OffertaPutOutModel>> Crm_OffertaInvia(GenericRequest<Crm_OffertaInviaInModel> inModel) => await _service.Crm_OffertaInvia(inModel, false);

        [Authorize, HttpGet, Route("Crm_OffertaPdf")]
        public async Task<IActionResult> Crm_OffertaPdf([FromQuery] int id)
        {
            try
            {
                var file = await _service.Crm_OffertaPdf(id);
                return File(file.Content, file.ContentType, file.NomeFile);
            }
            catch (UserMessageException ex)
            {
                return NotFound(ex.Message);
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class Crm_NotaAllegatoController : NvxControllerBase
    {
        // limite della richiesta: allegato + margine per i campi del modulo
        private const long MaxRichiesta = Crm_NotaAllegatoService.MaxAllegato + 1024 * 1024;

        private readonly ICrm_NotaAllegatoService _service;
        public Crm_NotaAllegatoController(IHttpContextAccessor httpContextAccessor, ICrm_NotaAllegatoService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Crm_NotaPut")]
        public async Task<GenericResult<Crm_NotaPutOutModel>> Crm_NotaPut(GenericRequest<Crm_NotaPutInModel> inModel) => await _service.Crm_NotaPut(inModel, false);
        [Authorize, HttpPost, Route("Crm_NotaDelete")]
        public async Task<GenericResult<Crm_IdOutModel>> Crm_NotaDelete(GenericRequest<Crm_IdInModel> inModel) => await _service.Crm_NotaDelete(inModel, false);
        [Authorize, HttpPost, Route("Crm_AllegatoDelete")]
        public async Task<GenericResult<Crm_IdOutModel>> Crm_AllegatoDelete(GenericRequest<Crm_IdInModel> inModel) => await _service.Crm_AllegatoDelete(inModel, false);

        [Authorize, HttpPost, Route("Crm_AllegatoUpload")]
        [RequestSizeLimit(MaxRichiesta), RequestFormLimits(MultipartBodyLengthLimit = MaxRichiesta)]
        public async Task<GenericResult<Crm_AllegatoUploadOutModel>> Crm_AllegatoUpload([FromForm] string data, IFormFile file)
        {
            var request = new GenericRequest<Crm_AllegatoUploadModel> { Data = JsonConvert.DeserializeObject<Crm_AllegatoUploadModel>(data) ?? new Crm_AllegatoUploadModel() };
            await using var stream = file.OpenReadStream();
            return await _service.Crm_AllegatoUpload(request, stream, file.FileName, file.ContentType, file.Length, false);
        }

        [Authorize, HttpGet, Route("Crm_AllegatoDownload")]
        public async Task<IActionResult> Crm_AllegatoDownload([FromQuery] int id)
        {
            try
            {
                var file = await _service.Crm_AllegatoDownload(id);
                return File(file.Content, file.ContentType, file.NomeFile);
            }
            catch (UserMessageException ex)
            {
                return NotFound(ex.Message);
            }
            catch (FileNotFoundException)
            {
                return NotFound(CrmMessages.Get("FileMissing"));
            }
        }
    }

    [ApiController]
    [Route("api/[controller]")]
    public class Crm_ReportController : NvxControllerBase
    {
        private readonly ICrm_ReportService _service;
        public Crm_ReportController(IHttpContextAccessor httpContextAccessor, ICrm_ReportService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Crm_Report")]
        public async Task<GenericResult<Crm_ReportOutModel>> Crm_Report(GenericRequest<Crm_ReportInModel> inModel) => await _service.Crm_Report(inModel, false);
    }
}
