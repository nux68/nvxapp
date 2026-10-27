using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoAutovettureService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoAutovettureService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers
{
    // le autovetture si modificano dall'anagrafica dipendente (Dip_AnagraficaPut); qui solo la lettura per la nota spese
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_RapportoAutovettureController : NvxControllerBase
    {
        private readonly IDip_RapportoAutovettureService _dip_RapportoAutovettureService;

        public Dip_RapportoAutovettureController(IHttpContextAccessor httpContextAccessor,
                                                 IDip_RapportoAutovettureService dip_RapportoAutovettureService) : base(httpContextAccessor)
        {
            _dip_RapportoAutovettureService = dip_RapportoAutovettureService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll4User")]
        public async Task<GenericResult<Dip_RapportoAutovetture_GetAll4User_OutModel>> GetAll4User(GenericRequest<Dip_RapportoAutovetture_GetAll4User_InModel> inModel)
            => await _dip_RapportoAutovettureService.GetAll4User(inModel, false);
    }
}
