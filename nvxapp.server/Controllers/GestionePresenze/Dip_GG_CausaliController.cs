using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_Anagrafica;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_Causali;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;



namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_GG_CausaliController : NvxControllerBase
    {
        private readonly IDip_GG_CausaliService _dip_GG_CausaliService;

        public Dip_GG_CausaliController(
                                        IHttpContextAccessor httpContextAccessor,
                                        IDip_GG_CausaliService dip_GG_CausaliService
        ) : base(httpContextAccessor)
        {
            _dip_GG_CausaliService = dip_GG_CausaliService;
        }

        

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Dip_GG_CausaliOutModel>> GetAll(GenericRequest<Dip_GG_CausaliInModel> inModel)
        {
            var res = await _dip_GG_CausaliService.GetAll(inModel, false);

            return res;
        }

        

    }



}
