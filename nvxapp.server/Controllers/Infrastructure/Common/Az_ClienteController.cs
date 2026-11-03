using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.Az_ClienteService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.Az_ClienteService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.Infrastructure
{
    // tabella comune dell'azienda (schema Common): disponibile con qualsiasi applicativo attivo
    [ApiController]
    [Route("api/[controller]")]
    public class Az_ClienteController : NvxControllerBase
    {
        private readonly IAz_ClienteService _service;

        public Az_ClienteController(IHttpContextAccessor httpContextAccessor, IAz_ClienteService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_Cliente_GetAll_OutModel>> GetAll(GenericRequest<Az_Cliente_GetAll_InModel> inModel)
            => await _service.GetAll(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Az_ClienteGet")]
        public async Task<GenericResult<Az_ClienteGetOutModel>> Az_ClienteGet(GenericRequest<Az_ClienteGetInModel> inModel)
            => await _service.Az_ClienteGet(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Az_ClientePut")]
        public async Task<GenericResult<Az_ClientePutOutModel>> Az_ClientePut(GenericRequest<Az_ClientePutInModel> inModel)
            => await _service.Az_ClientePut(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Az_ClienteDelete")]
        public async Task<GenericResult<Az_ClienteDeleteOutModel>> Az_ClienteDelete(GenericRequest<Az_ClienteDeleteInModel> inModel)
            => await _service.Az_ClienteDelete(inModel, false);
    }
}
