using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Assets;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.Controllers.Assets
{
    // Dati dei widget della dashboard (ANALISI_ASSETS_WIDGET.md). I permessi li controlla il servizio: qui solo l'autenticazione.
    [ApiController]
    [Route("api/[controller]")]
    public class Ast_WidgetController : NvxControllerBase
    {
        private readonly IAst_WidgetService _service;
        public Ast_WidgetController(IHttpContextAccessor httpContextAccessor, IAst_WidgetService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Ast_WidgetFlottaGet")]
        public async Task<GenericResult<Ast_WidgetFlottaOutModel>> Ast_WidgetFlottaGet(GenericRequest<Ast_EmptyInModel> inModel) => await _service.Ast_WidgetFlottaGet(inModel, false);
    }
}
