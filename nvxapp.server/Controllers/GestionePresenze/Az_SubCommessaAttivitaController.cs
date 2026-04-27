using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SubCommessaAttivitaService;

namespace nvxapp.server.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    public class Az_SubCommessaAttivitaController : NvxControllerBase
    {
        private readonly IAz_SubCommessaAttivitaService _az_SubCommessaAttivitaService;

        public Az_SubCommessaAttivitaController(
            IHttpContextAccessor httpContextAccessor,
            IAz_SubCommessaAttivitaService az_SubCommessaAttivitaService
        ) : base(httpContextAccessor)
        {
            _az_SubCommessaAttivitaService = az_SubCommessaAttivitaService;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll")]
        public async Task<GenericResult<Az_SubCommessaAttivita_GetAll_OutModel>> GetAll(GenericRequest<Az_SubCommessaAttivita_GetAll_InModel> inModel)
        {
            var res = await _az_SubCommessaAttivitaService.GetAll(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("GetAll_4FullList")]
        public async Task<GenericResult<Az_SubCommessaAttivita_GetAll_4FullList_OutModel>> GetAll_4FullList(GenericRequest<Az_SubCommessaAttivita_GetAll_4FullList_InModel> inModel)
        {
            var res = await _az_SubCommessaAttivitaService.GetAll_4FullList(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Get4SubCommessa")]
        public async Task<GenericResult<Az_SubCommessaAttivita_Get4SubCommessa_OutModel>> Get4SubCommessa(GenericRequest<Az_SubCommessaAttivita_Get4SubCommessa_InModel> inModel)
        {
            var res = await _az_SubCommessaAttivitaService.Get4SubCommessa(inModel, false);
            return res;
        }

        [Authorize]
        [HttpPost]
        [Route("Put4SubCommessa")]
        public async Task<GenericResult<Az_SubCommessaAttivita_Put4SubCommessa_OutModel>> Put4SubCommessa(GenericRequest<Az_SubCommessaAttivita_Put4SubCommessa_InModel> inModel)
        {
            var res = await _az_SubCommessaAttivitaService.Put4SubCommessa(inModel, false);
            return res;
        }
    }
}
