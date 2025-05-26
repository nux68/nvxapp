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
        [Route("Get4User")]
        public async Task<GenericResult<Az_SediReparto_Get4User_OutModel>> Get4User(GenericRequest<Az_SediReparto_Get4User_InModel> inModel)
        {
            var res = await _az_RepartoService.Get4User(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Get4Admin")]
        public async Task<GenericResult<Az_SediReparto_Get4Admin_OutModel>> Get4Admin(GenericRequest<Az_SediReparto_Get4Admin_InModel> inModel)
        {
            var res = await _az_RepartoService.Get4Admin(inModel, false);

            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Get4AdminApproval")]
        public async Task<GenericResult<Az_SediReparto_Get4AdminApproval_OutModel>> Get4AdminApproval(GenericRequest<Az_SediReparto_Get4AdminApproval_InModel> inModel)
        {
            var res = await _az_RepartoService.Get4AdminApproval(inModel, false);

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
