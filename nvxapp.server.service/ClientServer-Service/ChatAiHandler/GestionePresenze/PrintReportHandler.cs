using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ReportService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ReportService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Avvia la stampa PDF di un report per un mese (default il corrente), di tutti i dipendenti o di uno.
    // Il PDF si genera in background come dal pulsante Report delle pagine HR: l'avanzamento e il download
    // compaiono nella barra in alto. ReportService limita i dipendenti in base al ruolo (HR tutti,
    // CompanyAdmin quelli dei suoi reparti). La quadratura mensile (per reparto) resta nella sua pagina.
    public class PrintReportHandler : BaseCommandDipeHandler
    {
        private static readonly Dictionary<string, ReportTipo> _tipi = new(StringComparer.OrdinalIgnoreCase)
        {
            ["piano ferie"] = ReportTipo.PianoFerie, ["ferie"] = ReportTipo.PianoFerie, ["vacation plan"] = ReportTipo.PianoFerie, ["holiday plan"] = ReportTipo.PianoFerie,
            ["attività per dipendente"] = ReportTipo.AttivitaPerDipendente, ["attivita per dipendente"] = ReportTipo.AttivitaPerDipendente,
            ["activities per employee"] = ReportTipo.AttivitaPerDipendente, ["activities by employee"] = ReportTipo.AttivitaPerDipendente,
            ["attività"] = ReportTipo.AttivitaTotali, ["attivita"] = ReportTipo.AttivitaTotali, ["statistiche attività"] = ReportTipo.AttivitaTotali,
            ["attività totali"] = ReportTipo.AttivitaTotali, ["activities"] = ReportTipo.AttivitaTotali, ["activity"] = ReportTipo.AttivitaTotali,
            ["note spese per tipo"] = ReportTipo.NotaSpesePerTipo, ["spese per tipo"] = ReportTipo.NotaSpesePerTipo, ["expenses by type"] = ReportTipo.NotaSpesePerTipo,
            ["note spese"] = ReportTipo.NotaSpesePerDipendente, ["nota spese"] = ReportTipo.NotaSpesePerDipendente, ["spese"] = ReportTipo.NotaSpesePerDipendente,
            ["expenses"] = ReportTipo.NotaSpesePerDipendente, ["expense report"] = ReportTipo.NotaSpesePerDipendente,
        };

        private readonly IntentDefinition _intentDefinition;
        private readonly IReportService _reportService;

        public PrintReportHandler(IDip_AnagraficaService dip_AnagraficaService,
                                  IReportService reportService,
                                  IChatUserContext chatUserContext
                                  ) : base(dip_AnagraficaService, chatUserContext)
        {
            _reportService = reportService;

            _intentDefinition = new IntentDefinition
            {
                Name = "PrintReport",
                DisplayName = AttendanceTrackingMessages.Get("ChatPrintReportDisplayName"),
                Usable4Role = new List<string> { RuoloHR, RuoloAdmin },
                Description = AttendanceTrackingMessages.Get("ChatPrintReportDescription"),
                Keywords = new() { "stampa", "stampami", "pdf", "report", "print" },
                Examples = new() { "stampa il piano ferie di novembre", "fammi il pdf delle attività di ottobre", "stampa le note spese di Rossi del mese scorso", "print the vacation plan for December" },
                RequiresConfirmation = false,
                Pages = new() { "/vacationplan", "/activitystatistics", "/poweradmintimesheet" },
                Slots = BuildSlots(
                    new SlotDefinition
                    {
                        Name = "report",
                        Type = "string",
                        Required = true,
                        PromptDescription = "report richiesto come detto dall'utente: piano ferie, attività (totali), attività per dipendente, note spese (per dipendente), note spese per tipo",
                        Question = AttendanceTrackingMessages.Get("ChatPrintReportQuestion"),
                        Label = AttendanceTrackingMessages.Get("ChatPrintReportLabel"),
                        Normalizer = NormalizzaTipo,
                        Validator = v => Enum.TryParse<ReportTipo>(v, out var t) && t != ReportTipo.QuadraturaMensile
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, AttendanceTrackingMessages.Get("ChatPrintReportQuestion"), "report")
                    },
                    SlotMese("month", AttendanceTrackingMessages.Get("ChatMonthQuestion"), AttendanceTrackingMessages.Get("ChatMonthLabel")),
                    Facoltativo(EmployeeNameSlot, string.Empty))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        // "il piano ferie" -> "PianoFerie": vince l'espressione più lunga contenuta nel testo
        private static string? NormalizzaTipo(string valore)
        {
            var testo = $" {valore.Trim().ToLowerInvariant()} ";
            return _tipi.Where(kv => testo.Contains($" {kv.Key} ") || testo.Contains($" {kv.Key}"))
                        .OrderByDescending(kv => kv.Key.Length)
                        .Select(kv => kv.Value.ToString())
                        .FirstOrDefault();
        }

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            if (!Enum.TryParse<ReportTipo>(slots.GetValueOrDefault("report"), out var tipo))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatPrintReportQuestion"));
            var (anno, mese) = ParseMese(slots.GetValueOrDefault("month"));

            // un dipendente oppure tutti (il servizio tiene solo quelli che l'utente può vedere)
            List<string> utenti;
            if (slots.TryGetValue("employeeName", out var nome) && !string.IsNullOrWhiteSpace(nome))
            {
                var dipendente = await RisolviDipendente(nome, _dip_AnagraficaService, _chatUserContext);
                if (dipendente == null)
                    return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmployeeNotFoundRetry", nome));
                utenti = new List<string> { dipendente.IdAspNetUsers };
            }
            else
                utenti = (await Dipendenti(_dip_AnagraficaService)).Select(d => d.IdAspNetUsers).ToList();

            var res = await _reportService.ReportExportPdf(new GenericRequest<Report_InModel>
            {
                Data = new() { Report = new ReportModel { Tipo = tipo, Year = anno, Month = mese, SelectedUserId = utenti } }
            }, false);
            if (!res.Success || res.Data == null || string.IsNullOrEmpty(res.Data.JobId))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatPrintReportError", string.Join(", ", res.Messages.Select(m => m.Text))));

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatPrintReportStarted",
                AttendanceTrackingMessages.Get($"ChatReport{tipo}"), Mese(anno, mese)));
        }
    }
}
