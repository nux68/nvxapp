using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_WidgetService;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_WidgetService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.Controllers.DocumentManagement
{
    // Dati dei widget della dashboard del documentale (ANALISI_WIDGET_DOCUMENTALE.md): i permessi li controlla il servizio
    [ApiController]
    [Route("api/[controller]")]
    public class Doc_WidgetController : NvxControllerBase
    {
        private readonly IDoc_WidgetService _service;

        public Doc_WidgetController(IHttpContextAccessor httpContextAccessor, IDoc_WidgetService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize, HttpPost, Route("Doc_SpazioPerTipoGet")]
        public async Task<GenericResult<Doc_SpazioPerTipoGetOutModel>> Doc_SpazioPerTipoGet(GenericRequest<Doc_SpazioPerTipoGetInModel> inModel) => await _service.Doc_SpazioPerTipoGet(inModel, false);
    }
}
