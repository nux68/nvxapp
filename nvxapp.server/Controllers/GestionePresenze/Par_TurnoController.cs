using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TurniService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    // I permessi (ruoli dell'azienda) li controllano i servizi: qui solo l'autenticazione.
    [ApiController]
    [Route("api/[controller]")]
    public class Par_TurnoController : NvxControllerBase
    {
        private readonly IPar_TurnoService _service;
        public Par_TurnoController(IHttpContextAccessor httpContextAccessor, IPar_TurnoService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("GetAll")]
        public async Task<GenericResult<Par_Turno_GetAllOutModel>> GetAll(GenericRequest<Par_Turno_GetAllInModel> inModel) => await _service.GetAll(inModel, false);

        [Authorize, HttpPost, Route("Par_TurnoGet")]
        public async Task<GenericResult<Par_Turno_GetOutModel>> Par_TurnoGet(GenericRequest<Par_Turno_GetInModel> inModel) => await _service.Par_TurnoGet(inModel, false);

        [Authorize, HttpPost, Route("Par_TurnoPut")]
        public async Task<GenericResult<Par_Turno_PutOutModel>> Par_TurnoPut(GenericRequest<Par_Turno_PutInModel> inModel) => await _service.Par_TurnoPut(inModel, false);

        [Authorize, HttpPost, Route("Par_TurnoDelete")]
        public async Task<GenericResult<Par_Turno_DeleteOutModel>> Par_TurnoDelete(GenericRequest<Par_Turno_DeleteInModel> inModel) => await _service.Par_TurnoDelete(inModel, false);

        [Authorize, HttpPost, Route("Coperture4Reparto")]
        public async Task<GenericResult<Az_SediRepartoTurno_GetOutModel>> Coperture4Reparto(GenericRequest<Az_SediRepartoTurno_GetInModel> inModel) => await _service.Coperture4Reparto(inModel, false);

        [Authorize, HttpPost, Route("Coperture4RepartoPut")]
        public async Task<GenericResult<Az_SediRepartoTurno_PutOutModel>> Coperture4RepartoPut(GenericRequest<Az_SediRepartoTurno_PutInModel> inModel) => await _service.Coperture4RepartoPut(inModel, false);
    }
}
