using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Digest;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Digest.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Riepiloghi periodici dell'utente corrente e "Genera ora" (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-AI.3)
    [ApiController]
    [Route("api/[controller]")]
    public class DigestController : NvxControllerBase
    {
        private readonly IDigestService _service;

        public DigestController(IHttpContextAccessor httpContextAccessor,
                                IDigestService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("DigestList")]
        public async Task<GenericResult<DigestListOutModel>> DigestList(GenericRequest<DigestListInModel> inModel)
        {
            return await _service.DigestList(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DigestGet")]
        public async Task<GenericResult<DigestGetOutModel>> DigestGet(GenericRequest<DigestGetInModel> inModel)
        {
            return await _service.DigestGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DigestGenerate")]
        public async Task<GenericResult<DigestGenerateOutModel>> DigestGenerate(GenericRequest<DigestGenerateInModel> inModel)
        {
            return await _service.DigestGenerate(inModel, false);
        }
    }
}
