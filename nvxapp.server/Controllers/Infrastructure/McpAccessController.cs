using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Mcp;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Mcp.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Accesso di un'AI esterna tramite MCP (ANALISI_INFRASTRUCTURE_MCP.md): API delle pagine del client
    // (token personali, impostazione dell'azienda, registro delle chiamate). L'endpoint MCP vero e proprio
    // non e' un controller: vedi Utility/McpEndpoint.cs.
    [ApiController]
    [Route("api/[controller]")]
    public class McpAccessController : NvxControllerBase
    {
        private readonly IMcpAccessService _service;

        public McpAccessController(IHttpContextAccessor httpContextAccessor,
                                   IMcpAccessService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("McpTokenList")]
        public async Task<GenericResult<McpTokenListOutModel>> McpTokenList(GenericRequest<McpTokenListInModel> inModel)
        {
            return await _service.McpTokenList(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("McpTokenCreate")]
        public async Task<GenericResult<McpTokenCreateOutModel>> McpTokenCreate(GenericRequest<McpTokenCreateInModel> inModel)
        {
            return await _service.McpTokenCreate(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("McpTokenRevoke")]
        public async Task<GenericResult<McpTokenRevokeOutModel>> McpTokenRevoke(GenericRequest<McpTokenRevokeInModel> inModel)
        {
            return await _service.McpTokenRevoke(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("McpCompanySettingGet")]
        public async Task<GenericResult<McpCompanySettingGetOutModel>> McpCompanySettingGet(GenericRequest<McpCompanySettingGetInModel> inModel)
        {
            return await _service.McpCompanySettingGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("McpCompanySettingPut")]
        public async Task<GenericResult<McpCompanySettingPutOutModel>> McpCompanySettingPut(GenericRequest<McpCompanySettingPutInModel> inModel)
        {
            return await _service.McpCompanySettingPut(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("McpCallLogList")]
        public async Task<GenericResult<McpCallLogListOutModel>> McpCallLogList(GenericRequest<McpCallLogListInModel> inModel)
        {
            return await _service.McpCallLogList(inModel, false);
        }
    }
}
