namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Risolve il nome/alias di una pagina nella relativa route Angular.
    public interface IRouteRegistry
    {
        string Resolve(string? page);
    }
}
