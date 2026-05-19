using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;


namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands.Handlers
{
    // Gestisce la registrazione di un periodo ferie.
    // TODO: sostituire il placeholder con la chiamata API reale,
    //       aggiungere il controllo sovrapposizioni e il lookup dipendente.
    public class RegisterHolidayHandler : ICommandHandler
    {
        public IntentDefinition IntentDefinition => new()
        {
            Name        = "RegisterHoliday",
            Description = "mette in ferie un dipendente per un periodo",
            Slots       = new()
            {
                new()
                {
                    Name              = "employeeName",
                    Type              = "string",
                    Required          = true,
                    PromptDescription = "nome e cognome di una persona",
                    Question          = "Per quale dipendente?",
                    Label             = "Dipendente"
                },
                new()
                {
                    Name              = "startDate",
                    Type              = "yyyy-MM-dd",
                    Required          = true,
                    PromptDescription = $"data di inizio nel formato yyyy-MM-dd. Oggi \u00e8 {DateTime.Today:yyyy-MM-dd}.",
                    Question          = "Da quale data?",
                    Label             = "Dal"
                },
                new()
                {
                    Name              = "endDate",
                    Type              = "yyyy-MM-dd",
                    Required          = true,
                    PromptDescription = $"data di fine nel formato yyyy-MM-dd. Oggi \u00e8 {DateTime.Today:yyyy-MM-dd}.",
                    Question          = "Fino a quale data?",
                    Label             = "Al"
                }
            }
        };

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
