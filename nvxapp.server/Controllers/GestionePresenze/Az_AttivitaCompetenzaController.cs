using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaCompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaCompetenzaService;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_AttivitaCompetenzaController : NvxControllerBase
    {
        private readonly IAz_AttivitaCompetenzaService _az_AttivitaCompetenzaService;

        public Az_AttivitaCompetenzaController(
            IHttpContextAccessor httpContextAccessor,
            IAz_AttivitaCompetenzaService az_AttivitaCompetenzaService
        ) : base(httpContextAccessor)
        {
            _az_AttivitaCompetenzaService = az_AttivitaCompetenzaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_AttivitaCompetenza_GetAll_OutModel>> GetAll(GenericRequest<Az_AttivitaCompetenza_GetAll_InModel> inModel)
        {
            var res = await _az_AttivitaCompetenzaService.GetAll(inModel, false);
            return res;
        }
    }
}
