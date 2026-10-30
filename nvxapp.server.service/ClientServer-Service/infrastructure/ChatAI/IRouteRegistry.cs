namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Risolve il nome/alias di una pagina nella relativa route Angular.
    // roles: ruoli dell'utente, per i nomi che portano a pagine diverse secondo il ruolo (IRouteProvider.RoleRoutes)
    public interface IRouteRegistry
    {
        string Resolve(string? page, IReadOnlyCollection<string>? roles = null);
    }
}
