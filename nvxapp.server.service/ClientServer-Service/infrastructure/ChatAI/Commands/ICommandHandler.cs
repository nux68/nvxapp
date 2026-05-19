using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands
{
    // Contratto base per tutti gli handler di comando.
    // Riceve gli slot grezzi della sessione e restituisce un CommandResult.
    public interface ICommandHandler
    {
        Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots);
    }
}
