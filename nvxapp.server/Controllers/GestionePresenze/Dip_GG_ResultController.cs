using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_GG_ResultController : NvxControllerBase
    {
        private readonly IDip_GG_ResultService _dip_GG_ResultService;

        public Dip_GG_ResultController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_GG_ResultService dip_GG_ResultService
        ) : base(httpContextAccessor)
        {
            _dip_GG_ResultService = dip_GG_ResultService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_GG_Result_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Result_GetAll_InModel> inModel)
        {
            var res = await _dip_GG_ResultService.GetAll(inModel, false);

            return res;
        }
        

        [Authorize]
        [HttpPost]
        [Route("Dip_GG_Result_Get_4Calculation")]
        public async Task<GenericResult<Dip_GG_Result_Get_4Calculation_OutModel>> Dip_GG_Result_Get_4Calculation(GenericRequest<Dip_GG_Result_Get_4Calculation_InModel> inModel)
        {
            var res = await _dip_GG_ResultService.Dip_GG_Result_Get_4Calculation(inModel, false);

            return res;
        }

    }

}
