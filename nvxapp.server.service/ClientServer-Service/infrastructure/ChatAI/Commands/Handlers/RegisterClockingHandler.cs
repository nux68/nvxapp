using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;


namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands.Handlers
{
    // Gestisce la registrazione di una timbratura entrata/uscita.
    // TODO: sostituire il placeholder con la chiamata API reale
    //       e il lookup del dipendente per nome ? EmployeeId.
    public class RegisterClockingHandler : ICommandHandler
    {
        public IntentDefinition IntentDefinition => new()
        {
            Name        = "RegisterClocking",
            Description = "registra una timbratura di entrata o uscita",
            Slots       = new()
            {
                new() { Name = "employeeName", Type = "string",     Required = true                    },
                new() { Name = "time",         Type = "HH:mm",      Required = true                    },
                new() { Name = "date",         Type = "yyyy-MM-dd", Required = false, Default = "oggi" },
                new() { Name = "direction",    Type = "IN/OUT",     Required = false, Default = "IN"   }
            }
        };

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
