using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands.Handlers
{
    // Gestisce la registrazione di un periodo ferie.
    // TODO: sostituire il placeholder con la chiamata API reale,
    //       aggiungere il controllo sovrapposizioni e il lookup dipendente.
    public class RegisterHolidayHandler : BaseCommandDipeHandler
    {
        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name        = "RegisterHoliday",
            DisplayName = "Ferie",
            Description = "mette in ferie un dipendente per un periodo",
            Keywords    = new() { "ferie", "vacanza", "vacanze", "permesso", "assenza", "holiday" },
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
                    Required          = true,
                    PromptDescription = "data di fine nel formato yyyy-MM-dd. Se dice 'oggi' normalizza alla data odierna.",
                    Question          = "Fino a quale data?",
                    Label             = "Al",
                    Validator         = v => DateOnly.TryParse(v, out _) ? null
                        : SlotValidationResult.Failed(SlotValidationError.InvalidFormat,
                            $"'{v}' non \u00e8 una data valida. Usa il formato gg/mm/aaaa.", "endDate")
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
            var employeeName = slots.GetValueOrDefault("employeeName", "-");
            var startDate    = slots.GetValueOrDefault("startDate", "-");
            var endDate      = slots.GetValueOrDefault("endDate", "-");

            int days = 0;
            if (DateOnly.TryParse(startDate, out var s) && DateOnly.TryParse(endDate, out var e))
                days = e.DayNumber - s.DayNumber + 1;

            var daysLabel = days > 0 ? $" ({days} giorni)" : string.Empty;

            return Task.FromResult(CommandResult.Ok(
                $"Ferie registrate: {employeeName} dal {startDate} al {endDate}{daysLabel}."));
        }
    }
}
