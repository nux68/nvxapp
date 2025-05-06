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
        public async Task<GenericResult<Az_SediRepartoOutModel>> GetAll(GenericRequest<Az_SediRepartoInModel> inModel)
        {
            var res = await _az_RepartoService.GetAll(inModel, false);

            return res;
        }

        

    }



}
