using nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands.Handlers
{
    // Gestisce la registrazione di un periodo di malattia.
    // TODO: sostituire il placeholder con la chiamata API reale
    //       e il lookup del dipendente per nome -> EmployeeId.
    public class RegisterSickLeaveHandler : BaseCommandDipeHandler
    {
        public RegisterSickLeaveHandler(IDip_AnagraficaService dip_AnagraficaService):base(dip_AnagraficaService)
        {
        }

        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name        = "RegisterSickLeave",
            DisplayName = "Malattia",
            Description = "registra una malattia per un dipendente",
            Keywords    = new() { "malattia", "malato", "sick", "certificato", "medico", "influenza", "congedo" },
            Slots       = BuildSlots(
                EmployeeNameSlot,
                new()
                {
                    Name              = "startDate",
                    Type              = "yyyy-MM-dd",
                    Required          = true,
                    PromptDescription = "data di inizio nel formato yyyy-MM-dd. Se dice 'oggi' normalizza alla data odierna.",
                    Question          = "Da quale data?",
                    Label             = "Dal",
                    Validator         = v => DateOnly.TryParse(v, out _) ? null
                        : SlotValidationResult.Failed(SlotValidationError.InvalidFormat,
                            $"'{v}' non \u00e8 una data valida. Usa il formato gg/mm/aaaa.", "startDate")
                },
                new()
                {
                    Name              = "endDate",
                    Type              = "yyyy-MM-dd",
                    Required          = false,
                    PromptDescription = "data di fine nel formato yyyy-MM-dd. Se dice 'oggi' normalizza alla data odierna.",
                    Question          = "Fino a quale data?",
                    Label             = "Al",
                    Validator         = v => DateOnly.TryParse(v, out _) ? null
                        : SlotValidationResult.Failed(SlotValidationError.InvalidFormat,
                            $"'{v}' non \u00e8 una data valida. Usa il formato gg/mm/aaaa.", "endDate")
                },
                new()
                {
                    Name              = "certificateNumber",
                    Type              = "string",
                    Required          = false,
                    PromptDescription = "codice o numero del certificato medico",
                    Question          = "Hai il numero del certificato medico? (premi invio per saltare)",
                    Label             = "Certificato"
                }
            ),
            CrossValidator = slots =>
            {
                if (slots.TryGetValue("startDate", out var s) &&
                    slots.TryGetValue("endDate",   out var e) &&
                    DateOnly.TryParse(s, out var start) &&
                    DateOnly.TryParse(e, out var end) &&
                    start > end)
                    return SlotValidationResult.Failed(SlotValidationError.BusinessRuleViolation,
                        "La data di inizio non pu\u00f2 essere successiva alla data di fine.", "startDate");
                return null;
            }
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var employeeName      = slots.GetValueOrDefault("employeeName", "-");
            var startDate         = slots.GetValueOrDefault("startDate", "-");
            var endDate           = slots.GetValueOrDefault("endDate", string.Empty);
            var certificateNumber = slots.GetValueOrDefault("certificateNumber", string.Empty);

            var endPart  = !string.IsNullOrEmpty(endDate)  ? $" al {endDate}"                    : string.Empty;
            var certPart = !string.IsNullOrEmpty(certificateNumber) ? $" (certificato n. {certificateNumber})" : string.Empty;

            return Task.FromResult(CommandResult.Ok(
                $"Malattia registrata: {employeeName} dal {startDate}{endPart}{certPart}."));
        }
    }
}
