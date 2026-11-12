using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Compliance;
using nvxapp.server.service.ClientServer_Service.Compliance.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.Controllers.Compliance
{
    // Widget della dashboard di Compliance (ANALISI_COMPLIANCE_WIDGET.md). I permessi li controlla il servizio.
    [ApiController]
    [Route("api/[controller]")]
    public class Cmp_WidgetController : NvxControllerBase
    {
        private readonly ICmp_WidgetService _service;
        public Cmp_WidgetController(IHttpContextAccessor httpContextAccessor, ICmp_WidgetService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Cmp_WidgetScadenzeGet")]
        public async Task<GenericResult<Cmp_WidgetScadenzeGetOutModel>> Cmp_WidgetScadenzeGet(GenericRequest<Cmp_EmptyInModel> inModel) => await _service.Cmp_WidgetScadenzeGet(inModel, false);
        [Authorize, HttpPost, Route("Cmp_WidgetGdprGet")]
        public async Task<GenericResult<Cmp_WidgetGdprGetOutModel>> Cmp_WidgetGdprGet(GenericRequest<Cmp_EmptyInModel> inModel) => await _service.Cmp_WidgetGdprGet(inModel, false);
    }
}
