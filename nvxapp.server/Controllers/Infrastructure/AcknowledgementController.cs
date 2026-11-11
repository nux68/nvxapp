using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Prese visione dell'utente corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.6)
    [ApiController]
    [Route("api/[controller]")]
    public class AcknowledgementController : NvxControllerBase
    {
        private readonly IAcknowledgementUserService _service;

        public AcknowledgementController(IHttpContextAccessor httpContextAccessor,
                                         IAcknowledgementUserService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("AcknowledgementList")]
        public async Task<GenericResult<AcknowledgementListOutModel>> AcknowledgementList(GenericRequest<AcknowledgementListInModel> inModel)
        {
            return await _service.AcknowledgementList(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("AcknowledgementConfirm")]
        public async Task<GenericResult<AcknowledgementConfirmOutModel>> AcknowledgementConfirm(GenericRequest<AcknowledgementConfirmInModel> inModel)
        {
            return await _service.AcknowledgementConfirm(inModel, false);
        }
    }
}
