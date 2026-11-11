using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Compliance;
using nvxapp.server.service.ClientServer_Service.Compliance.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ServerModels;

namespace nvxapp.server.Controllers.Compliance
{
    // I permessi (ruoli dell'azienda) li controllano i servizi: qui solo l'autenticazione.

    [ApiController]
    [Route("api/[controller]")]
    public class Cmp_ConfigController : NvxControllerBase
    {
        private readonly ICmp_ConfigService _service;
        public Cmp_ConfigController(IHttpContextAccessor httpContextAccessor, ICmp_ConfigService service) : base(httpContextAccessor) { _service = service; }

        [Authorize, HttpPost, Route("Cmp_CfgGet")]
        public async Task<GenericResult<Cmp_CfgGetOutModel>> Cmp_CfgGet(GenericRequest<Cmp_EmptyInModel> inModel) => await _service.Cmp_CfgGet(inModel, false);
        [Authorize, HttpPost, Route("Cmp_CfgPut")]
        public async Task<GenericResult<Cmp_CfgPutOutModel>> Cmp_CfgPut(GenericRequest<Cmp_CfgPutInModel> inModel) => await _service.Cmp_CfgPut(inModel, false);
    }
}
