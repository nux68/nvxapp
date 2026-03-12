using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_GG_GiustificativiController : NvxControllerBase
    {
        private readonly IDip_GG_GiustificativiService _dip_GG_GiustificativiService;

        public Dip_GG_GiustificativiController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_GG_GiustificativiService dip_GG_GiustificativiService
        ) : base(httpContextAccessor)
        {
            _dip_GG_GiustificativiService = dip_GG_GiustificativiService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_GG_Giustificativi_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Giustificativi_GetAll_InModel> inModel)
        {
            var res = await _dip_GG_GiustificativiService.GetAll(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_GG_Giustificativi_Get_4Calculation")]
        public async Task<GenericResult<Dip_GG_Giustificativi_Get_4Calculation_OutModel>> Dip_GG_Giustificativi_Get_4Calculation(GenericRequest<Dip_GG_Giustificativi_Get_4Calculation_InModel> inModel)
        {
            var res = await _dip_GG_GiustificativiService.Dip_GG_Giustificativi_Get_4Calculation(inModel, false);

            return res;
        }

    }



}
