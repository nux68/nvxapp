using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Tipi di richiesta dei ticket. Riservati al SuperUser (controllo nel servizio).
    [ApiController]
    [Route("api/[controller]")]
    public class TicketTypeController : NvxControllerBase
    {
        private readonly ITicketTypeService _ticketTypeService;

        public TicketTypeController(
                                    IHttpContextAccessor httpContextAccessor,
                                    ITicketTypeService ticketTypeService
          ) : base(httpContextAccessor)
        {
            _ticketTypeService = ticketTypeService;
        }

        [Authorize]
        [HttpPost]
        [Route("TicketTypeList")]
        public async Task<GenericResult<TicketTypeListOutModel>> TicketTypeList(GenericRequest<TicketTypeListInModel> inModel)
        {
            return await _ticketTypeService.TicketTypeList(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("TicketTypeGet")]
        public async Task<GenericResult<TicketTypeGetOutModel>> TicketTypeGet(GenericRequest<TicketTypeGetInModel> inModel)
        {
            return await _ticketTypeService.TicketTypeGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("TicketTypePut")]
        public async Task<GenericResult<TicketTypePutOutModel>> TicketTypePut(GenericRequest<TicketTypePutInModel> inModel)
        {
            return await _ticketTypeService.TicketTypePut(inModel, false);
        }
    }
}
