using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.CommonOrganizzazioneService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Common.CommonOrganizzazioneService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.Infrastructure
{
    // sedi, reparti e competenze dell'azienda (tabelle comuni, schema Common) in sola lettura, per i moduli che non sono
    // le presenze: disponibile con qualsiasi applicativo attivo
    [ApiController]
    [Route("api/[controller]")]
    public class CommonOrganizzazioneController : NvxControllerBase
    {
        private readonly ICommonOrganizzazioneService _service;

        public CommonOrganizzazioneController(IHttpContextAccessor httpContextAccessor, ICommonOrganizzazioneService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("SediGetAll")]
        public async Task<GenericResult<CommonOrganizzazione_SediGetAll_OutModel>> SediGetAll(GenericRequest<CommonOrganizzazione_SediGetAll_InModel> inModel)
            => await _service.SediGetAll(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("CompetenzeGetAll")]
        public async Task<GenericResult<CommonOrganizzazione_CompetenzeGetAll_OutModel>> CompetenzeGetAll(GenericRequest<CommonOrganizzazione_CompetenzeGetAll_InModel> inModel)
            => await _service.CompetenzeGetAll(inModel, false);
    }
}
