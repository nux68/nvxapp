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
        [Route("DealerGet")]
        public async Task<GenericResult<DealerGetOutModel>> DealerGet(GenericRequest<DealerGetInModel> inModel)
        {
            var res = await _accountService.DealerGet(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("DealerPut")]
        public async Task<GenericResult<DealerPutOutModel>> DealerPut(GenericRequest<DealerPutInModel> inModel)
        {
            var res = await _accountService.DealerPut(inModel, false);

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
        [Route("FinancialAdvisorGet")]
        public async Task<GenericResult<FinancialAdvisorGetOutModel>> FinancialAdvisorGet(GenericRequest<FinancialAdvisorGetInModel> inModel)
        {
            var res = await _accountService.FinancialAdvisorGet(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("FinancialAdvisorPut")]
        public async Task<GenericResult<FinancialAdvisorPutOutModel>> FinancialAdvisorPut(GenericRequest<FinancialAdvisorPutInModel> inModel)
        {
            var res = await _accountService.FinancialAdvisorPut(inModel, false);

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
        [Route("CompanyGet")]
        public async Task<GenericResult<CompanyGetOutModel>> CompanyGet(GenericRequest<CompanyGetInModel> inModel)
        {
            var res = await _accountService.CompanyGet(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("CompanyPut")]
        public async Task<GenericResult<CompanyPutOutModel>> CompanyPut(GenericRequest<CompanyPutInModel> inModel)
        {
            var res = await _accountService.CompanyPut(inModel, false);

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

        [Authorize]
        [HttpPost]
        [Route("UserCompanyGet")]
        public async Task<GenericResult<UserCompanyGetOutModel>> UserCompanyGet(GenericRequest<UserCompanyGetInModel> inModel)
        {
            var res = await _accountService.UserCompanyGet(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("UserCompanyPut")]
        public async Task<GenericResult<UserCompanyPutOutModel>> UserCompanyPut(GenericRequest<UserCompanyPutInModel> inModel)
        {
            var res = await _accountService.UserCompanyPut(inModel, false);

            return res;
        }


    }



}
