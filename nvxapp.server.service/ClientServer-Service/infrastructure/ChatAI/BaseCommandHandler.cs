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
        // La coppia di slot data "from"/"to" e' un periodo: un mese detto dall'utente vale dal primo
        // all'ultimo giorno (SlotDefinition.PeriodEndSlot).
        protected static List<SlotDefinition> BuildSlots(params SlotDefinition[] handlerSlots)
        {
            var slots = new List<SlotDefinition>(handlerSlots);
            var from = slots.FirstOrDefault(s => s.Name == "from" && s.Type == SlotNormalizers.DateFormat);
            var to = slots.FirstOrDefault(s => s.Name == "to" && s.Type == SlotNormalizers.DateFormat);
            if (from != null && to != null && string.IsNullOrEmpty(from.PeriodEndSlot))
                from.PeriodEndSlot = to.Name;
            return slots;
        }
    }
}
