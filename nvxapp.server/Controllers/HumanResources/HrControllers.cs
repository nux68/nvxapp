using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.HumanResources;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.Controllers.HumanResources
{
    // I permessi (ruoli dell'azienda) li controllano i servizi: qui solo l'autenticazione.

    [ApiController]
    [Route("api/[controller]")]
    public class Hr_ConfigController : NvxControllerBase
    {
        private readonly IHr_ConfigService _service;
        public Hr_ConfigController(IHttpContextAccessor httpContextAccessor, IHr_ConfigService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Hr_CfgGet")]
        public async Task<GenericResult<Hr_CfgGetOutModel>> Hr_CfgGet(GenericRequest<Hr_EmptyInModel> inModel) => await _service.Hr_CfgGet(inModel, false);
        [Authorize, HttpPost, Route("Hr_CfgPut")]
        public async Task<GenericResult<Hr_CfgPutOutModel>> Hr_CfgPut(GenericRequest<Hr_CfgPutInModel> inModel) => await _service.Hr_CfgPut(inModel, false);
    }
}
