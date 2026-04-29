using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.PresentStaffService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.PresentStaffService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    [ApiController]
    [Route("api/[controller]")]
    public class PresentStaffController : NvxControllerBase
    {
        private readonly IPresentStaffService _presentStaffService;

        public PresentStaffController(
            IHttpContextAccessor httpContextAccessor,
            IPresentStaffService presentStaffService
        ) : base(httpContextAccessor)
        {
            _presentStaffService = presentStaffService;
        }

        [Authorize]
        [HttpPost]
        [Route("PresentStaffGet")]
        public async Task<GenericResult<PresentStaff_GetOutModel>> PresentStaffGet(GenericRequest<PresentStaff_GetInModel> inModel)
        {
            var res = await _presentStaffService.PresentStaffGet(inModel, false);
            return res;
        }
    }
}
