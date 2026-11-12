using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Richieste da approvare comuni a tutti gli applicativi (ANALISI_INFRASTRUCTURE_RICHIESTE.md)
    [ApiController]
    [Route("api/[controller]")]
    public class RequestController : NvxControllerBase
    {
        private readonly IRequestService _requestService;

        public RequestController(IHttpContextAccessor httpContextAccessor,
                                 IRequestService requestService) : base(httpContextAccessor)
        {
            _requestService = requestService;
        }

        [Authorize]
        [HttpPost]
        [Route("RequestCount")]
        public async Task<GenericResult<RequestCountOutModel>> RequestCount(GenericRequest<RequestCountInModel> inModel)
            => await _requestService.RequestCount(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("RequestList")]
        public async Task<GenericResult<RequestListOutModel>> RequestList(GenericRequest<RequestListInModel> inModel)
            => await _requestService.RequestList(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("RequestApprove")]
        public async Task<GenericResult<RequestActionOutModel>> RequestApprove(GenericRequest<RequestActionInModel> inModel)
            => await _requestService.RequestApprove(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("RequestReject")]
        public async Task<GenericResult<RequestActionOutModel>> RequestReject(GenericRequest<RequestActionInModel> inModel)
            => await _requestService.RequestReject(inModel, false);

        [Authorize]
        [HttpPost]
        [Route("RequestCancel")]
        public async Task<GenericResult<RequestActionOutModel>> RequestCancel(GenericRequest<RequestActionInModel> inModel)
            => await _requestService.RequestCancel(inModel, false);
    }
}
