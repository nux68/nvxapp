using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnomalieService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Giornate da sistemare prima dell'export paghe di un mese (default il mese corrente, fino a ieri):
    // stesse regole della pagina "Anomalie presenze" (ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md, AT-AI.4).
    public class DaysToFixHandler : BaseCommandHandler
    {
        private const int MaxRighe = 15;

        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_AnomalieService _dip_AnomalieService;
        private readonly IGestionePresenzeAnomalie _anomalie;

        public DaysToFixHandler(IDip_AnomalieService dip_AnomalieService, IGestionePresenzeAnomalie anomalie)
        {
            _dip_AnomalieService = dip_AnomalieService;
            _anomalie = anomalie;

            _intentDefinition = new IntentDefinition
            {
                Name = "DaysToFix",
                Application = ApplicationType.AttendanceTracking,
                Category = AttendanceTrackingMessages.Get("ChatCategoryClockings"),
                CategoryOrder = 110,
                DisplayName = AttendanceTrackingMessages.Get("ChatDaysToFixDisplayName"),
                Usable4Role = new List<string> { RuoloHR },
                Description = AttendanceTrackingMessages.Get("ChatDaysToFixDescription"),
                Keywords = new() { "giornate da sistemare", "da sistemare", "prima delle paghe", "export paghe", "days to fix", "before payroll" },
                Examples = new() { "giornate da sistemare", "cosa devo sistemare prima delle paghe di ottobre?", "giornate da sistemare del mese scorso", "which days must be fixed before payroll?" },
                RequiresConfirmation = false,
                Pages = new() { "/attendanceanomalies" },
                Slots = BuildSlots(
                    SlotMese("month", AttendanceTrackingMessages.Get("ChatMonthQuestion"), AttendanceTrackingMessages.Get("ChatMonthLabel")))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (anno, mese) = ParseMese(slots.GetValueOrDefault("month"));
            var dal = new DateTime(anno, mese, 1);
            var al = dal.AddMonths(1).AddDays(-1);
            if (dal >= DateTime.Today)
                return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatDaysToFixFuture", Mese(anno, mese)));

            var dipendenti = await _dip_AnomalieService.DipendentiVisibili(new List<string>(), al < DateTime.Today ? al : DateTime.Today);
            if (dipendenti.Count == 0)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatNoEmployeesAvailable"));

            var r = _anomalie.Analizza(dipendenti, dal, al, DateTime.Today);
            var daSistemare = r.Giornate.Where(g => g.DaSistemare).ToList();
            if (daSistemare.Count == 0)
                return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatDaysToFixNone", Mese(anno, mese), r.GiornateDaVerificare));

            var sb = new StringBuilder();
            sb.AppendLine(AttendanceTrackingMessages.Get("ChatDaysToFixTitle", Mese(anno, mese), daSistemare.Count, r.DipendentiCoinvolti));
            foreach (var g in daSistemare.Take(MaxRighe))
                sb.AppendLine($"  • {Data(g.Data)} {g.NomeDipendente}: {string.Join("; ", g.Anomalie.Where(a => a.Gravita == AnomaliaGravita.Errore).Select(a => a.Descrizione))}");
            if (daSistemare.Count > MaxRighe)
                sb.AppendLine("  " + AttendanceTrackingMessages.Get("ChatListMore", daSistemare.Count - MaxRighe));
            if (r.GiornateDaVerificare > 0)
                sb.AppendLine(AttendanceTrackingMessages.Get("ChatDaysToFixWarnings", r.GiornateDaVerificare));

            return CommandResult.Ok(sb.ToString().TrimEnd());
        }
    }
}
