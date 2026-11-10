using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.UserDashboard;
using nvxapp.server.service.ClientServer_Service.Infrastructure.UserDashboard.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Widget della dashboard dell'utente corrente (ANALISI_WIDGET.md)
    [ApiController]
    [Route("api/[controller]")]
    public class UserDashboardController : NvxControllerBase
    {
        private readonly IUserDashboardService _userDashboardService;

        public UserDashboardController(IHttpContextAccessor httpContextAccessor,
                                       IUserDashboardService userDashboardService) : base(httpContextAccessor)
        {
            _userDashboardService = userDashboardService;
        }

        [Authorize]
        [HttpPost]
        [Route("UserDashboardGet")]
        public async Task<GenericResult<UserDashboardGetOutModel>> UserDashboardGet(GenericRequest<UserDashboardGetInModel> inModel)
        {
            return await _userDashboardService.UserDashboardGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("UserDashboardPut")]
        public async Task<GenericResult<UserDashboardPutOutModel>> UserDashboardPut(GenericRequest<UserDashboardPutInModel> inModel)
        {
            return await _userDashboardService.UserDashboardPut(inModel, false);
        }
    }
}
