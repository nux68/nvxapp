using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_SubCommessaAttivitaController : NvxControllerBase
    {
        private readonly IAz_SubCommessaAttivitaService _az_SubCommessaAttivitaService;

        public Az_SubCommessaAttivitaController(
            IHttpContextAccessor httpContextAccessor,
            IAz_SubCommessaAttivitaService az_SubCommessaAttivitaService
        ) : base(httpContextAccessor)
        {
            _az_SubCommessaAttivitaService = az_SubCommessaAttivitaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_SubCommessaAttivita_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaAttivita_GetAll_InModel> inModel)
        {
            var res = await _az_SubCommessaAttivitaService.GetAll(inModel, false);
            return res;
        }
    }
}
