using Microsoft.AspNetCore.SignalR;
using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.HubAI;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Notification
{
    /*
     Avviso push (SignalR, evento "DataChangedNotifier") quando un COMANDO ha modificato dei dati senza passare dalle
     pagine: oggi i comandi eseguiti da un assistente AI esterno tramite MCP (McpToolService), che scrivono mentre i
     browser degli utenti sono aperti su altre pagine (ANALISI_INFRASTRUCTURE_MCP.md, sezione 12quater, MCP-26).

     Le pagine che mostrano quei dati ascoltano l'evento (client: DataChangedNotifierService) e si ricaricano se
     l'applicativo e' il loro. L'evento non porta dati: dice solo "applicativo X, comando Y", e la pagina rilegge dal
     server con i permessi di chi la guarda.

     Destinatari: gli utenti attivi dell'azienda (una prenotazione o una timbratura cambia cio' che vedono colleghi e
     amministratori) piu' gli utenti indicati (chi ha eseguito il comando e chi ha fatto il login, se impersona).
     Si chiama dopo il salvataggio: un errore di invio finisce nel log e non annulla l'operazione.

     Non sostituisce gli avvisi specifici dei moduli (RequestNotifier, GiustificativiNotifier, fine del ricalcolo del
     cartellino...), che continuano a partire dai servizi: copre i casi in cui il servizio non avvisa nessuno perche'
     dal web chi salva ricarica da solo la propria pagina.
    */
    public class DataChangeNotifier : IDataChangeNotifier
    {
        public const string EventName = "DataChangedNotifier";

        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly IUserCompanyRepository _userCompanyRepository;

        public DataChangeNotifier(IHubContext<SignalRHub> hubContext,
                                  IUserCompanyRepository userCompanyRepository)
        {
            _hubContext = hubContext;
            _userCompanyRepository = userCompanyRepository;
        }

        public async Task NotifyAsync(int? idCompany, ApplicationType? application, string command, params string?[] userIds)
        {
            try
            {
                var users = userIds.Where(x => !string.IsNullOrEmpty(x)).Select(x => x!).ToList();
                if (idCompany != null)
                    users.AddRange(await _userCompanyRepository.FindAll(x => x.IdCompany == idCompany && !x.Disabled)
                                                               .Select(x => x.IdAspNetUsers).ToListAsync());
                users = users.Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                if (users.Count == 0)
                    return;

                await _hubContext.Clients.Users(users).SendAsync(EventName, new DataChangedNotifierData
                {
                    Application = application == null ? null : (int)application.Value,
                    Command = command
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "DataChangedNotifier: invio non riuscito, comando {Command}", command);
            }
        }
    }

    public class DataChangedNotifierData
    {
        /// <summary>applicativo dei dati modificati (ApplicationType); null = dati comuni</summary>
        public int? Application { get; set; }
        /// <summary>nome del comando eseguito (IntentDefinition.Name, es. "ClockNow")</summary>
        public string Command { get; set; } = string.Empty;
    }

    public interface IDataChangeNotifier
    {
        /// <summary>avvisa i browser degli utenti attivi dell'azienda (e degli utenti indicati) che un comando ha modificato dei dati</summary>
        Task NotifyAsync(int? idCompany, ApplicationType? application, string command, params string?[] userIds);
    }
}
