using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_SediRepartoController : NvxControllerBase
    {
        private readonly IAz_SediRepartoService _az_RepartoService;

        public Az_SediRepartoController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IAz_SediRepartoService az_RepartoService
        ) : base(httpContextAccessor)
        {
            _az_RepartoService = az_RepartoService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_SediReparto_GetAll_OutModel>> GetAll(GenericRequest<Az_SediReparto_GetAll_InModel> inModel)
        {
            var res = await _az_RepartoService.GetAll(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Az_SediRepartoGet")]
        public async Task<GenericResult<Az_SediRepartoGetOutModel>> Az_SediRepartoGet(GenericRequest<Az_SediRepartoGetInModel> inModel)
        {
            var res = await _az_RepartoService.Az_SediRepartoGet(inModel, false);

            return res;
        }


        [Authorize]
        [HttpPost]
        [Route("Az_SediRepartoPut")]
        public async Task<GenericResult<Az_SediRepartoPutOutModel>> Az_SediRepartoPut(GenericRequest<Az_SediRepartoPutInModel> inModel)
        {
            var res = await _az_RepartoService.Az_SediRepartoPut(inModel, false);

            return res;
        }



    }



}
