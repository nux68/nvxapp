using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands.Handlers
{
    // Gestisce la registrazione di una timbratura entrata/uscita.
    // TODO: sostituire il placeholder con la chiamata API reale
    //       e il lookup del dipendente per nome ? EmployeeId.
    public class RegisterClockingHandler : IRegisterClockingHandler
    {
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
