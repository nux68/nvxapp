using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_AnagraficaController : NvxControllerBase
    {
        private readonly IDip_AnagraficaService _dip_AnagraficaService;

        public Dip_AnagraficaController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_AnagraficaService dip_AnagraficaService
        ) : base(httpContextAccessor)
        {
            _dip_AnagraficaService = dip_AnagraficaService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_Anagrafica_GetAll_OutModel>> GetAll(GenericRequest<Dip_Anagrafica_GetAll_InModel> inModel)
        {
            var res = await _dip_AnagraficaService.GetAll(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_AnagraficaGet")]
        public async Task<GenericResult<Dip_Anagrafica_Get_OutModel>> Dip_AnagraficaGet(GenericRequest<Dip_Anagrafica_Get_InModel> inModel)
        {
            return await _dip_AnagraficaService.Dip_AnagraficaGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_AnagraficaPut")]
        public async Task<GenericResult<Dip_Anagrafica_Put_OutModel>> Dip_AnagraficaPut(GenericRequest<Dip_Anagrafica_Put_InModel> inModel)
        {
            return await _dip_AnagraficaService.Dip_AnagraficaPut(inModel, false);
        }

    }



}
