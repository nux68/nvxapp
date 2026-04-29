using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.VacationPlanService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.VacationPlanService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    [ApiController]
    [Route("api/[controller]")]
    public class VacationPlanController : NvxControllerBase
    {
        private readonly IVacationPlanService _vacationPlanService;

        public VacationPlanController(
            IHttpContextAccessor httpContextAccessor,
            IVacationPlanService vacationPlanService
        ) : base(httpContextAccessor)
        {
            _vacationPlanService = vacationPlanService;
        }

        [Authorize]
        [HttpPost]
        [Route("VacationPlanGet")]
        public async Task<GenericResult<VacationPlan_GetOutModel>> VacationPlanGet(GenericRequest<VacationPlan_GetInModel> inModel)
        {
            var res = await _vacationPlanService.VacationPlanGet(inModel, false);
            return res;
        }
    }
}
