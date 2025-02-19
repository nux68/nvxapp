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

        [Authorize]
        [HttpPost]
        [Route("DealerList")]
        public async Task<GenericResult<DealerListOutModel>> DealerList(GenericRequest<DealerListInModel> inModel)
        {
            var res = await _accountService.DealerList(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("FinancialAdvisorList")]
        public async Task<GenericResult<FinancialAdvisorListOutModel>> FinancialAdvisorList(GenericRequest<FinancialAdvisorListInModel> inModel)
        {
            var res = await _accountService.FinancialAdvisorList(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("CompanyList")]
        public async Task<GenericResult<CompanyListOutModel>> CompanyList(GenericRequest<CompanyListInModel> inModel)
        {
            var res = await _accountService.CompanyList(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserCompanyList")]
        public async Task<GenericResult<UserCompanyListOutModel>> UserCompanyList(GenericRequest<UserCompanyListInModel> inModel)
        {
            var res = await _accountService.UserCompanyList(inModel, false);

            return res;
        }

    }



}
