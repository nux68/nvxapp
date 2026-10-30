using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Piano ferie di un periodo (default: da oggi per una settimana): chi è assente per i giustificativi
    // visibili nel piano ferie (Par_Giustificativi.VisualizzaInPianoFerie), come la pagina Piano ferie.
    // I giorni consecutivi dello stesso giustificativo si mostrano come un intervallo.
    public class VacationPlanQueryHandler : BaseCommandHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_AnagraficaService _dip_AnagraficaService;
        private readonly IDip_GG_GiustificativiService _dip_GG_GiustificativiService;
        private readonly IPar_GiustificativiService _par_GiustificativiService;

        public VacationPlanQueryHandler(IDip_AnagraficaService dip_AnagraficaService,
                                        IDip_GG_GiustificativiService dip_GG_GiustificativiService,
                                        IPar_GiustificativiService par_GiustificativiService)
        {
            _dip_AnagraficaService = dip_AnagraficaService;
            _dip_GG_GiustificativiService = dip_GG_GiustificativiService;
            _par_GiustificativiService = par_GiustificativiService;

            _intentDefinition = new IntentDefinition
            {
                Name = "VacationPlanQuery",
                Category = AttendanceTrackingMessages.Get("ChatCategoryLeave"),
                CategoryOrder = 300,
                DisplayName = AttendanceTrackingMessages.Get("ChatVacationPlanDisplayName"),
                Usable4Role = new List<string> { RuoloHR },
                Description = AttendanceTrackingMessages.Get("ChatVacationPlanDescription"),
                Keywords = new() { "chi è in ferie", "chi sarà in ferie", "piano ferie", "chi è assente", "who is on holiday", "vacation plan" },
                Examples = new() { "chi è in ferie la prossima settimana?", "piano ferie di ottobre", "chi sarà assente dal 3 al 7 novembre?", "who is on holiday next week?" },
                RequiresConfirmation = false,
                Pages = new() { "/vacationplan" },
                Slots = BuildSlots(
                    SlotData("from", false, "oggi", AttendanceTrackingMessages.Get("ChatPeriodFromQuestion"), AttendanceTrackingMessages.Get("ChatFromLabel"),
                        SignificatoDal + "; per una settimana copia l'espressione (es. 'la prossima settimana', 'questa settimana')"),
                    SlotData("to", false, string.Empty, AttendanceTrackingMessages.Get("ChatPeriodToQuestion"), AttendanceTrackingMessages.Get("ChatToLabel"),
                        SignificatoAl + "; se l'utente indica una settimana ('la prossima settimana') ometti lo slot: il periodo dura 7 giorni"))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var dal = ParseData(slots.GetValueOrDefault("from", "oggi"));
            var al = slots.TryGetValue("to", out var a) ? ParseData(a) : dal.AddDays(6);
            if (al < dal) al = dal;

            var dipendenti = await Dipendenti(_dip_AnagraficaService);
            var perRapporto = PerRapporto(dipendenti);
            var nelPiano = (await Giustificativi(_par_GiustificativiService))
                .Where(g => g.VisualizzaInPianoFerie)
                .ToDictionary(g => g.Id, g => g.Descrizione ?? g.Codice ?? string.Empty);

            var res = await _dip_GG_GiustificativiService.Dip_GG_Giustificativi_Get_4Calculation(
                new GenericRequest<Dip_GG_Giustificativi_Get_4Calculation_InModel>
                {
                    Data = new() { UsersId = dipendenti.Select(d => d.IdAspNetUsers).ToList(), Dal = dal, Al = al }
                }, true);
            if (!res.Success || res.Data == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatDataReadError", string.Join(", ", res.Messages.Select(m => m.Text))));

            var periodo = $"{Data(dal)} - {Data(al)}";

            // per dipendente e giustificativo, giorni consecutivi raggruppati in intervalli
            var righe = res.Data.Dip_GG_Giustificativi
                .Where(g => StatoValido(g.RichiestaStato) && nelPiano.ContainsKey(g.IdPar_Giustificativi) && perRapporto.ContainsKey(g.IdDip_RapportoLavoro))
                .GroupBy(g => (Dipendente: Nome(perRapporto[g.IdDip_RapportoLavoro]), Tipo: nelPiano[g.IdPar_Giustificativi]))
                .OrderBy(g => g.Key.Dipendente)
                .Select(g => $"{g.Key.Dipendente}: {g.Key.Tipo} {string.Join(", ", Intervalli(g.Select(x => x.Data.Date)))}")
                .ToList();

            if (righe.Count == 0)
                return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatVacationPlanNone", periodo));

            var sb = new StringBuilder();
            sb.AppendLine(AttendanceTrackingMessages.Get("ChatVacationPlanTitle", periodo));
            foreach (var riga in righe.Take(20))
                sb.AppendLine("  • " + riga);
            if (righe.Count > 20)
                sb.AppendLine("  " + AttendanceTrackingMessages.Get("ChatListMore", righe.Count - 20));
            return CommandResult.Ok(sb.ToString().TrimEnd());
        }

        // giorni -> "03/11/2026 - 07/11/2026", "10/11/2026"
        private static IEnumerable<string> Intervalli(IEnumerable<DateTime> giorni)
        {
            var ordinati = giorni.Distinct().OrderBy(d => d).ToList();
            for (int i = 0; i < ordinati.Count;)
            {
                int j = i;
                while (j + 1 < ordinati.Count && ordinati[j + 1] == ordinati[j].AddDays(1)) j++;
                yield return i == j ? Data(ordinati[i]) : $"{Data(ordinati[i])} - {Data(ordinati[j])}";
                i = j + 1;
            }
        }
    }
}
