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
    public class Dip_TurnoScambioController : NvxControllerBase
    {
        private readonly IDip_TurnoScambioService _service;
        public Dip_TurnoScambioController(IHttpContextAccessor httpContextAccessor, IDip_TurnoScambioService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Proponi")]
        public async Task<GenericResult<Dip_TurnoScambio_ProponiOutModel>> Proponi(GenericRequest<Dip_TurnoScambio_ProponiInModel> inModel) => await _service.Proponi(inModel, false);

        [Authorize, HttpPost, Route("Rispondi")]
        public async Task<GenericResult<Dip_TurnoScambio_RispondiOutModel>> Rispondi(GenericRequest<Dip_TurnoScambio_RispondiInModel> inModel) => await _service.Rispondi(inModel, false);

        [Authorize, HttpPost, Route("Annulla")]
        public async Task<GenericResult<Dip_TurnoScambio_AnnullaOutModel>> Annulla(GenericRequest<Dip_TurnoScambio_AnnullaInModel> inModel) => await _service.Annulla(inModel, false);
    }
}
