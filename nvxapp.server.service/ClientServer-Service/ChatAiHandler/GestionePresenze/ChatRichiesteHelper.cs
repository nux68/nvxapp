using Newtonsoft.Json.Linq;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Richieste dei dipendenti per i comandi della chat: lettura di quelle in attesa e descrizione leggibile.
    public static class ChatRichiesteHelper
    {
        // richieste dell'azienda con la fase corrente (revoca se presente, altrimenti richiesta) ancora da decidere
        public static async Task<List<Dip_GG_RichiestaModel>> InAttesa(IDip_GG_RichiestaService service)
        {
            var res = await service.GetAll4Admin(new GenericRequest<Dip_GG_Richiesta_GetAll4Admin_InModel>
            {
                Data = new() { Filtro = Dip_GG_Richiesta_Filtro.DaApprovare }
            }, true);
            return res.Success && res.Data != null ? res.Data.Dip_GG_Richiesta : new();
        }

        // data della richiesta (stringa del modello) -> DateTime
        public static DateTime? Data(string? valore)
        {
            if (string.IsNullOrWhiteSpace(valore)) return null;
            if (DateTime.TryParseExact(valore, new[] { "dd/MM/yyyy", "dd/MM/yyyy HH:mm:ss", "yyyy-MM-dd", "yyyy-MM-ddTHH:mm:ss" },
                    CultureInfo.InvariantCulture, DateTimeStyles.None, out var d))
                return d.Date;
            return DateTime.TryParse(valore, CultureInfo.InvariantCulture, DateTimeStyles.None, out d) ? d.Date : null;
        }

        // la richiesta riguarda il giorno indicato (Data..DataA)
        public static bool Comprende(Dip_GG_RichiestaModel r, DateTime giorno)
        {
            var dal = Data(r.Data);
            var al = Data(r.DataA) ?? dal;
            return dal != null && giorno >= dal.Value && giorno <= (al ?? dal).Value;
        }

        // "Rossi Mario: Ferie dal 03/11/2026 al 07/11/2026, giornata intera"
        public static string Descrivi(Dip_GG_RichiestaModel r, Dip_AnagraficaModel? dipendente, IReadOnlyDictionary<int, string> nomiGiustificativi)
        {
            var nome = dipendente != null ? Nome(dipendente) : "?";
            var dal = Data(r.Data);
            var al = Data(r.DataA) ?? dal;
            var dalTesto = dal != null ? ChatPresenzeHelper.Data(dal.Value) : r.Data;
            var alTesto = al != null ? ChatPresenzeHelper.Data(al.Value) : r.DataA;

            JObject? dati = null;
            try { dati = string.IsNullOrEmpty(r.Dati) ? null : JObject.Parse(r.Dati); } catch { }
            string? Campo(string nomeCampo) => dati?.GetValue(nomeCampo, StringComparison.OrdinalIgnoreCase)?.ToString();

            switch (r.RichiestaTipo)
            {
                case TipoRichiesta.Timbratura:
                    return AttendanceTrackingMessages.Get("ChatRequestClocking", nome, dalTesto, Campo("hhmm") ?? "?");

                case TipoRichiesta.Giustificativo:
                    var tipo = int.TryParse(Campo("idPar_Giustificativi"), out var id) ? nomiGiustificativi.GetValueOrDefault(id, "?") : "?";
                    // giustificativo a importo: l'importo al posto delle ore
                    var ore = decimal.TryParse(Campo("importo"), System.Globalization.NumberStyles.Number, System.Globalization.CultureInfo.InvariantCulture, out var importo) && importo > 0
                        ? ImportoSlotHelper.Testo(importo)
                        : bool.TryParse(Campo("allDay"), out var allDay) && allDay
                            ? AttendanceTrackingMessages.Get("ChatRequestAllDay")
                            : AttendanceTrackingMessages.Get("ChatRequestHours", Campo("hhmm") ?? "?");
                    return AttendanceTrackingMessages.Get("ChatRequestJustification", nome, tipo, dalTesto, alTesto, ore);

                default:
                    return AttendanceTrackingMessages.Get("ChatRequestOther", nome, r.RichiestaTipo.ToString(), dalTesto);
            }
        }
    }
}
