using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Documentation;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Documentation.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Documenti dei livelli (es. documentazione per le risposte AI ai ticket). Livello e permessi li decide il servizio.
    [ApiController]
    [Route("api/[controller]")]
    public class DocumentController : NvxControllerBase
    {
        private readonly IDocumentService _documentService;

        public DocumentController(
                                  IHttpContextAccessor httpContextAccessor,
                                  IDocumentService documentService
          ) : base(httpContextAccessor)
        {
            _documentService = documentService;
        }

        [Authorize]
        [HttpPost]
        [Route("DocumentScope")]
        public async Task<GenericResult<DocumentScopeOutModel>> DocumentScope(GenericRequest<DocumentScopeInModel> inModel)
        {
            return await _documentService.DocumentScope(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DocumentList")]
        public async Task<GenericResult<DocumentListOutModel>> DocumentList(GenericRequest<DocumentListInModel> inModel)
        {
            return await _documentService.DocumentList(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DocumentGet")]
        public async Task<GenericResult<DocumentGetOutModel>> DocumentGet(GenericRequest<DocumentGetInModel> inModel)
        {
            return await _documentService.DocumentGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DocumentPut")]
        public async Task<GenericResult<DocumentPutOutModel>> DocumentPut(GenericRequest<DocumentPutInModel> inModel)
        {
            return await _documentService.DocumentPut(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DocumentDelete")]
        public async Task<GenericResult<DocumentDeleteOutModel>> DocumentDelete(GenericRequest<DocumentDeleteInModel> inModel)
        {
            return await _documentService.DocumentDelete(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DocumentFileText")]
        public async Task<GenericResult<DocumentFileTextOutModel>> DocumentFileText(GenericRequest<DocumentFileTextInModel> inModel)
        {
            return await _documentService.DocumentFileText(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DocumentFileGet")]
        public async Task<GenericResult<DocumentFileGetOutModel>> DocumentFileGet(GenericRequest<DocumentFileGetInModel> inModel)
        {
            return await _documentService.DocumentFileGet(inModel, false);
        }
    }
}
