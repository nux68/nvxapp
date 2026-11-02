using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deletion.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Cancellazione fisica di centri, studi, aziende e utenti: anteprima ed esecuzione (permessi controllati nel servizio).
    [ApiController]
    [Route("api/[controller]")]
    public class DeletionController : NvxControllerBase
    {
        private readonly IDeletionService _deletionService;

        public DeletionController(
                                 IHttpContextAccessor httpContextAccessor,
                                 IDeletionService deletionService
          ) : base(httpContextAccessor)
        {
            _deletionService = deletionService;
        }

        [Authorize]
        [HttpPost]
        [Route("DeletionPreview")]
        public async Task<GenericResult<DeletionPreviewOutModel>> DeletionPreview(GenericRequest<DeletionPreviewInModel> inModel)
        {
            return await _deletionService.DeletionPreview(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DeletionExecute")]
        public async Task<GenericResult<DeletionExecuteOutModel>> DeletionExecute(GenericRequest<DeletionExecuteInModel> inModel)
        {
            return await _deletionService.DeletionExecute(inModel, false);
        }
    }
}
