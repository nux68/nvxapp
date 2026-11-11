using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaCdgService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaCdgService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    // Controllo di gestione sulle commesse (ANALISI_ATTENDANCETRACKING_CONTROLLO_GESTIONE.md): i permessi li controlla il servizio
    [ApiController]
    [Route("api/[controller]")]
    public class Az_CommessaCdgController : NvxControllerBase
    {
        private readonly IAz_CommessaCdgService _service;

        public Az_CommessaCdgController(IHttpContextAccessor httpContextAccessor, IAz_CommessaCdgService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize, HttpPost, Route("CdgGet")]
        public async Task<GenericResult<Az_CommessaCdg_GetOutModel>> CdgGet(GenericRequest<Az_CommessaCdg_GetInModel> inModel) => await _service.CdgGet(inModel, false);

        [Authorize, HttpPost, Route("Widget_OreCommesseGet")]
        public async Task<GenericResult<Widget_OreCommesseGetOutModel>> Widget_OreCommesseGet(GenericRequest<Widget_OreCommesseGetInModel> inModel) => await _service.Widget_OreCommesseGet(inModel, false);
    }
}
