using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaSediRepartoService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaSediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_SubCommessaSediRepartoController : NvxControllerBase
    {
        private readonly IAz_SubCommessaSediRepartoService _service;

        public Az_SubCommessaSediRepartoController(
            IHttpContextAccessor httpContextAccessor,
            IAz_SubCommessaSediRepartoService service
        ) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_SubCommessaSediReparto_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaSediReparto_GetAll_InModel> inModel)
        {
            var res = await _service.GetAll(inModel, false);
            return res;
        }
    }
}
