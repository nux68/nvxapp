using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.SmartWorkingService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.SmartWorkingService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    // I permessi (ruoli dell'azienda) li controllano i servizi: qui solo l'autenticazione.
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_AccordoSWController : NvxControllerBase
    {
        private readonly IDip_AccordoSWService _service;
        public Dip_AccordoSWController(IHttpContextAccessor httpContextAccessor, IDip_AccordoSWService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("GetAll")]
        public async Task<GenericResult<Dip_AccordoSW_GetAllOutModel>> GetAll(GenericRequest<Dip_AccordoSW_GetAllInModel> inModel) => await _service.GetAll(inModel, false);

        [Authorize, HttpPost, Route("Dip_AccordoSWGet")]
        public async Task<GenericResult<Dip_AccordoSW_GetOutModel>> Dip_AccordoSWGet(GenericRequest<Dip_AccordoSW_GetInModel> inModel) => await _service.Dip_AccordoSWGet(inModel, false);

        [Authorize, HttpPost, Route("Dip_AccordoSWPut")]
        public async Task<GenericResult<Dip_AccordoSW_PutOutModel>> Dip_AccordoSWPut(GenericRequest<Dip_AccordoSW_PutInModel> inModel) => await _service.Dip_AccordoSWPut(inModel, false);

        [Authorize, HttpPost, Route("Dip_AccordoSWDelete")]
        public async Task<GenericResult<Dip_AccordoSW_DeleteOutModel>> Dip_AccordoSWDelete(GenericRequest<Dip_AccordoSW_DeleteInModel> inModel) => await _service.Dip_AccordoSWDelete(inModel, false);

        [Authorize, HttpPost, Route("Calendario")]
        public async Task<GenericResult<Dip_AccordoSW_CalendarioOutModel>> Calendario(GenericRequest<Dip_AccordoSW_CalendarioInModel> inModel) => await _service.Calendario(inModel, false);
    }
}
