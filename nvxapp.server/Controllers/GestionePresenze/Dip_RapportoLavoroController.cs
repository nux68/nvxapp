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
        [Route("Dip_RapportoLavoroGet")]
        public async Task<GenericResult<Dip_RapportoLavoro_Get_OutModel>> Dip_RapportoLavoroGet(GenericRequest<Dip_RapportoLavoro_Get_InModel> inModel)
        {
            return await _dip_RapportoLavoroService.Dip_RapportoLavoroGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_RapportoLavoroPut")]
        public async Task<GenericResult<Dip_RapportoLavoro_Put_OutModel>> Dip_RapportoLavoroPut(GenericRequest<Dip_RapportoLavoro_Put_InModel> inModel)
        {
            return await _dip_RapportoLavoroService.Dip_RapportoLavoroPut(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Dip_RapportoLavoro_Get_4Users")]
        public async Task<GenericResult<Dip_RapportoLavoro_Get_4Users_OutModel>> Dip_RapportoLavoro_Get_4Users(GenericRequest<Dip_RapportoLavoro_Get_4Users_InModel> inModel)
        {
            return await _dip_RapportoLavoroService.Dip_RapportoLavoro_Get_4Users(inModel, false);
        }

    }



}
