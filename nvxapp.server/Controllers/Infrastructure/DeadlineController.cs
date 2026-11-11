using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Scadenzario dell'azienda corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.3)
    [ApiController]
    [Route("api/[controller]")]
    public class DeadlineController : NvxControllerBase
    {
        private readonly IDeadlineService _service;

        public DeadlineController(IHttpContextAccessor httpContextAccessor,
                                  IDeadlineService service) : base(httpContextAccessor)
        {
            _service = service;
        }

        [Authorize]
        [HttpPost]
        [Route("DeadlineList")]
        public async Task<GenericResult<DeadlineListOutModel>> DeadlineList(GenericRequest<DeadlineListInModel> inModel)
        {
            return await _service.DeadlineList(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DeadlineGet")]
        public async Task<GenericResult<DeadlineGetOutModel>> DeadlineGet(GenericRequest<DeadlineGetInModel> inModel)
        {
            return await _service.DeadlineGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DeadlinePut")]
        public async Task<GenericResult<DeadlinePutOutModel>> DeadlinePut(GenericRequest<DeadlinePutInModel> inModel)
        {
            return await _service.DeadlinePut(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DeadlineClose")]
        public async Task<GenericResult<DeadlineCloseOutModel>> DeadlineClose(GenericRequest<DeadlineCloseInModel> inModel)
        {
            return await _service.DeadlineClose(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DeadlineDelete")]
        public async Task<GenericResult<DeadlineDeleteOutModel>> DeadlineDelete(GenericRequest<DeadlineDeleteInModel> inModel)
        {
            return await _service.DeadlineDelete(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("DeadlineSummary")]
        public async Task<GenericResult<DeadlineSummaryOutModel>> DeadlineSummary(GenericRequest<DeadlineSummaryInModel> inModel)
        {
            return await _service.DeadlineSummary(inModel, false);
        }
    }
}
