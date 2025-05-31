using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AttivitaService.Models;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_AttivitaController : NvxControllerBase
    {
        private readonly IAz_AttivitaService _az_AttivitaService;

        public Az_AttivitaController(
            IHttpContextAccessor httpContextAccessor,
            IAz_AttivitaService az_AttivitaService
        ) : base(httpContextAccessor)
        {
            _az_AttivitaService = az_AttivitaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_Attivita_GetAll_OutModel>> GetAll(GenericRequest<Az_Attivita_GetAll_InModel> inModel)
        {
            var res = await _az_AttivitaService.GetAll(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Az_AttivitaGet")]
        public async Task<GenericResult<Az_AttivitaGetOutModel>> Az_AttivitaGet(GenericRequest<Az_AttivitaGetInModel> inModel)
        {
            var res = await _az_AttivitaService.Az_AttivitaGet(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Az_AttivitaPut")]
        public async Task<GenericResult<Az_AttivitaPutOutModel>> Az_AttivitaPut(GenericRequest<Az_AttivitaPutInModel> inModel)
        {
            var res = await _az_AttivitaService.Az_AttivitaPut(inModel, false);
            return res;
        }
    }
}
