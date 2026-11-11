using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Assets;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.Controllers.Assets
{
    // I permessi (ruoli dell'azienda) li controllano i servizi: qui solo l'autenticazione.

    [ApiController]
    [Route("api/[controller]")]
    public class Ast_ConfigController : NvxControllerBase
    {
        private readonly IAst_ConfigService _service;
        public Ast_ConfigController(IHttpContextAccessor httpContextAccessor, IAst_ConfigService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Ast_CfgGet")]
        public async Task<GenericResult<Ast_CfgGetOutModel>> Ast_CfgGet(GenericRequest<Ast_EmptyInModel> inModel) => await _service.Ast_CfgGet(inModel, false);
        [Authorize, HttpPost, Route("Ast_CfgPut")]
        public async Task<GenericResult<Ast_CfgPutOutModel>> Ast_CfgPut(GenericRequest<Ast_CfgPutInModel> inModel) => await _service.Ast_CfgPut(inModel, false);
    }
}
