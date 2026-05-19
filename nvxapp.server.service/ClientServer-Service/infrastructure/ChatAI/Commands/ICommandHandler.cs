using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands
{
    // Contratto base per tutti gli handler di comando.
    // Ogni handler dichiara il proprio IntentName: il registry si auto-costruisce
    // a partire dall'IEnumerable iniettato, senza mapping espliciti nel codice.
    public interface ICommandHandler
    {
        // Nome dell'intent gestito da questo handler (es. "RegisterClocking").
        // Deve corrispondere esattamente al Name usato in IntentCatalog.
        string IntentName { get; }

        Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots);
    }
}
