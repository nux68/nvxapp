using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;



namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands.Handlers
{
    // Gestisce la registrazione di un periodo ferie.
    // TODO: sostituire il placeholder con la chiamata API reale,
    //       aggiungere il controllo sovrapposizioni e il lookup dipendente.
    public class RegisterHolidayHandler : ICommandHandler
    {
        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name        = "RegisterHoliday",
            Description = "mette in ferie un dipendente per un periodo",
            Keywords    = new() { "ferie", "vacanza", "vacanze", "permesso", "assenza", "holiday" },
            Slots       = new()
            {
                new()
                {
                    Name              = "employeeName",
                    Type              = "string",
                    Required          = true,
                    PromptDescription = "nome e cognome della persona fisica presente nel testo (es. 'Marco Rossi', 'mario lalli'). Estrai il nome esattamente come appare nel testo.",
                    Question          = "Per quale dipendente?",
                    Label             = "Dipendente",
                    Validator         = v => v.Trim().Length >= 2 && v.Any(char.IsLetter) ? null
                        : SlotValidationResult.Failed(SlotValidationError.InvalidFormat,
                            $"'{v}' non sembra un nome valido. Inserire nome e cognome del dipendente.", "employeeName")
                },
                new()
                {
                    Name              = "startDate",
                    Type              = "yyyy-MM-dd",
                    Required          = true,
                    PromptDescription = $"data di inizio nel formato yyyy-MM-dd. Oggi \u00e8 {DateTime.Today:yyyy-MM-dd}.",
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
                    PromptDescription = $"data di fine nel formato yyyy-MM-dd. Oggi \u00e8 {DateTime.Today:yyyy-MM-dd}.",
                    Question          = "Fino a quale data?",
                    Label             = "Al",
                    Validator         = v => DateOnly.TryParse(v, out _) ? null
                        : SlotValidationResult.Failed(SlotValidationError.InvalidFormat,
                            $"'{v}' non \u00e8 una data valida. Usa il formato gg/mm/aaaa.", "endDate")
                }
            },
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

        public IntentDefinition IntentDefinition => _intentDefinition;

        public Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
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
