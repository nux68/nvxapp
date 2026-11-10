using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_WidgetService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_WidgetService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.GestionePresenze
{
    // Dati dei widget della dashboard delle presenze (ANALISI_WIDGET_PRESENZE.md): i permessi li controlla il servizio
    [ApiController]
    [Route("api/[controller]")]
    public class Dip_WidgetController : NvxControllerBase
    {
        private readonly IDip_WidgetService _service;

        public Dip_WidgetController(IHttpContextAccessor httpContextAccessor, IDip_WidgetService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize, HttpPost, Route("Widget_PresenzeOggiGet")]
        public async Task<GenericResult<Widget_PresenzeOggiGetOutModel>> Widget_PresenzeOggiGet(GenericRequest<Widget_PresenzeOggiGetInModel> inModel) => await _service.Widget_PresenzeOggiGet(inModel, false);

        [Authorize, HttpPost, Route("Widget_RichiesteGet")]
        public async Task<GenericResult<Widget_RichiesteGetOutModel>> Widget_RichiesteGet(GenericRequest<Widget_RichiesteGetInModel> inModel) => await _service.Widget_RichiesteGet(inModel, false);
    }
}
