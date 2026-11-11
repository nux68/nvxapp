using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnomalieService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnomalieService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    // Anomalie delle timbrature, giornate da sistemare e previsione delle assenze (ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md):
    // i permessi li controlla il servizio
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_AnomalieController : NvxControllerBase
    {
        private readonly IDip_AnomalieService _service;

        public Dip_AnomalieController(IHttpContextAccessor httpContextAccessor, IDip_AnomalieService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize, HttpPost, Route("AnomalieGet")]
        public async Task<GenericResult<Dip_Anomalie_GetOutModel>> AnomalieGet(GenericRequest<Dip_Anomalie_GetInModel> inModel) => await _service.AnomalieGet(inModel, false);

        [Authorize, HttpPost, Route("AnomalieSpiega")]
        public async Task<GenericResult<Dip_Anomalie_SpiegaOutModel>> AnomalieSpiega(GenericRequest<Dip_Anomalie_GetInModel> inModel) => await _service.AnomalieSpiega(inModel, false);

        [Authorize, HttpPost, Route("PrevisioneAssenzeGet")]
        public async Task<GenericResult<Dip_PrevisioneAssenze_GetOutModel>> PrevisioneAssenzeGet(GenericRequest<Dip_PrevisioneAssenze_GetInModel> inModel) => await _service.PrevisioneAssenzeGet(inModel, false);

        [Authorize, HttpPost, Route("Widget_GiornateDaSistemareGet")]
        public async Task<GenericResult<Widget_GiornateDaSistemareGetOutModel>> Widget_GiornateDaSistemareGet(GenericRequest<Widget_GiornateDaSistemareGetInModel> inModel) => await _service.Widget_GiornateDaSistemareGet(inModel, false);

        [Authorize, HttpPost, Route("Widget_PrevisioneAssenzeGet")]
        public async Task<GenericResult<Widget_PrevisioneAssenzeGetOutModel>> Widget_PrevisioneAssenzeGet(GenericRequest<Widget_PrevisioneAssenzeGetInModel> inModel) => await _service.Widget_PrevisioneAssenzeGet(inModel, false);
    }
}
