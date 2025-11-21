using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_CausaliController : NvxControllerBase
    {
        private readonly IPar_CausaliService _par_CausaliService;

        public Par_CausaliController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IPar_CausaliService par_CausaliService
        ) : base(httpContextAccessor)
        {
            _par_CausaliService = par_CausaliService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_CausaliOutModel>> GetAll(GenericRequest<Par_CausaliInModel> inModel)
        {
            var res = await _par_CausaliService.GetAll(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Par_CausaliGet")]
        public async Task<GenericResult<Par_CausaliGetOutModel>> Par_CausaliGet(GenericRequest<Par_CausaliGetInModel> inModel)
        {
            var res = await _par_CausaliService.Par_CausaliGet(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Par_CausaliPut")]
        public async Task<GenericResult<Par_CausaliPutOutModel>> Par_CausaliPut(GenericRequest<Par_CausaliPutInModel> inModel)
        {
            var res = await _par_CausaliService.Par_CausaliPut(inModel, false);

            return res;
        }



    }



}
