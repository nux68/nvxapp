using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Ricerca di una timbratura di un dipendente per giorno e orario (correzione ed eliminazione dalla chat).
    public static class ChatTimbratureHelper
    {
        // Timbratura valida del giorno all'orario indicato ("HH:mm", si confronta l'orario attuale della timbratura).
        // Se non c'è, Errore spiega quali timbrature ha il dipendente in quel giorno.
        public static async Task<(Dip_GG_TimbraturaModel? Timbratura, string? Errore)> Trova(
            IDip_GG_TimbraturaService service, Dip_AnagraficaModel dipendente, DateTime giorno, string orario)
        {
            var res = await service.Dip_GG_Timbratura_Get_4Calculation(
                new GenericRequest<Dip_GG_Timbratura_Get_4Calculation_InModel>
                {
                    Data = new() { UsersId = new List<string> { dipendente.IdAspNetUsers }, Dal = giorno, Al = giorno }
                }, true);
            if (!res.Success || res.Data == null)
                return (null, AttendanceTrackingMessages.Get("ChatDataReadError", string.Join(", ", res.Messages.Select(m => m.Text))));

            var valide = res.Data.Dip_GG_Timbratura.Where(t => StatoValido(t.RichiestaStato)).OrderBy(t => t.Timbratura).ToList();
            var trovata = valide.FirstOrDefault(t => t.Timbratura.ToString("HH:mm") == orario);
            if (trovata != null)
                return (trovata, null);

            var delGiorno = valide.Count > 0
                ? string.Join(", ", valide.Select(t => t.Timbratura.ToString("HH:mm")))
                : AttendanceTrackingMessages.Get("ChatNobody");
            return (null, AttendanceTrackingMessages.Get("ChatClockingNotFound", Nome(dipendente), orario, Data(giorno), delGiorno));
        }
    }
}
