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
    }
}
