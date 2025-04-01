using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_AnagraficaController : NvxControllerBase
    {
        private readonly IAz_AnagraficaService _az_AnagraficaService;

        public Az_AnagraficaController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IAz_AnagraficaService az_AnagraficaService
        ) : base(httpContextAccessor)
        {
            _az_AnagraficaService = az_AnagraficaService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_AnagraficaOutModel>> GetAll(GenericRequest<Az_AnagraficaInModel> inModel)
        {
            var res = await _az_AnagraficaService.GetAll(inModel, false);

            return res;
        }

        

    }



}
