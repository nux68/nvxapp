using Microsoft.AspNetCore.SignalR;
using nvxapp.server.service.HubAI;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Notification
{
    /*
     Avvisi a comparsa per gli utenti collegati (SignalR, evento "UserNotifier"), usabili da ogni applicativo
     (es. promemoria delle attivita' del CRM). Il client mostra il titolo e il messaggio gia' tradotti e, se c'e'
     un indirizzo, il pulsante per aprirlo. Chi non e' collegato non riceve nulla (l'avviso non si conserva).
    */
    public class UserNotifier : IUserNotifier
    {
        public const string EventName = "UserNotifier";

        private readonly IHubContext<SignalRHub> _hubContext;

        public UserNotifier(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyAsync(IEnumerable<string> userIds, UserNotificationData data)
        {
            foreach (var userId in userIds.Where(x => !string.IsNullOrEmpty(x)).Distinct())
            {
                try
                {
                    await _hubContext.Clients.User(userId).SendAsync(EventName, data);
                }
                catch (Exception ex)
                {
                    Log.Warning(ex, "UserNotifier: invio non riuscito all'utente {UserId} ({Title})", userId, data.Title);
                }
            }
        }
    }

    public class UserNotificationData
    {
        public string Title { get; set; } = string.Empty;
        public string Message { get; set; } = string.Empty;
        // pagina da aprire (es. "/crmactivityedit") e stato da passarle (es. { id: 12 }); null = nessun pulsante
        public string? Url { get; set; }
        public object? State { get; set; }
    }

    public interface IUserNotifier
    {
        Task NotifyAsync(IEnumerable<string> userIds, UserNotificationData data);
    }
}
