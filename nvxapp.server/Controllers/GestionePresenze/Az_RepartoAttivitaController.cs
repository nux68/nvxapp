using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_RepartoAttivita;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_RepartoAttivitaService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_RepartoAttivitaController : NvxControllerBase
    {
        private readonly IAz_RepartoAttivitaService _az_RepartoAttivitaService;

        public Az_RepartoAttivitaController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IAz_RepartoAttivitaService az_RepartoAttivitaService
        ) : base(httpContextAccessor)
        {
            _az_RepartoAttivitaService = az_RepartoAttivitaService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_RepartoAttivitaOutModel>> GetAll(GenericRequest<Az_RepartoAttivitaInModel> inModel)
        {
            var res = await _az_RepartoAttivitaService.GetAll(inModel, false);

            return res;
        }

        

    }



}
