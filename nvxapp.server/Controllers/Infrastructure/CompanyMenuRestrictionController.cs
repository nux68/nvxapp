using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyMenuRestriction;
using nvxapp.server.service.ClientServer_Service.Infrastructure.CompanyMenuRestriction.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Voci di menu nascoste ai ruoli CompanyAdmin e User dell'azienda corrente.
    // Lettura e salvataggio riservati al CompanyPowerAdmin (controllo nel servizio).
    [ApiController]
    [Route("api/[controller]")]
    public class CompanyMenuRestrictionController : NvxControllerBase
    {
        private readonly ICompanyMenuRestrictionService _companyMenuRestrictionService;

        public CompanyMenuRestrictionController(
                                    IHttpContextAccessor httpContextAccessor,
                                    ICompanyMenuRestrictionService companyMenuRestrictionService
          ) : base(httpContextAccessor)
        {
            _companyMenuRestrictionService = companyMenuRestrictionService;
        }

        [Authorize]
        [HttpPost]
        [Route("CompanyMenuRestrictionList")]
        public async Task<GenericResult<CompanyMenuRestrictionListOutModel>> CompanyMenuRestrictionList(GenericRequest<CompanyMenuRestrictionListInModel> inModel)
        {
            return await _companyMenuRestrictionService.CompanyMenuRestrictionList(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("CompanyMenuRestrictionPut")]
        public async Task<GenericResult<CompanyMenuRestrictionPutOutModel>> CompanyMenuRestrictionPut(GenericRequest<CompanyMenuRestrictionPutInModel> inModel)
        {
            return await _companyMenuRestrictionService.CompanyMenuRestrictionPut(inModel, false);
        }
    }
}
