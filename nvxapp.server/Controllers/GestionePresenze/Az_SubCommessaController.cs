using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaService;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_SubCommessaController : NvxControllerBase
    {
        private readonly IAz_SubCommessaService _az_SubCommessaService;

        public Az_SubCommessaController(
            IHttpContextAccessor httpContextAccessor,
            IAz_SubCommessaService az_SubCommessaService
        ) : base(httpContextAccessor)
        {
            _az_SubCommessaService = az_SubCommessaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_SubCommessa_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessa_GetAll_InModel> inModel)
        {
            var res = await _az_SubCommessaService.GetAll(inModel, false);
            return res;
        }
    }
}
