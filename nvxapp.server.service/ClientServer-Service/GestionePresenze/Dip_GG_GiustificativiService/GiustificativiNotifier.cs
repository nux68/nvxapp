using Microsoft.AspNetCore.SignalR;
using nvxapp.server.service.HubAI;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService
{
    /*
     Notifiche push (SignalR, evento "GiustificativiNotifier") agli utenti interessati da un giustificativo
     inserito, modificato o cancellato dal client (es. Calendario HR, chat): il dipendente e gli amministratori
     dell'azienda. Le pagine aperte con quel mese (es. piano ferie) si ricaricano.
     Si chiama dopo il salvataggio: un errore di invio finisce nel log e non annulla l'operazione.
    */
    public class GiustificativiNotifier : IGiustificativiNotifier
    {
        public const string EventName = "GiustificativiNotifier";

        private readonly IHubContext<SignalRHub> _hubContext;

        public GiustificativiNotifier(IHubContext<SignalRHub> hubContext)
        {
            _hubContext = hubContext;
        }

        public async Task GiustificativiNotifierAsync(IEnumerable<string> userIds, GiustificativiNotifierData data)
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
                Log.Warning(ex, "GiustificativiNotifier: invio non riuscito, rapporto {IdDip_RapportoLavoro} giorno {Data}", data.IdDip_RapportoLavoro, data.Data);
            }
        }
    }

    public class GiustificativiNotifierData
    {
        public int IdDip_RapportoLavoro { get; set; }
        // giorno del giustificativo (yyyy-MM-dd) e il suo mese (1-12)
        public string Data { get; set; } = string.Empty;
        public int Year { get; set; }
        public int Month { get; set; }
    }

    public interface IGiustificativiNotifier
    {
        Task GiustificativiNotifierAsync(IEnumerable<string> userIds, GiustificativiNotifierData data);
    }
}
