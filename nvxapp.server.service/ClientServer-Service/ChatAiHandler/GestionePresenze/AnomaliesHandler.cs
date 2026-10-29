using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Anomalie di un giorno (default ieri): dipendenti con un numero dispari di timbrature valide,
    // cioè manca un'entrata o un'uscita. Per oggi indica chi è ancora in servizio.
    public class AnomaliesHandler : BaseCommandHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_AnagraficaService _dip_AnagraficaService;
        private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;

        public AnomaliesHandler(IDip_AnagraficaService dip_AnagraficaService,
                                IDip_GG_TimbraturaService dip_GG_TimbraturaService)
        {
            _dip_AnagraficaService = dip_AnagraficaService;
            _dip_GG_TimbraturaService = dip_GG_TimbraturaService;

            _intentDefinition = new IntentDefinition
            {
                Name = "ClockingAnomalies",
                Category = AttendanceTrackingMessages.Get("ChatCategoryClockings"),
                CategoryOrder = 100,
                DisplayName = AttendanceTrackingMessages.Get("ChatAnomaliesDisplayName"),
                Usable4Role = new List<string> { RuoloHR },
                Description = AttendanceTrackingMessages.Get("ChatAnomaliesDescription"),
                Keywords = new() { "anomalie", "anomalia", "timbrature mancanti", "manca la timbratura", "anomalies", "missing clockings" },
                Examples = new() { "ci sono anomalie ieri?", "chi ha dimenticato di timbrare l'uscita lunedì?", "timbrature mancanti di venerdì", "any missing clockings yesterday?" },
                RequiresConfirmation = false,
                Pages = new() { "/poweradmintimesheet", "/presentstaff" },
                Slots = BuildSlots(
                    SlotData("date", false, "ieri", AttendanceTrackingMessages.Get("ChatDateQuestion"), AttendanceTrackingMessages.Get("ChatDateLabel")))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var giorno = ParseData(slots.GetValueOrDefault("date", "ieri"));

            var dipendenti = await Dipendenti(_dip_AnagraficaService);
            if (dipendenti.Count == 0)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatNoEmployeesAvailable"));
            var perRapporto = PerRapporto(dipendenti);

            var res = await _dip_GG_TimbraturaService.Dip_GG_Timbratura_Get_4Calculation(
                new GenericRequest<Dip_GG_Timbratura_Get_4Calculation_InModel>
                {
                    Data = new() { UsersId = dipendenti.Select(d => d.IdAspNetUsers).ToList(), Dal = giorno, Al = giorno }
                }, true);
            if (!res.Success || res.Data == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatDataReadError", string.Join(", ", res.Messages.Select(m => m.Text))));

            var dispari = res.Data.Dip_GG_Timbratura
                .Where(t => StatoValido(t.RichiestaStato))
                .GroupBy(t => t.IdDip_RapportoLavoro)
                .Where(g => g.Count() % 2 == 1 && perRapporto.ContainsKey(g.Key))
                .Select(g => $"{Nome(perRapporto[g.Key])}: {string.Join(", ", g.OrderBy(t => t.Timbratura).Select(t => t.Timbratura.ToString("HH:mm")))}")
                .OrderBy(s => s)
                .ToList();

            if (dispari.Count == 0)
                return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatAnomaliesNone", Data(giorno)));

            var sb = new StringBuilder();
            sb.AppendLine(AttendanceTrackingMessages.Get(giorno == DateTime.Today ? "ChatAnomaliesTodayTitle" : "ChatAnomaliesTitle", Data(giorno), dispari.Count));
            foreach (var riga in dispari.Take(20))
                sb.AppendLine("  • " + riga);
            if (dispari.Count > 20)
                sb.AppendLine("  " + AttendanceTrackingMessages.Get("ChatListMore", dispari.Count - 20));

            return CommandResult.Ok(sb.ToString().TrimEnd());
        }
    }
}
