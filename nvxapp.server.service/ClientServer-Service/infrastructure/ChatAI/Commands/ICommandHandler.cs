using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands
{
    // Contratto base per tutti gli handler di comando.
    // Ogni handler è responsabile sia della propria esecuzione sia della
    // definizione dell'intent (nome, descrizione, slot richiesti).
    // CommandRegistry e IntentCatalog si auto-costruiscono a runtime
    // dall'IEnumerable<ICommandHandler> iniettato dalla DI.
    public interface ICommandHandler
    {
        // Definizione completa dell'intent: nome, descrizione e lista slot.
        // Usata da IntentCatalog per costruire il system prompt verso Ollama
        // e da GetMissingRequiredSlots per sapere quali slot sono obbligatori.
        IntentDefinition IntentDefinition { get; }

        Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots);
    }
}
