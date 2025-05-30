using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CompetenzaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CompetenzaService.Models;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_CompetenzaController : NvxControllerBase
    {
        private readonly IAz_CompetenzaService _az_CompetenzaService;

        public Az_CompetenzaController(
            IHttpContextAccessor httpContextAccessor,
            IAz_CompetenzaService az_CompetenzaService
        ) : base(httpContextAccessor)
        {
            _az_CompetenzaService = az_CompetenzaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_Competenza_GetAll_OutModel>> GetAll(GenericRequest<Az_Competenza_GetAll_InModel> inModel)
        {
            var res = await _az_CompetenzaService.GetAll(inModel, false);
            return res;
        }
    }
}
