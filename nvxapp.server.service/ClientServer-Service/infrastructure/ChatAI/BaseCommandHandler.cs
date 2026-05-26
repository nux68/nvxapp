using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands
{
    // Classe base generica per tutti gli handler di comando.
    // Non contiene slot specifici: definisce solo il contratto minimo.
    public abstract class BaseCommandHandler : ICommandHandler
    {
        public abstract IntentDefinition IntentDefinition { get; }
        public abstract Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots);

        // Helper — compone la lista slot a partire dagli slot passati come parametri.
        // Uso: BuildSlots(new SlotDefinition { ... }, ...)
        protected static List<SlotDefinition> BuildSlots(params SlotDefinition[] handlerSlots)
            => new(handlerSlots);
    }
}
