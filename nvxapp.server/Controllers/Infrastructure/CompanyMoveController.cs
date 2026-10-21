using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyMove;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyMove.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Spostamento di un'azienda sotto un altro studio: riservato al SuperUser (controllo nel servizio).
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyMoveController : NvxControllerBase
    {
        private readonly ICompanyMoveService _companyMoveService;

        public CompanyMoveController(
                                    IHttpContextAccessor httpContextAccessor,
                                    ICompanyMoveService companyMoveService
          ) : base(httpContextAccessor)
        {
            _companyMoveService = companyMoveService;
        }

        [Authorize]
        [HttpPost]
        [Route("CompanyMoveTree")]
        public async Task<GenericResult<CompanyMoveTreeOutModel>> CompanyMoveTree(GenericRequest<CompanyMoveTreeInModel> inModel)
        {
            return await _companyMoveService.CompanyMoveTree(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("CompanyMovePut")]
        public async Task<GenericResult<CompanyMovePutOutModel>> CompanyMovePut(GenericRequest<CompanyMovePutInModel> inModel)
        {
            return await _companyMoveService.CompanyMovePut(inModel, false);
        }
    }
}
