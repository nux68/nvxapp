using Microsoft.AspNetCore.SignalR;
using nvxapp.server.service.HubAI;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Request
{
    /*
     Avviso push (SignalR, evento "RequestNotifier") agli utenti interessati da un cambio delle richieste di un modulo
     (richiedente, approvatori, amministratori): i client ricalcolano i contatori del menu e ricaricano le pagine
     delle richieste (ANALISI_INFRASTRUCTURE_RICHIESTE.md, RICH-INF.1).
     Si chiama dopo il salvataggio: un errore di invio finisce nel log e non annulla l'operazione.
    */
    public class RequestNotifier : IRequestNotifier
    {
        public const string EventName = "RequestNotifier";

        private readonly IHubContext<SignalRHub> _hubContext;

        public RequestNotifier(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task NotifyAsync(IEnumerable<string?> userIds, string? key = null)
        {
            var users = userIds.Where(x => !string.IsNullOrEmpty(x)).Select(x => x!).Distinct().ToList();
            if (users.Count == 0)
                return;

            try
            {
                await _hubContext.Clients.Users(users).SendAsync(EventName, new RequestNotifierData { Key = key });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "RequestNotifier: invio non riuscito, richiesta {Key}", key);
            }
        }
    }

    public class RequestNotifierData
    {
        /// <summary>chiave della richiesta cambiata ("<Prefix>:<Id>"), se una sola</summary>
        public string? Key { get; set; }
    }

    public interface IRequestNotifier
    {
        Task NotifyAsync(IEnumerable<string?> userIds, string? key = null);
    }
}
