using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaUserService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_SubCommessaUserController : NvxControllerBase
    {
        private readonly IAz_SubCommessaUserService _service;

        public Az_SubCommessaUserController(
            IHttpContextAccessor httpContextAccessor,
            IAz_SubCommessaUserService service
        ) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_SubCommessaUser_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaUser_GetAll_InModel> inModel)
        {
            var res = await _service.GetAll(inModel, false);
            return res;
        }
    }
}