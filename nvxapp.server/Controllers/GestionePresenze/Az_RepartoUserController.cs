using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_RepartoUserService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_RepartoUserService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_RepartoUserController : NvxControllerBase
    {
        private readonly IAz_RepartoUserService _az_RepartoUserService;

        public Az_RepartoUserController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IAz_RepartoUserService az_RepartoUserService
        ) : base(httpContextAccessor)
        {
            _az_RepartoUserService = az_RepartoUserService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_RepartoUser_GetAll_OutModel>> GetAll(GenericRequest<Az_RepartoUser_GetAll_InModel> inModel)
        {
            var res = await _az_RepartoUserService.GetAll(inModel, false);

            return res;
        }

        

    }



}
