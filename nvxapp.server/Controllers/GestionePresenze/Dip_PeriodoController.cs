using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_PeriodoService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_PeriodoService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_PeriodoController : NvxControllerBase
    {
        private readonly IDip_PeriodoService _dip_PeriodoService;

        public Dip_PeriodoController(IHttpContextAccessor httpContextAccessor,
                                     IDip_PeriodoService dip_PeriodoService) : base(httpContextAccessor)
        {
            _dip_PeriodoService = dip_PeriodoService;
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_Periodo_Get")]
        public async Task<GenericResult<Dip_Periodo_Get_OutModel>> Dip_Periodo_Get(GenericRequest<Dip_Periodo_Get_InModel> inModel)
        {
            return await _dip_PeriodoService.Dip_Periodo_Get(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_Periodo_GetAll")]
        public async Task<GenericResult<Dip_Periodo_GetAll_OutModel>> Dip_Periodo_GetAll(GenericRequest<Dip_Periodo_GetAll_InModel> inModel)
        {
            return await _dip_PeriodoService.Dip_Periodo_GetAll(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_Periodo_SetStato")]
        public async Task<GenericResult<Dip_Periodo_SetStato_OutModel>> Dip_Periodo_SetStato(GenericRequest<Dip_Periodo_SetStato_InModel> inModel)
        {
            return await _dip_PeriodoService.Dip_Periodo_SetStato(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_Periodo_SetRiconosciute")]
        public async Task<GenericResult<Dip_Periodo_SetRiconosciute_OutModel>> Dip_Periodo_SetRiconosciute(GenericRequest<Dip_Periodo_SetRiconosciute_InModel> inModel)
        {
            return await _dip_PeriodoService.Dip_Periodo_SetRiconosciute(inModel, false);
        }
    }
}
