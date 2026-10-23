using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Ticket di assistenza: elenchi, dettaglio, apertura, passaggi e download degli allegati.
    // Visibilita' e permessi sono controllati nel servizio.
    [ApiController]
    [Route("api/[controller]")]
    public class TicketController : NvxControllerBase
    {
        private readonly ITicketService _ticketService;

        public TicketController(
                                    IHttpContextAccessor httpContextAccessor,
                                    ITicketService ticketService
          ) : base(httpContextAccessor)
        {
            _ticketService = ticketService;
        }

        [Authorize]
        [HttpPost]
        [Route("TicketList")]
        public async Task<GenericResult<TicketListOutModel>> TicketList(GenericRequest<TicketListInModel> inModel)
        {
            return await _ticketService.TicketList(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("TicketCount")]
        public async Task<GenericResult<TicketCountOutModel>> TicketCount(GenericRequest<TicketCountInModel> inModel)
        {
            return await _ticketService.TicketCount(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("TicketGet")]
        public async Task<GenericResult<TicketGetOutModel>> TicketGet(GenericRequest<TicketGetInModel> inModel)
        {
            return await _ticketService.TicketGet(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("TicketOpenInfo")]
        public async Task<GenericResult<TicketOpenInfoOutModel>> TicketOpenInfo(GenericRequest<TicketOpenInfoInModel> inModel)
        {
            return await _ticketService.TicketOpenInfo(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("TicketOpen")]
        public async Task<GenericResult<TicketOpenOutModel>> TicketOpen(GenericRequest<TicketOpenInModel> inModel)
        {
            return await _ticketService.TicketOpen(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("TicketExecute")]
        public async Task<GenericResult<TicketActionOutModel>> TicketExecute(GenericRequest<TicketActionInModel> inModel)
        {
            return await _ticketService.TicketExecute(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("TicketAttachGet")]
        public async Task<GenericResult<TicketAttachGetOutModel>> TicketAttachGet(GenericRequest<TicketAttachGetInModel> inModel)
        {
            return await _ticketService.TicketAttachGet(inModel, false);
        }
    }
}
