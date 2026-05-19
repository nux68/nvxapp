using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands.Handlers;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands
{
    // Registro centrale: mappa ogni nome di intent al suo handler.
    // Nuovi comandi si aggiungono qui senza toccare ChatAIService.
    public class CommandRegistry : ICommandRegistry
    {
        private readonly Dictionary<string, ICommandHandler> _handlers;

        public CommandRegistry(
            IRegisterClockingHandler  clockingHandler,
            IRegisterHolidayHandler   holidayHandler,
            IRegisterSickLeaveHandler sickLeaveHandler)
        {
            _handlers = new Dictionary<string, ICommandHandler>(StringComparer.OrdinalIgnoreCase)
            {
                { "RegisterClocking",  clockingHandler  },
                { "RegisterHoliday",   holidayHandler   },
                { "RegisterSickLeave", sickLeaveHandler }
            };
        }

        public ICommandHandler Resolve(string intentName)
        {
            if (_handlers.TryGetValue(intentName, out var handler))
                return handler;

            throw new InvalidOperationException($"Nessun handler registrato per l'intent '{intentName}'.");
        }
    }
}
