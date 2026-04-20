using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Account.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.Helpers;


namespace nvxapp.server.Controllers.Infrastructure
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




        [Authorize]
        [HttpPost]
        [Route("UserList")]
        public async Task<GenericResult<UserListOutModel>> UserList(GenericRequest<UserListInModel> inModel)
        {
            var res = await _accountService.UserList(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserGet")]
        public async Task<GenericResult<UserGetOutModel>> UserGet(GenericRequest<UserGetInModel> inModel)
        {
            var res = await _accountService.UserGet(inModel, false);

            return res;
        }
        [Authorize]
        [HttpPost]
        [Route("UserPut")]
        public async Task<GenericResult<UserPutOutModel>> UserPut(GenericRequest<UserPutInModel> inModel)
        {
            var res = await _accountService.UserPut(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("UserDealerList")]
        public async Task<GenericResult<UserDealerListOutModel>> UserDealerList(GenericRequest<UserDealerListInModel> inModel)
        {
            var res = await _accountService.UserDealerList(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserDealerGet")]
        public async Task<GenericResult<UserDealerGetOutModel>> UserDealerGet(GenericRequest<UserDealerGetInModel> inModel)
        {
            var res = await _accountService.UserDealerGet(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("UserDealerPut")]
        public async Task<GenericResult<UserDealerPutOutModel>> UserDealerPut(GenericRequest<UserDealerPutInModel> inModel)
        {
            var res = await _accountService.UserDealerPut(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("UserFinancialAdvisorList")]
        public async Task<GenericResult<UserFinancialAdvisorListOutModel>> UserFinancialAdvisorList(GenericRequest<UserFinancialAdvisorListInModel> inModel)
        {
            var res = await _accountService.UserFinancialAdvisorList(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("UserFinancialAdvisorGet")]
        public async Task<GenericResult<UserFinancialAdvisorGetOutModel>> UserFinancialAdvisorGet(GenericRequest<UserFinancialAdvisorGetInModel> inModel)
        {
            var res = await _accountService.UserFinancialAdvisorGet(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("UserFinancialAdvisorPut")]
        public async Task<GenericResult<UserFinancialAdvisorPutOutModel>> UserFinancialAdvisorPut(GenericRequest<UserFinancialAdvisorPutInModel> inModel)
        {
            var res = await _accountService.UserFinancialAdvisorPut(inModel, false);

            return res;
        }

        [Authorize]
        [HttpGet]
        [Route("Download/{fileName}")]
        public IActionResult Download(string fileName)
        {
            var filePath = Path.Combine(NVXSystem.ExportFolder, fileName);
            if (!System.IO.File.Exists(filePath))
                return NotFound(new { message = "File non trovato." });

            var contentType = fileName.EndsWith(".csv", StringComparison.OrdinalIgnoreCase)
                ? "text/csv"
                : "text/plain";

            return PhysicalFile(filePath, contentType, fileName);
        }
    }



}
