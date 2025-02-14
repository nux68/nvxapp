using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Account;
using nvxapp.server.service.ClientServer_Service.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class AccountController : NvxControllerBase
    {
        private readonly IAccountService _accountService;

        public AccountController(
                                    IHttpContextAccessor httpContextAccessor,
                                    IAccountService accountService
          ) : base(httpContextAccessor)
        {
            _accountService = accountService;
        }



        [HttpPost]
        [Route("Login")]
        public async Task<GenericResult<LoginOutModel>> Login(GenericRequest<LoginInModel> inModel)
        {
            var res = await _accountService.Login(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserRoles")]
        public async Task<GenericResult<UserRolesOutModel>> UserRoles(GenericRequest<UserRolesInModel> inModel)
        {
            var res = await _accountService.UserRoles(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserLoad")]
        public async Task<GenericResult<UserLoadOutModel>> UserLoad(GenericRequest<UserLoadInModel> inModel)
        {
            var res = await _accountService.UserLoad(inModel, false);

            return res;
        }

    }



}
