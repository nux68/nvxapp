using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHH;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioIntervalloHHService.Models;



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

        

    }



}
