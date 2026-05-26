namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Risolve il nome dell'intent nel relativo handler.
    public interface ICommandRegistry
    {
        ICommandHandler Resolve(string intentName);
    }
}
