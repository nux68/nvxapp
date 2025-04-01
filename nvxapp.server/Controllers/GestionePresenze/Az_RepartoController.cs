using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_Reparto;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_RepartoService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_RepartoController : NvxControllerBase
    {
        private readonly IAz_RepartoService _az_RepartoService;

        public Az_RepartoController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IAz_RepartoService az_RepartoService
        ) : base(httpContextAccessor)
        {
            _az_RepartoService = az_RepartoService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_RepartoOutModel>> GetAll(GenericRequest<Az_RepartoInModel> inModel)
        {
            var res = await _az_RepartoService.GetAll(inModel, false);

            return res;
        }

        

    }



}
