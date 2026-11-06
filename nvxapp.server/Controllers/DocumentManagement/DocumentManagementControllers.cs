using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Newtonsoft.Json;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_ConfigService;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_ConfigService.Models;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_DocumentoService;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_DocumentoService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.DocumentManagement
{
    // Configurazione del documentale (impostazioni, tipologie, cartelle, abilitazioni): i permessi li controlla il servizio
    [ApiController]
    [Route("api/[controller]")]
    public class Doc_ConfigController : NvxControllerBase
    {
        private readonly IDoc_ConfigService _service;

        public Doc_ConfigController(IHttpContextAccessor httpContextAccessor, IDoc_ConfigService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize, HttpPost, Route("Doc_SetupGet")]
        public async Task<GenericResult<Doc_SetupGetOutModel>> Doc_SetupGet(GenericRequest<Doc_SetupGetInModel> inModel) => await _service.Doc_SetupGet(inModel, false);

        [Authorize, HttpPost, Route("Doc_CfgPut")]
        public async Task<GenericResult<Doc_CfgPutOutModel>> Doc_CfgPut(GenericRequest<Doc_CfgPutInModel> inModel) => await _service.Doc_CfgPut(inModel, false);

        [Authorize, HttpPost, Route("Doc_TipoPut")]
        public async Task<GenericResult<Doc_TipoPutOutModel>> Doc_TipoPut(GenericRequest<Doc_TipoPutInModel> inModel) => await _service.Doc_TipoPut(inModel, false);

        [Authorize, HttpPost, Route("Doc_TipoDelete")]
        public async Task<GenericResult<Doc_DeleteOutModel>> Doc_TipoDelete(GenericRequest<Doc_DeleteInModel> inModel) => await _service.Doc_TipoDelete(inModel, false);

        [Authorize, HttpPost, Route("Doc_CartellaPut")]
        public async Task<GenericResult<Doc_CartellaPutOutModel>> Doc_CartellaPut(GenericRequest<Doc_CartellaPutInModel> inModel) => await _service.Doc_CartellaPut(inModel, false);

        [Authorize, HttpPost, Route("Doc_CartellaDelete")]
        public async Task<GenericResult<Doc_DeleteOutModel>> Doc_CartellaDelete(GenericRequest<Doc_DeleteInModel> inModel) => await _service.Doc_CartellaDelete(inModel, false);

        [Authorize, HttpPost, Route("Doc_AbilitazioneList")]
        public async Task<GenericResult<Doc_AbilitazioneListOutModel>> Doc_AbilitazioneList(GenericRequest<Doc_AbilitazioneListInModel> inModel) => await _service.Doc_AbilitazioneList(inModel, false);

        [Authorize, HttpPost, Route("Doc_AbilitazionePut")]
        public async Task<GenericResult<Doc_AbilitazionePutOutModel>> Doc_AbilitazionePut(GenericRequest<Doc_AbilitazionePutInModel> inModel) => await _service.Doc_AbilitazionePut(inModel, false);

        [Authorize, HttpPost, Route("Doc_DipendentePut")]
        public async Task<GenericResult<Doc_DipendentePutOutModel>> Doc_DipendentePut(GenericRequest<Doc_DipendentePutInModel> inModel) => await _service.Doc_DipendentePut(inModel, false);
    }

    /*
     Documenti. Il caricamento e' multipart (campo "data" con il JSON dei dati + campo "file"), il download restituisce
     il file in streaming: i documenti arrivano a 50 MB e oltre, il base64 nel JSON li gonfierebbe di un terzo.
     Il limite vero della dimensione e' quello dello spazio dell'azienda (IStorageQuotaService); qui solo un tetto tecnico.
    */
    [ApiController]
    [Route("api/[controller]")]
    public class Doc_DocumentoController : NvxControllerBase
    {
        private const long MaxRichiesta = 512L * 1024 * 1024;

        private readonly IDoc_DocumentoService _service;

        public Doc_DocumentoController(IHttpContextAccessor httpContextAccessor, IDoc_DocumentoService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize, HttpPost, Route("Doc_DocumentoList")]
        public async Task<GenericResult<Doc_DocumentoListOutModel>> Doc_DocumentoList(GenericRequest<Doc_DocumentoListInModel> inModel) => await _service.Doc_DocumentoList(inModel, false);

        [Authorize, HttpPost, Route("Doc_DocumentoGet")]
        public async Task<GenericResult<Doc_DocumentoGetOutModel>> Doc_DocumentoGet(GenericRequest<Doc_DocumentoGetInModel> inModel) => await _service.Doc_DocumentoGet(inModel, false);

        [Authorize, HttpPost, Route("Doc_DocumentoUpload")]
        [RequestSizeLimit(MaxRichiesta), RequestFormLimits(MultipartBodyLengthLimit = MaxRichiesta)]
        public async Task<GenericResult<Doc_DocumentoPutOutModel>> Doc_DocumentoUpload([FromForm] string data, IFormFile file)
        {
            var request = new GenericRequest<Doc_DocumentoEditModel> { Data = JsonConvert.DeserializeObject<Doc_DocumentoEditModel>(data) ?? new Doc_DocumentoEditModel() };
            await using var stream = file.OpenReadStream();
            return await _service.Doc_DocumentoUpload(request, stream, file.FileName, file.ContentType, false);
        }

        // invio multiplo: data = { documento: {...}, idDip_Anagrafica: [..] }
        [Authorize, HttpPost, Route("Doc_DocumentoUploadMultiplo")]
        [RequestSizeLimit(MaxRichiesta), RequestFormLimits(MultipartBodyLengthLimit = MaxRichiesta)]
        public async Task<GenericResult<Doc_DocumentoUploadMultiploOutModel>> Doc_DocumentoUploadMultiplo([FromForm] string data, IFormFile file)
        {
            var request = new GenericRequest<Doc_DocumentoUploadMultiploInModel> { Data = JsonConvert.DeserializeObject<Doc_DocumentoUploadMultiploInModel>(data) ?? new Doc_DocumentoUploadMultiploInModel() };
            await using var stream = file.OpenReadStream();
            return await _service.Doc_DocumentoUploadMultiplo(request, stream, file.FileName, file.ContentType, false);
        }

        [Authorize, HttpPost, Route("Doc_DocumentoNewVersion")]
        [RequestSizeLimit(MaxRichiesta), RequestFormLimits(MultipartBodyLengthLimit = MaxRichiesta)]
        public async Task<GenericResult<Doc_DocumentoPutOutModel>> Doc_DocumentoNewVersion([FromForm] int id, IFormFile file)
        {
            var request = new GenericRequest<Doc_DocumentoIdInModel> { Data = new Doc_DocumentoIdInModel { Id = id } };
            await using var stream = file.OpenReadStream();
            return await _service.Doc_DocumentoNewVersion(request, stream, file.FileName, file.ContentType, false);
        }

        [Authorize, HttpPost, Route("Doc_DocumentoPut")]
        public async Task<GenericResult<Doc_DocumentoPutOutModel>> Doc_DocumentoPut(GenericRequest<Doc_DocumentoPutInModel> inModel) => await _service.Doc_DocumentoPut(inModel, false);

        [Authorize, HttpPost, Route("Doc_DocumentoCestino")]
        public async Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoCestino(GenericRequest<Doc_DocumentoIdInModel> inModel) => await _service.Doc_DocumentoCestino(inModel, false);

        [Authorize, HttpPost, Route("Doc_DocumentoRipristina")]
        public async Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoRipristina(GenericRequest<Doc_DocumentoIdInModel> inModel) => await _service.Doc_DocumentoRipristina(inModel, false);

        [Authorize, HttpPost, Route("Doc_DocumentoElimina")]
        public async Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoElimina(GenericRequest<Doc_DocumentoIdInModel> inModel) => await _service.Doc_DocumentoElimina(inModel, false);

        [Authorize, HttpPost, Route("Doc_SvuotaCestino")]
        public async Task<GenericResult<Doc_SvuotaCestinoOutModel>> Doc_SvuotaCestino(GenericRequest<Doc_SvuotaCestinoInModel> inModel) => await _service.Doc_SvuotaCestino(inModel, false);

        [Authorize, HttpPost, Route("Doc_DocumentoPresaVisione")]
        public async Task<GenericResult<Doc_DocumentoIdOutModel>> Doc_DocumentoPresaVisione(GenericRequest<Doc_DocumentoIdInModel> inModel) => await _service.Doc_DocumentoPresaVisione(inModel, false);

        [Authorize, HttpPost, Route("Doc_LogList")]
        public async Task<GenericResult<Doc_LogListOutModel>> Doc_LogList(GenericRequest<Doc_LogListInModel> inModel) => await _service.Doc_LogList(inModel, false);

        // file della versione (corrente se non indicata); errore: 404 con il messaggio nella lingua dell'utente
        [Authorize, HttpGet, Route("Doc_DocumentoDownload")]
        public async Task<IActionResult> Doc_DocumentoDownload([FromQuery] int id, [FromQuery] int? versione)
        {
            try
            {
                var file = await _service.Doc_DocumentoDownload(id, versione);
                return File(file.Content, file.ContentType, file.NomeFile);
            }
            catch (UserMessageException ex)
            {
                return NotFound(ex.Message);
            }
            catch (FileNotFoundException)
            {
                return NotFound(DocumentManagementMessages.Get("FileMissing"));
            }
        }
    }
}
