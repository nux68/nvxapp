using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands.Handlers;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Registro centrale auto-costruito.
    // Non contiene mapping espliciti: ogni ICommandHandler dichiara
    // il proprio IntentName e viene registrato automaticamente.
    // Per aggiungere un nuovo comando basta creare un nuovo ICommandHandler
    // in qualsiasi cartella — non serve toccare questo file.
    public class CommandRegistry : ICommandRegistry
    {
        private readonly Dictionary<string, ICommandHandler> _handlers;

        public CommandRegistry(IEnumerable<ICommandHandler> handlers)
        {
            _handlers = handlers.ToDictionary(
                h => h.IntentDefinition.Name,
                h => h,
                StringComparer.OrdinalIgnoreCase);
        }

        public ICommandHandler Resolve(string intentName)
        {
            if (_handlers.TryGetValue(intentName, out var handler))
                return handler;

            throw new InvalidOperationException(
                $"Nessun handler registrato per l'intent '{intentName}'. " +
                $"Handler disponibili: {string.Join(", ", _handlers.Keys)}");
        }
    }
}
