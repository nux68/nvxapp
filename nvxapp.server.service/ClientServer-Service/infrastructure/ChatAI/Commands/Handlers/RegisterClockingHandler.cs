using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;



namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands.Handlers
{
    // Gestisce la registrazione di una timbratura entrata/uscita.
    // TODO: sostituire il placeholder con la chiamata API reale
    //       e il lookup del dipendente per nome ? EmployeeId.
    public class RegisterClockingHandler : ICommandHandler
    {
        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name        = "RegisterClocking",
            DisplayName = "Timbratura",
            Description = "registra una timbratura di entrata o uscita",
            Keywords    = new() { "timbratura", "timbra", "entrata", "uscita", "orario", "clocking", "timbrare" },
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
                    Name              = "time",
                    Type              = "HH:mm",
                    Required          = true,
                    PromptDescription = $"orario nel formato HH:mm. Se l'utente dice 'alle 9' restituisci '09:00'.",
                    Question          = "A che orario? (es. 09:00)",
                    Label             = "Orario",
                    Validator         = v => TimeOnly.TryParse(v, out _) ? null
                        : SlotValidationResult.Failed(SlotValidationError.InvalidFormat,
                            $"'{v}' non \u00e8 un orario valido. Usa il formato HH:mm (es. 09:00).", "time")
                },
                new()
                {
                    Name              = "date",
                    Type              = "yyyy-MM-dd",
                    Required          = false,
                    Default           = "oggi",
                    PromptDescription = $"data nel formato yyyy-MM-dd. Oggi \u00e8 {DateTime.Today:yyyy-MM-dd}. Se dice 'oggi' restituisci '{DateTime.Today:yyyy-MM-dd}'.",
                    Question          = "Per quale data?",
                    Label             = "Data",
                    Validator         = v => DateOnly.TryParse(v, out _) ? null
                        : SlotValidationResult.Failed(SlotValidationError.InvalidFormat,
                            $"'{v}' non \u00e8 una data valida. Usa il formato gg/mm/aaaa.", "date")
                },
                new()
                {
                    Name              = "direction",
                    Type              = "IN/OUT",
                    Required          = false,
                    Default           = "IN",
                    PromptDescription = "valore IN oppure OUT. Se dice 'entrata' restituisci IN, se dice 'uscita' restituisci OUT.",
                    Question          = "Entrata o uscita?",
                    Label             = "Tipo"
                }
            }
        };

        public IntentDefinition IntentDefinition => _intentDefinition;

        public Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var employeeName = slots.GetValueOrDefault("employeeName", "-");
            var time         = slots.GetValueOrDefault("time", "-");
            var date         = slots.GetValueOrDefault("date", DateTime.Today.ToString("dd/MM/yyyy"));
            var direction    = slots.GetValueOrDefault("direction", "IN");

            var directionLabel = direction.Equals("OUT", StringComparison.OrdinalIgnoreCase)
                ? "uscita"
                : "entrata";

            return Task.FromResult(CommandResult.Ok(
                $"Timbratura di {directionLabel} registrata: {employeeName} alle {time} del {date}."));
        }
    }
}
