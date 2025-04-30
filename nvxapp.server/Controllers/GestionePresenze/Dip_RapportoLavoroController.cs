using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_RapportoLavoroService;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_RapportoLavoroController : NvxControllerBase
    {
        private readonly IDip_RapportoLavoroService _dip_RapportoLavoroService;

        public Dip_RapportoLavoroController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_RapportoLavoroService dip_RapportoLavoroService
        ) : base(httpContextAccessor)
        {
            _dip_RapportoLavoroService = dip_RapportoLavoroService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_RapportoLavoro_GetAll_OutModel>> GetAll(GenericRequest<Dip_RapportoLavoro_GetAll_InModel> inModel)
        {
            var res = await _dip_RapportoLavoroService.GetAll(inModel, false);

            return res;
        }

        

    }



}
