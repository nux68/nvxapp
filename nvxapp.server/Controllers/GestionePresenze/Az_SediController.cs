using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediService;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_SediController : NvxControllerBase
    {
        private readonly IAz_SediService _az_SediService;

        public Az_SediController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IAz_SediService az_SediService
        ) : base(httpContextAccessor)
        {
            _az_SediService = az_SediService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_Sedi_GetAll_OutModel>> GetAll(GenericRequest<Az_Sedi_GetAll_InModel> inModel)
        {
            var res = await _az_SediService.GetAll(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("AzSediGet")]
        public async Task<GenericResult<Az_SediGetOutModel>> AzSediGet(GenericRequest<Az_SediGetInModel> inModel)
        {
            var res = await _az_SediService.AzSediGet(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("AzSediPut")]
        public async Task<GenericResult<Az_SediPutOutModel>> AzSediPut(GenericRequest<Az_SediPutInModel> inModel)
        {
            var res = await _az_SediService.AzSediPut(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("AzSediDelete")]
        public async Task<GenericResult<Az_SediDeleteOutModel>> AzSediDelete(GenericRequest<Az_SediDeleteInModel> inModel)
        {
            var res = await _az_SediService.AzSediDelete(inModel, false);
            return res;
        }
    }
}
