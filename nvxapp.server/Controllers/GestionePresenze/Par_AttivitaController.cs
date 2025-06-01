using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaService.Models;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_AttivitaController : NvxControllerBase
    {
        private readonly IPar_AttivitaService _par_AttivitaService;

        public Par_AttivitaController(
            IHttpContextAccessor httpContextAccessor,
            IPar_AttivitaService par_AttivitaService
        ) : base(httpContextAccessor)
        {
            _par_AttivitaService = par_AttivitaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_Attivita_GetAll_OutModel>> GetAll(GenericRequest<Par_Attivita_GetAll_InModel> inModel)
        {
            var res = await _par_AttivitaService.GetAll(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Par_AttivitaGet")]
        public async Task<GenericResult<Par_AttivitaGetOutModel>> Par_AttivitaGet(GenericRequest<Par_AttivitaGetInModel> inModel)
        {
            var res = await _par_AttivitaService.Par_AttivitaGet(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Par_AttivitaPut")]
        public async Task<GenericResult<Par_AttivitaPutOutModel>> Par_AttivitaPut(GenericRequest<Par_AttivitaPutInModel> inModel)
        {
            var res = await _par_AttivitaService.Par_AttivitaPut(inModel, false);
            return res;
        }
    }
}
