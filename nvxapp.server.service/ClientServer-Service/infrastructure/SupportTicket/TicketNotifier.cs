using Microsoft.AspNetCore.SignalR;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.HubAI;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.SupportTicket
{
    /*
     Notifiche push (SignalR, evento "TicketNotifier") agli utenti interessati da un passaggio del ticket.
     Si chiama dopo il salvataggio: un errore di invio finisce nel log e non annulla il passaggio.
    */
    public class TicketNotifier : ITicketNotifier
    {
        public const string EventName = "TicketNotifier";

        private readonly IHubContext<SignalRHub> _hubContext;

        public TicketNotifier(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task TicketNotifierAsync(IEnumerable<string> userIds, TicketNotifierData data)
        {
            foreach (var userId in userIds.Where(x => !string.IsNullOrEmpty(x)).Distinct())
            {
                try
                {
                    await _hubContext.Clients.User(userId).SendAsync(EventName, data);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "TicketNotifier: invio non riuscito all'utente {UserId}, ticket {IdTicket}", userId, data.IdTicket);
                }
            }
        }
    }

    public class TicketNotifierData
    {
        public int IdTicket { get; set; }
        public string Oggetto { get; set; } = string.Empty;
        public TicketAction Action { get; set; }
        public TicketState State { get; set; }
    }

    public interface ITicketNotifier
    {
        Task TicketNotifierAsync(IEnumerable<string> userIds, TicketNotifierData data);
    }
}
