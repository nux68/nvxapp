using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket;
using nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;


namespace nvxapp.server.Controllers.Infrastructure
{
    // Risposta automatica AI dei tipi di richiesta per livello e nodo. Livello e permessi li decide il servizio.
    [ApiController]
    [Route("api/[controller]")]
    public class TicketTypeAiSettingController : NvxControllerBase
    {
        private readonly ITicketTypeAiSettingService _ticketTypeAiSettingService;

        public TicketTypeAiSettingController(
                                             IHttpContextAccessor httpContextAccessor,
                                             ITicketTypeAiSettingService ticketTypeAiSettingService
          ) : base(httpContextAccessor)
        {
            _ticketTypeAiSettingService = ticketTypeAiSettingService;
        }

        [Authorize]
        [HttpPost]
        [Route("TicketTypeAiSettingScope")]
        public async Task<GenericResult<TicketTypeAiSettingScopeOutModel>> TicketTypeAiSettingScope(GenericRequest<TicketTypeAiSettingScopeInModel> inModel)
        {
            return await _ticketTypeAiSettingService.TicketTypeAiSettingScope(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("TicketTypeAiSettingList")]
        public async Task<GenericResult<TicketTypeAiSettingListOutModel>> TicketTypeAiSettingList(GenericRequest<TicketTypeAiSettingListInModel> inModel)
        {
            return await _ticketTypeAiSettingService.TicketTypeAiSettingList(inModel, false);
        }

        [Authorize]
        [HttpPost]
        [Route("TicketTypeAiSettingPut")]
        public async Task<GenericResult<TicketTypeAiSettingPutOutModel>> TicketTypeAiSettingPut(GenericRequest<TicketTypeAiSettingPutInModel> inModel)
        {
            return await _ticketTypeAiSettingService.TicketTypeAiSettingPut(inModel, false);
        }
    }
}
