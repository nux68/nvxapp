using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_DocsService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCau_DocsService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    // documentazione del modello di export (file per l'AI): elenco, contenuto, caricamento, eliminazione
    [ApiController]
    [Route("api/[controller]")]
    public class Par_ExportCau_DocsController : NvxControllerBase
    {
        private readonly IPar_ExportCau_DocsService _par_ExportCau_DocsService;

        public Par_ExportCau_DocsController(
            IHttpContextAccessor httpContextAccessor,
            IPar_ExportCau_DocsService par_ExportCau_DocsService
        ) : base(httpContextAccessor)
        {
            _par_ExportCau_DocsService = par_ExportCau_DocsService;
        }

        [Authorize]
        [HttpPost]
        [Route("Par_ExportCau_Docs_GetAll")]
        public async Task<GenericResult<Par_ExportCau_Docs_GetAll_OutModel>> Par_ExportCau_Docs_GetAll(GenericRequest<Par_ExportCau_Docs_GetAll_InModel> inModel)
        {
            return await _par_ExportCau_DocsService.Par_ExportCau_Docs_GetAll(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_ExportCau_Docs_Get")]
        public async Task<GenericResult<Par_ExportCau_Docs_Get_OutModel>> Par_ExportCau_Docs_Get(GenericRequest<Par_ExportCau_Docs_Get_InModel> inModel)
        {
            return await _par_ExportCau_DocsService.Par_ExportCau_Docs_Get(inModel, false);
        }

        // il file arriva in base64 (5 MB al massimo)
        [Authorize]
        [HttpPost]
        [Route("Par_ExportCau_Docs_Put")]
        [RequestSizeLimit(10 * 1024 * 1024)]
        public async Task<GenericResult<Par_ExportCau_Docs_Put_OutModel>> Par_ExportCau_Docs_Put(GenericRequest<Par_ExportCau_Docs_Put_InModel> inModel)
        {
            return await _par_ExportCau_DocsService.Par_ExportCau_Docs_Put(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_ExportCau_Docs_Delete")]
        public async Task<GenericResult<Par_ExportCau_Docs_Delete_OutModel>> Par_ExportCau_Docs_Delete(GenericRequest<Par_ExportCau_Docs_Delete_InModel> inModel)
        {
            return await _par_ExportCau_DocsService.Par_ExportCau_Docs_Delete(inModel, false);
        }
    }
}
