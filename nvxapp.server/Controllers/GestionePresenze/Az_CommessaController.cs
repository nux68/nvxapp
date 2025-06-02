using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_CommessaController : NvxControllerBase
    {
        private readonly IAz_CommessaService _az_CommessaService;

        public Az_CommessaController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IAz_CommessaService az_CommessaService
        ) : base(httpContextAccessor)
        {
            _az_CommessaService = az_CommessaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_Commessa_GetAll_OutModel>> GetAll(GenericRequest<Az_Commessa_GetAll_InModel> inModel)
        {
            var res = await _az_CommessaService.GetAll(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("AZ_CommessaGet")]
        public async Task<GenericResult<Az_CommessaGetOutModel>> AZ_CommessaGet(GenericRequest<Az_CommessaGetInModel> inModel)
        {
            var res = await _az_CommessaService.AZ_CommessaGet(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("AZ_CommessaPut")]
        public async Task<GenericResult<Az_CommessaPutOutModel>> AZ_CommessaPut(GenericRequest<Az_CommessaPutInModel> inModel)
        {
            var res = await _az_CommessaService.AZ_CommessaPut(inModel, false);
            return res;
        }
    }
}
