using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CompetenzaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CompetenzaService.Models;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_CompetenzaController : NvxControllerBase
    {
        private readonly IPar_CompetenzaService _par_CompetenzaService;

        public Par_CompetenzaController(
            IHttpContextAccessor httpContextAccessor,
            IPar_CompetenzaService par_CompetenzaService
        ) : base(httpContextAccessor)
        {
            _par_CompetenzaService = par_CompetenzaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_Competenza_GetAll_OutModel>> GetAll(GenericRequest<Par_Competenza_GetAll_InModel> inModel)
        {
            var res = await _par_CompetenzaService.GetAll(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Par_CompetenzaGet")]
        public async Task<GenericResult<Par_CompetenzaGetOutModel>> Par_CompetenzaGet(GenericRequest<Par_CompetenzaGetInModel> inModel)
        {
            var res = await _par_CompetenzaService.Par_CompetenzaGet(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Par_CompetenzaPut")]
        public async Task<GenericResult<Par_CompetenzaPutOutModel>> Par_CompetenzaPut(GenericRequest<Par_CompetenzaPutInModel> inModel)
        {
            var res = await _par_CompetenzaService.Par_CompetenzaPut(inModel, false);
            return res;
        }
    }
}
