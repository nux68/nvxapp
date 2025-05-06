using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoAttivitaService;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_SediRepartoAttivitaController : NvxControllerBase
    {
        private readonly IAz_SediRepartoAttivitaService _az_SediRepartoAttivitaService;

        public Az_SediRepartoAttivitaController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IAz_SediRepartoAttivitaService az_SediRepartoAttivitaService
        ) : base(httpContextAccessor)
        {
            _az_SediRepartoAttivitaService = az_SediRepartoAttivitaService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_SediRepartoAttivitaOutModel>> GetAll(GenericRequest<Az_SediRepartoAttivitaInModel> inModel)
        {
            var res = await _az_SediRepartoAttivitaService.GetAll(inModel, false);

            return res;
        }

        

    }



}
