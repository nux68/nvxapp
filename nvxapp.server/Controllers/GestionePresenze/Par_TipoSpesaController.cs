using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_TipoSpesaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_TipoSpesaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_TipoSpesaController : NvxControllerBase
    {
        private readonly IPar_TipoSpesaService _par_TipoSpesaService;

        public Par_TipoSpesaController(IHttpContextAccessor httpContextAccessor,
                                       IPar_TipoSpesaService par_TipoSpesaService) : base(httpContextAccessor)
        {
            _par_TipoSpesaService = par_TipoSpesaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_TipoSpesaOutModel>> GetAll(GenericRequest<Par_TipoSpesaInModel> inModel)
            => await _par_TipoSpesaService.GetAll(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Par_TipoSpesaGet")]
        public async Task<GenericResult<Par_TipoSpesaGetOutModel>> Par_TipoSpesaGet(GenericRequest<Par_TipoSpesaGetInModel> inModel)
            => await _par_TipoSpesaService.Par_TipoSpesaGet(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Par_TipoSpesaPut")]
        public async Task<GenericResult<Par_TipoSpesaPutOutModel>> Par_TipoSpesaPut(GenericRequest<Par_TipoSpesaPutInModel> inModel)
            => await _par_TipoSpesaService.Par_TipoSpesaPut(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("Par_TipoSpesaDelete")]
        public async Task<GenericResult<Par_TipoSpesa_DeleteOutModel>> Par_TipoSpesaDelete(GenericRequest<Par_TipoSpesa_DeleteInModel> inModel)
            => await _par_TipoSpesaService.Par_TipoSpesaDelete(inModel, false);
    }
}
