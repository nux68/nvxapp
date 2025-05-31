using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediAttivitaService;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_SediAttivitaController : NvxControllerBase
    {
        private readonly IAz_SediAttivitaService _az_SediAttivitaService;

        public Az_SediAttivitaController(
            IHttpContextAccessor httpContextAccessor,
            IAz_SediAttivitaService az_SediAttivitaService
        ) : base(httpContextAccessor)
        {
            _az_SediAttivitaService = az_SediAttivitaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_SediAttivita_GetAll_OutModel>> GetAll(GenericRequest<Az_SediAttivita_GetAll_InModel> inModel)
        {
            var res = await _az_SediAttivitaService.GetAll(inModel, false);
            return res;
        }
    }
}
