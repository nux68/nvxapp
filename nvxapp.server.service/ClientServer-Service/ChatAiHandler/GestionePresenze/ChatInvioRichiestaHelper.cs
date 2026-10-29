using Newtonsoft.Json;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Invio di una richiesta (giustificativo o timbratura) come dalle pagine "richiesta" del client:
    // stesso corpo JSON, date "dd/MM/yyyy". Dal dipendente resta in attesa di approvazione;
    // dall'HR (fromHR) è già approvata.
    public static class ChatInvioRichiestaHelper
    {
        // idAspNetUsers null = utente corrente. Ritorna null se inviata, altrimenti il messaggio d'errore del servizio.
        public static async Task<string?> Giustificativo(IDip_GG_RichiestaService service, string? idAspNetUsers, bool fromHR,
            DateTime dal, DateTime al, int idPar_Giustificativi, TimeSpan? ore, decimal? importo = null)
        {
            // stesso formato del client (request-justification-user-page): senza ore = giornata intera;
            // giustificativo a importo: l'importo per giorno, senza ore
            var dati = JsonConvert.SerializeObject(new
            {
                hhmm = ore != null ? $"{(int)ore.Value.TotalHours:D2}:{ore.Value.Minutes:D2}" : "00:00",
                allDay = ore == null && importo == null,
                idPar_Giustificativi,
                importo
            });
            return await Invia(service, idAspNetUsers, fromHR, TipoRichiesta.Giustificativo, dal, al, dati);
        }

        public static async Task<string?> Timbratura(IDip_GG_RichiestaService service, string? idAspNetUsers, bool fromHR,
            DateTime giorno, TimeOnly orario, int idAz_SubCommessaAttivita)
        {
            // stesso formato del client (request-clocking-user-page)
            var dati = JsonConvert.SerializeObject(new
            {
                hhmm = orario.ToString("HH:mm"),
                idAz_SubCommessaAttivita
            });
            return await Invia(service, idAspNetUsers, fromHR, TipoRichiesta.Timbratura, giorno, giorno, dati);
        }

        private static async Task<string?> Invia(IDip_GG_RichiestaService service, string? idAspNetUsers, bool fromHR,
            TipoRichiesta tipo, DateTime dal, DateTime al, string dati)
        {
            var res = await service.Send(new GenericRequest<Dip_GG_Richiesta_Send_InModel>
            {
                Data = new()
                {
                    IdAspNetUsers = idAspNetUsers,
                    FromHR = fromHR,
                    ExcludeRicalc = false,
                    Dip_GG_Richiesta = new Dip_GG_RichiestaModel
                    {
                        Id = 0,
                        IdDip_RapportoLavoro = 0,     // lo imposta il servizio dal dipendente
                        RichiestaTipo = tipo,
                        RichiestaStato = StatoRichiesta.Immessa,
                        Data = Data(dal),
                        DataA = Data(al),
                        Dati = dati
                    }
                }
            }, false);

            return res.Success ? null : string.Join(", ", res.Messages.Select(m => m.Text));
        }
    }
}
