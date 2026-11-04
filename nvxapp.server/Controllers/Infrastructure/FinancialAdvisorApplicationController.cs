using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.FinancialAdvisorApplication;
using nvxapp.server.service.ClientServer_Service.Infrastructure.FinancialAdvisorApplication.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Abilitazione degli applicativi per studio (funzioni lato studio).
    // Il cambio di stato e' riservato al SuperUser e al DealerPowerAdmin del centro (controllo nel servizio).
    [ApiController]
    [Route("api/[controller]")]
    public class FinancialAdvisorApplicationController : NvxControllerBase
    {
        private readonly IFinancialAdvisorApplicationService _service;

        public FinancialAdvisorApplicationController(IHttpContextAccessor httpContextAccessor,
                                                     IFinancialAdvisorApplicationService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("FinancialAdvisorApplicationList")]
        public async Task<GenericResult<FinancialAdvisorApplicationListOutModel>> FinancialAdvisorApplicationList(GenericRequest<FinancialAdvisorApplicationListInModel> inModel)
            => await _service.FinancialAdvisorApplicationList(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("FinancialAdvisorApplicationPut")]
        public async Task<GenericResult<FinancialAdvisorApplicationPutOutModel>> FinancialAdvisorApplicationPut(GenericRequest<FinancialAdvisorApplicationPutInModel> inModel)
            => await _service.FinancialAdvisorApplicationPut(inModel, false);
    }
}
