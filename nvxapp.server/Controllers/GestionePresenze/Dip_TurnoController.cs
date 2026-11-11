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
    public class Dip_TurnoController : NvxControllerBase
    {
        private readonly IDip_TurnoService _service;
        public Dip_TurnoController(IHttpContextAccessor httpContextAccessor, IDip_TurnoService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Pianificazione")]
        public async Task<GenericResult<Dip_Turno_PianificazioneOutModel>> Pianificazione(GenericRequest<Dip_Turno_PianificazioneInModel> inModel) => await _service.Pianificazione(inModel, false);

        [Authorize, HttpPost, Route("Put")]
        public async Task<GenericResult<Dip_Turno_PutOutModel>> Put(GenericRequest<Dip_Turno_PutInModel> inModel) => await _service.Put(inModel, false);

        [Authorize, HttpPost, Route("GetAll4User")]
        public async Task<GenericResult<Dip_Turno_GetAll4UserOutModel>> GetAll4User(GenericRequest<Dip_Turno_GetAll4UserInModel> inModel) => await _service.GetAll4User(inModel, false);
    }
}
