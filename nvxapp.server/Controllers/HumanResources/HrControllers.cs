using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.HumanResources;
using nvxapp.server.service.ClientServer_Service.HumanResources._utility;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.Controllers.HumanResources
{
    // I permessi (ruoli dell'azienda e dello studio) li controllano i servizi: qui solo l'autenticazione.

    [ApiController]
    [Route("api/[controller]")]
    public class Hr_ConfigController : NvxControllerBase
    {
        private readonly IHr_ConfigService _service;
        public Hr_ConfigController(IHttpContextAccessor httpContextAccessor, IHr_ConfigService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Hr_CfgGet")]
        public async Task<GenericResult<Hr_CfgGetOutModel>> Hr_CfgGet(GenericRequest<Hr_EmptyInModel> inModel) => await _service.Hr_CfgGet(inModel, false);
        [Authorize, HttpPost, Route("Hr_CfgPut")]
        public async Task<GenericResult<Hr_CfgPutOutModel>> Hr_CfgPut(GenericRequest<Hr_CfgPutInModel> inModel) => await _service.Hr_CfgPut(inModel, false);
    }

    // file salvati (cedolini, attestati, documenti delle checklist): il messaggio d'errore nel corpo, come gli allegati del CRM
    public static class HrFile
    {
        public static async Task<IActionResult> Download(ControllerBase c, Func<Task<Hr_FileDownload>> file)
        {
            try
            {
                var f = await file();
                return c.File(f.Content, f.ContentType, f.NomeFile);
            }
            catch (UserMessageException ex)
            {
                return c.NotFound(ex.Message);
            }
            catch (FileNotFoundException)
            {
                return c.NotFound(HumanResourcesMessages.Get("DocumentNotFound"));
            }
            catch (DirectoryNotFoundException)
            {
                return c.NotFound(HumanResourcesMessages.Get("DocumentNotFound"));
            }
        }

        public static GenericRequest<T> Request<T>(string data) where T : new()
            => new() { Data = JsonConvert.DeserializeObject<T>(data ?? "{}") ?? new T() };
    }

    [ApiController]
    [Route("api/[controller]")]
    public class Hr_CedolinoController : NvxControllerBase
    {
        private const long MaxRichiesta = HrCedoliniCore.MaxPdf + 1024 * 1024;
        private readonly IHr_CedolinoService _service;
        public Hr_CedolinoController(IHttpContextAccessor httpContextAccessor, IHr_CedolinoService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Hr_StudioAziende")]
        public async Task<GenericResult<Hr_StudioAziendeOutModel>> Hr_StudioAziende(GenericRequest<Hr_EmptyInModel> inModel) => await _service.Hr_StudioAziende(inModel, false);
        [Authorize, HttpPost, Route("Hr_CaricamentoList")]
        public async Task<GenericResult<Hr_CaricamentoListOutModel>> Hr_CaricamentoList(GenericRequest<Hr_CaricamentoListInModel> inModel) => await _service.Hr_CaricamentoList(inModel, false);
        [Authorize, HttpPost, Route("Hr_CaricamentoGet")]
        public async Task<GenericResult<Hr_CaricamentoGetOutModel>> Hr_CaricamentoGet(GenericRequest<Hr_CedIdInModel> inModel) => await _service.Hr_CaricamentoGet(inModel, false);
        [Authorize, HttpPost, Route("Hr_ScartoAssegna")]
        public async Task<GenericResult<Hr_CaricamentoGetOutModel>> Hr_ScartoAssegna(GenericRequest<Hr_ScartoAssegnaInModel> inModel) => await _service.Hr_ScartoAssegna(inModel, false);
        [Authorize, HttpPost, Route("Hr_ScartoIgnora")]
        public async Task<GenericResult<Hr_CaricamentoGetOutModel>> Hr_ScartoIgnora(GenericRequest<Hr_CedIdInModel> inModel) => await _service.Hr_ScartoIgnora(inModel, false);
        [Authorize, HttpPost, Route("Hr_CaricamentoPubblica")]
        public async Task<GenericResult<Hr_JobOutModel>> Hr_CaricamentoPubblica(GenericRequest<Hr_CedIdInModel> inModel) => await _service.Hr_CaricamentoPubblica(inModel, false);
        [Authorize, HttpPost, Route("Hr_CaricamentoDelete")]
        public async Task<GenericResult<Hr_IdOutModel>> Hr_CaricamentoDelete(GenericRequest<Hr_CedIdInModel> inModel) => await _service.Hr_CaricamentoDelete(inModel, false);
        [Authorize, HttpPost, Route("Hr_MieiDocumenti")]
        public async Task<GenericResult<Hr_MieiDocumentiOutModel>> Hr_MieiDocumenti(GenericRequest<Hr_EmptyInModel> inModel) => await _service.Hr_MieiDocumenti(inModel, false);

        [Authorize, HttpPost, Route("Hr_CaricamentoUpload")]
        [RequestSizeLimit(MaxRichiesta), RequestFormLimits(MultipartBodyLengthLimit = MaxRichiesta)]
        public async Task<GenericResult<Hr_CaricamentoUploadOutModel>> Hr_CaricamentoUpload([FromForm] string data, IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            return await _service.Hr_CaricamentoUpload(HrFile.Request<Hr_CaricamentoUploadModel>(data), stream, file.FileName, file.Length, false);
        }

        [Authorize, HttpGet, Route("Hr_DocDownload")]
        public Task<IActionResult> Hr_DocDownload([FromQuery] int id, [FromQuery] int? idCompany) => HrFile.Download(this, () => _service.Hr_DocDownload(id, idCompany));

        [Authorize, HttpGet, Route("Hr_MioDocDownload")]
        public Task<IActionResult> Hr_MioDocDownload([FromQuery] int id) => HrFile.Download(this, () => _service.Hr_MioDocDownload(id));
    }

    [ApiController]
    [Route("api/[controller]")]
    public class Hr_DipendenteController : NvxControllerBase
    {
        private readonly IHr_DipendenteService _service;
        public Hr_DipendenteController(IHttpContextAccessor httpContextAccessor, IHr_DipendenteService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Hr_DipendenteList")]
        public async Task<GenericResult<Hr_DipendenteListOutModel>> Hr_DipendenteList(GenericRequest<Hr_DipendenteListInModel> inModel) => await _service.Hr_DipendenteList(inModel, false);
        [Authorize, HttpPost, Route("Hr_DipendenteGet")]
        public async Task<GenericResult<Hr_DipendenteGetOutModel>> Hr_DipendenteGet(GenericRequest<Hr_IdInModel> inModel) => await _service.Hr_DipendenteGet(inModel, false);
        [Authorize, HttpPost, Route("Hr_DipendentePut")]
        public async Task<GenericResult<Hr_DipendentePutOutModel>> Hr_DipendentePut(GenericRequest<Hr_DipendentePutInModel> inModel) => await _service.Hr_DipendentePut(inModel, false);
        [Authorize, HttpPost, Route("Hr_AccountCrea")]
        public async Task<GenericResult<Hr_AccountOutModel>> Hr_AccountCrea(GenericRequest<Hr_AccountCreaInModel> inModel) => await _service.Hr_AccountCrea(inModel, false);
        [Authorize, HttpPost, Route("Hr_AccountDisattiva")]
        public async Task<GenericResult<Hr_AccountOutModel>> Hr_AccountDisattiva(GenericRequest<Hr_AccountDisattivaInModel> inModel) => await _service.Hr_AccountDisattiva(inModel, false);
    }

    [ApiController]
    [Route("api/[controller]")]
    public class Hr_FormazioneController : NvxControllerBase
    {
        private const long MaxRichiesta = Hr_FormazioneService.MaxAttestato + 1024 * 1024;
        private readonly IHr_FormazioneService _service;
        public Hr_FormazioneController(IHttpContextAccessor httpContextAccessor, IHr_FormazioneService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Hr_CorsoList")]
        public async Task<GenericResult<Hr_CorsoListOutModel>> Hr_CorsoList(GenericRequest<Hr_EmptyInModel> inModel) => await _service.Hr_CorsoList(inModel, false);
        [Authorize, HttpPost, Route("Hr_CorsoGet")]
        public async Task<GenericResult<Hr_CorsoGetOutModel>> Hr_CorsoGet(GenericRequest<Hr_IdInModel> inModel) => await _service.Hr_CorsoGet(inModel, false);
        [Authorize, HttpPost, Route("Hr_CorsoPut")]
        public async Task<GenericResult<Hr_CorsoPutOutModel>> Hr_CorsoPut(GenericRequest<Hr_CorsoPutInModel> inModel) => await _service.Hr_CorsoPut(inModel, false);
        [Authorize, HttpPost, Route("Hr_CorsoDelete")]
        public async Task<GenericResult<Hr_IdOutModel>> Hr_CorsoDelete(GenericRequest<Hr_IdInModel> inModel) => await _service.Hr_CorsoDelete(inModel, false);
        [Authorize, HttpPost, Route("Hr_EdizioneGet")]
        public async Task<GenericResult<Hr_EdizioneGetOutModel>> Hr_EdizioneGet(GenericRequest<Hr_EdizioneGetInModel> inModel) => await _service.Hr_EdizioneGet(inModel, false);
        [Authorize, HttpPost, Route("Hr_EdizionePut")]
        public async Task<GenericResult<Hr_EdizionePutOutModel>> Hr_EdizionePut(GenericRequest<Hr_EdizionePutInModel> inModel) => await _service.Hr_EdizionePut(inModel, false);
        [Authorize, HttpPost, Route("Hr_EdizioneDelete")]
        public async Task<GenericResult<Hr_IdOutModel>> Hr_EdizioneDelete(GenericRequest<Hr_IdInModel> inModel) => await _service.Hr_EdizioneDelete(inModel, false);
        [Authorize, HttpPost, Route("Hr_PianoList")]
        public async Task<GenericResult<Hr_PianoListOutModel>> Hr_PianoList(GenericRequest<Hr_PianoListInModel> inModel) => await _service.Hr_PianoList(inModel, false);
        [Authorize, HttpPost, Route("Hr_PianoGet")]
        public async Task<GenericResult<Hr_PianoGetOutModel>> Hr_PianoGet(GenericRequest<Hr_IdInModel> inModel) => await _service.Hr_PianoGet(inModel, false);
        [Authorize, HttpPost, Route("Hr_PianoPut")]
        public async Task<GenericResult<Hr_PianoPutOutModel>> Hr_PianoPut(GenericRequest<Hr_PianoPutInModel> inModel) => await _service.Hr_PianoPut(inModel, false);
        [Authorize, HttpPost, Route("Hr_PianoDelete")]
        public async Task<GenericResult<Hr_IdOutModel>> Hr_PianoDelete(GenericRequest<Hr_IdInModel> inModel) => await _service.Hr_PianoDelete(inModel, false);

        [Authorize, HttpPost, Route("Hr_AttestatoUpload")]
        [RequestSizeLimit(MaxRichiesta), RequestFormLimits(MultipartBodyLengthLimit = MaxRichiesta)]
        public async Task<GenericResult<Hr_AttestatoUploadOutModel>> Hr_AttestatoUpload([FromForm] string data, IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            return await _service.Hr_AttestatoUpload(HrFile.Request<Hr_AttestatoUploadModel>(data), stream, file.FileName, file.ContentType, file.Length, false);
        }

        [Authorize, HttpGet, Route("Hr_AttestatoDownload")]
        public Task<IActionResult> Hr_AttestatoDownload([FromQuery] int id) => HrFile.Download(this, () => _service.Hr_AttestatoDownload(id));
    }

    [ApiController]
    [Route("api/[controller]")]
    public class Hr_OnboardingController : NvxControllerBase
    {
        private const long MaxRichiesta = Hr_OnboardingService.MaxDocumento + 1024 * 1024;
        private readonly IHr_OnboardingService _service;
        public Hr_OnboardingController(IHttpContextAccessor httpContextAccessor, IHr_OnboardingService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Hr_ModelloList")]
        public async Task<GenericResult<Hr_ModelloListOutModel>> Hr_ModelloList(GenericRequest<Hr_EmptyInModel> inModel) => await _service.Hr_ModelloList(inModel, false);
        [Authorize, HttpPost, Route("Hr_ModelloGet")]
        public async Task<GenericResult<Hr_ModelloGetOutModel>> Hr_ModelloGet(GenericRequest<Hr_IdInModel> inModel) => await _service.Hr_ModelloGet(inModel, false);
        [Authorize, HttpPost, Route("Hr_ModelloPut")]
        public async Task<GenericResult<Hr_ModelloPutOutModel>> Hr_ModelloPut(GenericRequest<Hr_ModelloPutInModel> inModel) => await _service.Hr_ModelloPut(inModel, false);
        [Authorize, HttpPost, Route("Hr_ModelloDelete")]
        public async Task<GenericResult<Hr_IdOutModel>> Hr_ModelloDelete(GenericRequest<Hr_IdInModel> inModel) => await _service.Hr_ModelloDelete(inModel, false);
        [Authorize, HttpPost, Route("Hr_ChecklistList")]
        public async Task<GenericResult<Hr_ChecklistListOutModel>> Hr_ChecklistList(GenericRequest<Hr_ChecklistListInModel> inModel) => await _service.Hr_ChecklistList(inModel, false);
        [Authorize, HttpPost, Route("Hr_ChecklistGet")]
        public async Task<GenericResult<Hr_ChecklistGetOutModel>> Hr_ChecklistGet(GenericRequest<Hr_IdInModel> inModel) => await _service.Hr_ChecklistGet(inModel, false);
        [Authorize, HttpPost, Route("Hr_ChecklistCrea")]
        public async Task<GenericResult<Hr_ChecklistGetOutModel>> Hr_ChecklistCrea(GenericRequest<Hr_ChecklistCreaInModel> inModel) => await _service.Hr_ChecklistCrea(inModel, false);
        [Authorize, HttpPost, Route("Hr_ChecklistPut")]
        public async Task<GenericResult<Hr_ChecklistGetOutModel>> Hr_ChecklistPut(GenericRequest<Hr_ChecklistPutInModel> inModel) => await _service.Hr_ChecklistPut(inModel, false);
        [Authorize, HttpPost, Route("Hr_ChecklistDelete")]
        public async Task<GenericResult<Hr_IdOutModel>> Hr_ChecklistDelete(GenericRequest<Hr_IdInModel> inModel) => await _service.Hr_ChecklistDelete(inModel, false);
        [Authorize, HttpPost, Route("Hr_ChecklistVocePut")]
        public async Task<GenericResult<Hr_ChecklistVoceOutModel>> Hr_ChecklistVocePut(GenericRequest<Hr_ChecklistVocePutInModel> inModel) => await _service.Hr_ChecklistVocePut(inModel, false);
        [Authorize, HttpPost, Route("Hr_ChecklistVocePresaVisione")]
        public async Task<GenericResult<Hr_ChecklistVoceOutModel>> Hr_ChecklistVocePresaVisione(GenericRequest<Hr_IdInModel> inModel) => await _service.Hr_ChecklistVocePresaVisione(inModel, false);

        [Authorize, HttpPost, Route("Hr_ChecklistVoceDocUpload")]
        [RequestSizeLimit(MaxRichiesta), RequestFormLimits(MultipartBodyLengthLimit = MaxRichiesta)]
        public async Task<GenericResult<Hr_ChecklistVoceOutModel>> Hr_ChecklistVoceDocUpload([FromForm] string data, IFormFile file)
        {
            await using var stream = file.OpenReadStream();
            return await _service.Hr_ChecklistVoceDocUpload(HrFile.Request<Hr_VoceDocUploadModel>(data), stream, file.FileName, file.ContentType, file.Length, false);
        }

        [Authorize, HttpGet, Route("Hr_ChecklistVoceDocDownload")]
        public Task<IActionResult> Hr_ChecklistVoceDocDownload([FromQuery] int id) => HrFile.Download(this, () => _service.Hr_ChecklistVoceDocDownload(id));
    }
}
