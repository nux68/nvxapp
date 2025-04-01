using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_Anagrafica;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_AnagraficaController : NvxControllerBase
    {
        private readonly IDip_AnagraficaService _dip_AnagraficaService;

        public Dip_AnagraficaController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_AnagraficaService dip_AnagraficaService
        ) : base(httpContextAccessor)
        {
            _dip_AnagraficaService = dip_AnagraficaService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_AnagraficaOutModel>> GetAll(GenericRequest<Dip_AnagraficaInModel> inModel)
        {
            var res = await _dip_AnagraficaService.GetAll(inModel, false);

            return res;
        }

        

    }



}
