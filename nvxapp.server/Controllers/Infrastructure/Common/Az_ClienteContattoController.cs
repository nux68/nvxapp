using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.Az_ClienteContattoService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.Az_ClienteContattoService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.Infrastructure
{
    // tabella comune dell'azienda (schema Common): disponibile con qualsiasi applicativo attivo
    [ApiController]
    [Route("api/[controller]")]
    public class Az_ClienteContattoController : NvxControllerBase
    {
        private readonly IAz_ClienteContattoService _service;

        public Az_ClienteContattoController(IHttpContextAccessor httpContextAccessor, IAz_ClienteContattoService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_ClienteContatto_GetAll_OutModel>> GetAll(GenericRequest<Az_ClienteContatto_GetAll_InModel> inModel)
            => await _service.GetAll(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Az_ClienteContattoGet")]
        public async Task<GenericResult<Az_ClienteContattoGetOutModel>> Az_ClienteContattoGet(GenericRequest<Az_ClienteContattoGetInModel> inModel)
            => await _service.Az_ClienteContattoGet(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Az_ClienteContattoPut")]
        public async Task<GenericResult<Az_ClienteContattoPutOutModel>> Az_ClienteContattoPut(GenericRequest<Az_ClienteContattoPutInModel> inModel)
            => await _service.Az_ClienteContattoPut(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Az_ClienteContattoDelete")]
        public async Task<GenericResult<Az_ClienteContattoDeleteOutModel>> Az_ClienteContattoDelete(GenericRequest<Az_ClienteContattoDeleteInModel> inModel)
            => await _service.Az_ClienteContattoDelete(inModel, false);
    }
}
