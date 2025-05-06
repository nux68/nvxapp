using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoUserService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_SediRepartoUserController : NvxControllerBase
    {
        private readonly IAz_SediRepartoUserService _az_SediRepartoUserService;

        public Az_SediRepartoUserController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IAz_SediRepartoUserService az_SediRepartoUserService
        ) : base(httpContextAccessor)
        {
            _az_SediRepartoUserService = az_SediRepartoUserService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_SediRepartoUser_GetAll_OutModel>> GetAll(GenericRequest<Az_SediRepartoUser_GetAll_InModel> inModel)
        {
            var res = await _az_SediRepartoUserService.GetAll(inModel, false);

            return res;
        }

        

    }



}
