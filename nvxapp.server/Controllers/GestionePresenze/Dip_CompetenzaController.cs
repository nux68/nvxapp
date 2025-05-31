using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_CompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_CompetenzaService;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_CompetenzaController : NvxControllerBase
    {
        private readonly IDip_CompetenzaService _dip_CompetenzaService;

        public Dip_CompetenzaController(
            IHttpContextAccessor httpContextAccessor,
            IDip_CompetenzaService dip_CompetenzaService
        ) : base(httpContextAccessor)
        {
            _dip_CompetenzaService = dip_CompetenzaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_Competenza_GetAll_OutModel>> GetAll(GenericRequest<Dip_Competenza_GetAll_InModel> inModel)
        {
            var res = await _dip_CompetenzaService.GetAll(inModel, false);
            return res;
        }
    }
}
