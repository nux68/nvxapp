using Microsoft.AspNetCore.SignalR;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Request;
using nvxapp.server.service.HubAI;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService
{
    /*
     Notifiche push (SignalR, evento "RichiestaNotifier") agli utenti interessati da un cambio delle richieste
     (dipendente, approvatori, amministratori dell'azienda): i client ricalcolano i contatori del menu.
     Manda anche l'evento comune "RequestNotifier" (IRequestNotifier, ANALISI_INFRASTRUCTURE_RICHIESTE.md): badge e pagine
     delle richieste comuni (Infrastructure) si aggiornano come quelli di Presenze.
     Si chiama dopo il salvataggio: un errore di invio finisce nel log e non annulla l'operazione.
    */
    public class RichiestaNotifier : IRichiestaNotifier
    {
        public const string EventName = "RichiestaNotifier";

        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IRequestNotifier _requestNotifier;

        public RichiestaNotifier(IHubContext<SignalRHub> hubContext, IRequestNotifier requestNotifier)
        {
            _hubContext = hubContext;
            _requestNotifier = requestNotifier;
        }

        public async Task RichiestaNotifierAsync(IEnumerable<string> userIds, RichiestaNotifierData data)
        {
            var users = userIds.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
            if (users.Count == 0)
                return;

            try
            {
                await _hubContext.Clients.Users(users).SendAsync(EventName, data);
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "RichiestaNotifier: invio non riuscito, richieste {IdDip_GG_Richiesta}", data.IdDip_GG_Richiesta);
            }

            await _requestNotifier.NotifyAsync(users, data.IdDip_GG_Richiesta.Count == 1 ? data.IdDip_GG_Richiesta[0].ToString() : null);
        }
    }

    public class RichiestaNotifierData
    {
        public List<int> IdDip_GG_Richiesta { get; set; } = new List<int>();
    }

    public interface IRichiestaNotifier
    {
        Task RichiestaNotifierAsync(IEnumerable<string> userIds, RichiestaNotifierData data);
    }
}
