using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_ClienteService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_ClienteService;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_ClienteController : NvxControllerBase
    {
        private readonly IAz_ClienteService _az_ClienteService;

        public Az_ClienteController(
            IHttpContextAccessor httpContextAccessor,
            IAz_ClienteService az_ClienteService
        ) : base(httpContextAccessor)
        {
            _az_ClienteService = az_ClienteService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_Cliente_GetAll_OutModel>> GetAll(GenericRequest<Az_Cliente_GetAll_InModel> inModel)
        {
            var res = await _az_ClienteService.GetAll(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Az_ClienteGet")]
        public async Task<GenericResult<Az_ClienteGetOutModel>> Az_ClienteGet(GenericRequest<Az_ClienteGetInModel> inModel)
        {
            var res = await _az_ClienteService.Az_ClienteGet(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Az_ClientePut")]
        public async Task<GenericResult<Az_ClientePutOutModel>> Az_ClientePut(GenericRequest<Az_ClientePutInModel> inModel)
        {
            var res = await _az_ClienteService.Az_ClientePut(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Az_ClienteDelete")]
        public async Task<GenericResult<Az_ClienteDeleteOutModel>> Az_ClienteDelete(GenericRequest<Az_ClienteDeleteInModel> inModel)
        {
            var res = await _az_ClienteService.Az_ClienteDelete(inModel, false);
            return res;
        }
    }
}
