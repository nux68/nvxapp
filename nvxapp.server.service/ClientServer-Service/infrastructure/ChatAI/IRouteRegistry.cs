namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Risolve il nome/alias di una pagina nella relativa route Angular.
    // roles: ruoli dell'utente, per i nomi che portano a pagine diverse secondo il ruolo (IRouteProvider.RoleRoutes)
    public interface IRouteRegistry
    {
        string Resolve(string? page, IReadOnlyCollection<string>? roles = null);

        // Come Resolve, ma solo per i nomi dichiarati dai provider: null se il nome non e' dichiarato (nessun fallback)
        string? Find(string? page, IReadOnlyCollection<string>? roles = null);

        // nomi di pagina dichiarati dai moduli per il prompt del comando Navigate (IRouteProvider.PromptPages)
        IReadOnlyList<string> PromptPages { get; }
    }
}
