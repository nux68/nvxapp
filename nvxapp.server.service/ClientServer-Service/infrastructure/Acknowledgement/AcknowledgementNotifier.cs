using Microsoft.AspNetCore.SignalR;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Notification;
using nvxapp.server.service.HubAI;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Acknowledgement
{
    /*
     Avviso push (SignalR, evento "AcknowledgementNotifier") di una presa visione confermata, dalla pagina "Prese visione"
     o da un canale (es. firma del documentale): agli amministratori dell'azienda e al destinatario. Le pagine che mostrano
     lo stato delle prese visione di un modulo (es. accordi di lavoro agile, fonte "at.accordo-sw") si ricaricano quando
     arriva un avviso della loro fonte. Si chiama dopo il salvataggio: un errore di invio finisce nel log.
    */
    public class AcknowledgementNotifier : IAcknowledgementNotifier
    {
        public const string EventName = "AcknowledgementNotifier";

        private readonly IHubContext<SignalRHub> _hubContext;
        private readonly ICompanyRecipients _companyRecipients;

        public AcknowledgementNotifier(IHubContext<SignalRHub> hubContext, ICompanyRecipients companyRecipients)
        {
            _hubContext = hubContext;
            _companyRecipients = companyRecipients;
        }

        public async Task ConfermataAsync(PresaVisione riga)
        {
            try
            {
                var destinatari = (await _companyRecipients.AdminsAsync(riga.IdCompany))
                                  .Append(riga.IdAspNetUsers)
                                  .Where(x => !string.IsNullOrEmpty(x)).Distinct().ToList();
                if (destinatari.Count == 0)
                    return;
                await _hubContext.Clients.Users(destinatari).SendAsync(EventName, new AcknowledgementNotifierData
                {
                    IdCompany = riga.IdCompany,
                    Application = riga.Application,
                    Fonte = riga.Fonte,
                    IdRiferimento = riga.IdRiferimento,
                    IdAspNetUsers = riga.IdAspNetUsers,
                    DataConferma = riga.DataConferma
                });
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "AcknowledgementNotifier: avviso non inviato per la presa visione {Id} ({Fonte})", riga.Id, riga.Fonte);
            }
        }
    }

    public class AcknowledgementNotifierData
    {
        public int IdCompany { get; set; }
        public int Application { get; set; }
        public string Fonte { get; set; } = string.Empty;
        public int? IdRiferimento { get; set; }
        public string IdAspNetUsers { get; set; } = string.Empty;
        public DateTime? DataConferma { get; set; }
    }

    public interface IAcknowledgementNotifier
    {
        /// <summary>avviso della presa visione appena confermata (amministratori dell'azienda e destinatario)</summary>
        Task ConfermataAsync(PresaVisione riga);
    }
}
