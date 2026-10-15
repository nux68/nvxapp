using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FestivitaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_FestivitaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Par_FestivitaController : NvxControllerBase
    {
        private readonly IPar_FestivitaService _par_FestivitaService;

        public Par_FestivitaController(IHttpContextAccessor httpContextAccessor,
                                       IPar_FestivitaService par_FestivitaService) : base(httpContextAccessor)
        {
            _par_FestivitaService = par_FestivitaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Par_Festivita_GetAllOutModel>> GetAll(GenericRequest<Par_Festivita_GetAllInModel> inModel)
        {
            return await _par_FestivitaService.GetAll(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_FestivitaGet")]
        public async Task<GenericResult<Par_Festivita_GetOutModel>> Par_FestivitaGet(GenericRequest<Par_Festivita_GetInModel> inModel)
        {
            return await _par_FestivitaService.Par_FestivitaGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_FestivitaPut")]
        public async Task<GenericResult<Par_Festivita_PutOutModel>> Par_FestivitaPut(GenericRequest<Par_Festivita_PutInModel> inModel)
        {
            return await _par_FestivitaService.Par_FestivitaPut(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("Par_FestivitaDelete")]
        public async Task<GenericResult<Par_Festivita_DeleteOutModel>> Par_FestivitaDelete(GenericRequest<Par_Festivita_DeleteInModel> inModel)
        {
            return await _par_FestivitaService.Par_FestivitaDelete(inModel, false);
        }
    }
}
