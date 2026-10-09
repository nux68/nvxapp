using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyApplication.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Attivazione degli applicativi per azienda.
    // NB: come gli altri controller e' protetto solo da [Authorize]; il controllo dei ruoli
    // lato server (riservare l'attivazione a studi/amministratori) e' previsto in piano-security.md
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyApplicationController : NvxControllerBase
    {
        private readonly ICompanyApplicationService _companyApplicationService;

        public CompanyApplicationController(
                                    IHttpContextAccessor httpContextAccessor,
                                    ICompanyApplicationService companyApplicationService
          ) : base(httpContextAccessor)
        {
            _companyApplicationService = companyApplicationService;
        }

        [Authorize]
        [HttpPost]
        [Route("CompanyApplicationList")]
        public async Task<GenericResult<CompanyApplicationListOutModel>> CompanyApplicationList(GenericRequest<CompanyApplicationListInModel> inModel)
        {
            return await _companyApplicationService.CompanyApplicationList(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("CompanyApplicationPut")]
        public async Task<GenericResult<CompanyApplicationPutOutModel>> CompanyApplicationPut(GenericRequest<CompanyApplicationPutInModel> inModel)
        {
            return await _companyApplicationService.CompanyApplicationPut(inModel, false);
        }
    }
}
