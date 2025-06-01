using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaCompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaCompetenzaService;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_AttivitaCompetenzaController : NvxControllerBase
    {
        private readonly IPar_AttivitaCompetenzaService _par_AttivitaCompetenzaService;

        public Par_AttivitaCompetenzaController(
            IHttpContextAccessor httpContextAccessor,
            IPar_AttivitaCompetenzaService par_AttivitaCompetenzaService
        ) : base(httpContextAccessor)
        {
            _par_AttivitaCompetenzaService = par_AttivitaCompetenzaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_AttivitaCompetenza_GetAll_OutModel>> GetAll(GenericRequest<Par_AttivitaCompetenza_GetAll_InModel> inModel)
        {
            var res = await _par_AttivitaCompetenzaService.GetAll(inModel, false);
            return res;
        }
    }
}
