using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_OrarioIntervalloHHController : NvxControllerBase
    {
        private readonly IPar_OrarioIntervalloHHService _par_OrarioIntervalloHHService;

        public Par_OrarioIntervalloHHController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IPar_OrarioIntervalloHHService par_OrarioIntervalloHHService
        ) : base(httpContextAccessor)
        {
            _par_OrarioIntervalloHHService = par_OrarioIntervalloHHService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_OrarioIntervalloHHOutModel>> GetAll(GenericRequest<Par_OrarioIntervalloHHInModel> inModel)
        {
            var res = await _par_OrarioIntervalloHHService.GetAll(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Par_OrarioIntervalloHH_Get")]
        public async Task<GenericResult<Par_OrarioIntervalloHH_Get_4Edit_OutModel>> Par_OrarioIntervalloHH_Get(GenericRequest<Par_OrarioIntervalloHH_Get_4Edit_InModel> inModel)
        {
            var res = await _par_OrarioIntervalloHHService.Par_OrarioIntervalloHH_Get(inModel, false);

            return res;
        }
        

        [Authorize]
        [HttpPost]
        [Route("Par_OrarioIntervalloHH_Put")]
        public async Task<GenericResult<Par_OrarioIntervalloHH_Put_4Edit_OutModel>> Par_OrarioIntervalloHH_Put(GenericRequest<Par_OrarioIntervalloHH_Put_4Edit_InModel> inModel)
        {
            var res = await _par_OrarioIntervalloHHService.Par_OrarioIntervalloHH_Put(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Par_OrarioIntervalloHH_Arrange_NumCoppie")]
        public async Task<GenericResult<Par_OrarioIntervalloHH_Arrange_Coppie_OutModel>> Par_OrarioIntervalloHH_Arrange_NumCoppie(GenericRequest<Par_OrarioIntervalloHH_Arrange_Coppie_InModel> inModel)
        {
            var res = await _par_OrarioIntervalloHHService.Par_OrarioIntervalloHH_Arrange_NumCoppie(inModel, false);

            return res;
        }

    }



}
