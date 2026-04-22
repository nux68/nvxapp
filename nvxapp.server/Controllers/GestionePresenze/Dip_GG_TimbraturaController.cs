using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_GG_TimbraturaController : NvxControllerBase
    {
        private readonly IDip_GG_TimbraturaService _Dip_GG_TimbraturaService;

        public Dip_GG_TimbraturaController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_GG_TimbraturaService Dip_GG_TimbraturaService
        ) : base(httpContextAccessor)
        {
            _Dip_GG_TimbraturaService = Dip_GG_TimbraturaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_GG_Timbratura_GetAll_OutModel>> GetAll(GenericRequest<Dip_GG_Timbratura_GetAll_InModel> inModel)
        {
            var res = await _Dip_GG_TimbraturaService.GetAll(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Stamp")]
        public async Task<GenericResult<Dip_GG_Timbratura_Stamp_OutModel>> Stamp(GenericRequest<Dip_GG_Timbratura_Stamp_InModel> inModel)
        {
            var res = await _Dip_GG_TimbraturaService.Stamp(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_GG_Timbratura_Get_4Calculation")]
        public async Task<GenericResult<Dip_GG_Timbratura_Get_4Calculation_OutModel>> Dip_GG_Timbratura_Get_4Calculation(GenericRequest<Dip_GG_Timbratura_Get_4Calculation_InModel> inModel)
        {
            var res = await _Dip_GG_TimbraturaService.Dip_GG_Timbratura_Get_4Calculation(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_GG_TimbraturaPut")]
        public async Task<GenericResult<Dip_GG_TimbraturaPutOutModel>> Dip_GG_TimbraturaPut(GenericRequest<Dip_GG_TimbraturaPutInModel> inModel)
        {
            var res = await _Dip_GG_TimbraturaService.Dip_GG_TimbraturaPut(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_GG_TimbraturaGet")]
        public async Task<GenericResult<Dip_GG_TimbraturaGetOutModel>> Dip_GG_TimbraturaGet(GenericRequest<Dip_GG_TimbraturaGetInModel> inModel)
        {
            var res = await _Dip_GG_TimbraturaService.Dip_GG_TimbraturaGet(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_GG_Timbratura_Delete")]
        public async Task<GenericResult<Dip_GG_Timbratura_DeleteOutModel>> Dip_GG_Timbratura_Delete(GenericRequest<Dip_GG_Timbratura_DeleteInModel> inModel)
        {
            var res = await _Dip_GG_TimbraturaService.Dip_GG_TimbraturaDelete(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("PrepareStamp")]
        public async Task<GenericResult<Dip_GG_Timbratura_StampPrepare_OutModel>> PrepareStamp(GenericRequest<Dip_GG_Timbratura_StampPrepare_InModel> inModel)
        {
            var res = await _Dip_GG_TimbraturaService.PrepareStamp(inModel, false);

            return res;
        }
    }

}
