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

        

    }



}
