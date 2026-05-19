using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands.Handlers
{
    // Gestisce la registrazione di un periodo ferie.
    // TODO: sostituire il placeholder con la chiamata API reale,
    //       aggiungere il controllo sovrapposizioni e il lookup dipendente.
    public class RegisterHolidayHandler : ICommandHandler
    {
        public string IntentName => "RegisterHoliday";

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
