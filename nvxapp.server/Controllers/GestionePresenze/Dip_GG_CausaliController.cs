using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_GG_CausaliController : NvxControllerBase
    {
        private readonly IDip_GG_CausaliService _dip_GG_CausaliService;

        public Dip_GG_CausaliController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_GG_CausaliService dip_GG_CausaliService
        ) : base(httpContextAccessor)
        {
            _dip_GG_CausaliService = dip_GG_CausaliService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_GG_Causali_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Causali_GetAll_InModel> inModel)
        {
            var res = await _dip_GG_CausaliService.GetAll(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_GG_Causali_Get_4Calculation")]
        public async Task<GenericResult<Dip_GG_Causali_Get_4Calculation_OutModel>> Dip_GG_Causali_Get_4Calculation(GenericRequest<Dip_GG_Causali_Get_4Calculation_InModel> inModel)
        {
            var res = await _dip_GG_CausaliService.Dip_GG_Causali_Get_4Calculation(inModel, false);

            return res;
        }

    }



}
