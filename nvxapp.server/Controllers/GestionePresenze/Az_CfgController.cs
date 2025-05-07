using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CfgService;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_CfgController : NvxControllerBase
    {
        private readonly IAz_CfgService _az_CfgService;

        public Az_CfgController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IAz_CfgService az_CfgService
        ) : base(httpContextAccessor)
        {
            _az_CfgService = az_CfgService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_Cfg_GetAll_OutModel>> GetAll(GenericRequest<Az_Cfg_GetAll_InModel> inModel)
        {
            var res = await _az_CfgService.GetAll(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Az_Cfg_Get")]
        public async Task<GenericResult<Az_Cfg_Get_OutModel>> Az_Cfg_Get(GenericRequest<Az_Cfg_Get_InModel> inModel)
        {
            var res = await _az_CfgService.Az_CfgGet(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Az_Cfg_Put")]
        public async Task<GenericResult<Az_Cfg_Put_OutModel>> Az_Cfg_Put(GenericRequest<Az_Cfg_Put_InModel> inModel)
        {
            var res = await _az_CfgService.Az_CfgPut(inModel, false);

            return res;
        }


    }



}
