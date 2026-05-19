using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands.Handlers
{
    // Gestisce la registrazione di un periodo di malattia.
    // TODO: sostituire il placeholder con la chiamata API reale
    //       e il lookup del dipendente per nome ? EmployeeId.
    public class RegisterSickLeaveHandler : ICommandHandler
    {
        public string IntentName => "RegisterSickLeave";

        public Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var employeeName      = slots.GetValueOrDefault("employeeName", "-");
            var startDate         = slots.GetValueOrDefault("startDate", "-");
            var endDate           = slots.GetValueOrDefault("endDate", string.Empty);
            var certificateNumber = slots.GetValueOrDefault("certificateNumber", string.Empty);

            var endPart  = !string.IsNullOrEmpty(endDate)
                ? $" al {endDate}"
                : string.Empty;

            var certPart = !string.IsNullOrEmpty(certificateNumber)
                ? $" (certificato n. {certificateNumber})"
                : string.Empty;

            return Task.FromResult(CommandResult.Ok(
                $"Malattia registrata: {employeeName} dal {startDate}{endPart}{certPart}."));
        }
    }
}
