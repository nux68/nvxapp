namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Registro centrale auto-costruito.
    // Non contiene mapping espliciti: ogni IRouteProvider dichiara
    // le proprie route e viene registrato automaticamente.
    // Per aggiungere un nuovo modulo basta creare un nuovo IRouteProvider
    // in qualsiasi cartella — non serve toccare questo file.
    public class RouteRegistry : IRouteRegistry
    {
        private readonly Dictionary<string, string> _routes;
        private readonly List<RoleRoute> _roleRoutes = new();

        public RouteRegistry(IEnumerable<IRouteProvider> providers)
        {
            _routes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var provider in providers)
            {
                foreach (var kv in provider.Routes)
                    _routes.TryAdd(kv.Key, kv.Value);   // primo provider vince in caso di duplicato
                _roleRoutes.AddRange(provider.RoleRoutes);
            }
        }

        public string Resolve(string? page, IReadOnlyCollection<string>? roles = null)
        {
            if (string.IsNullOrWhiteSpace(page)) return "/home";

            // pagina diversa secondo il ruolo (es. "richieste" per l'amministratore)
            if (roles is { Count: > 0 })
            {
                var roleRoute = _roleRoutes.FirstOrDefault(r =>
                    r.Name.Equals(page.Trim(), StringComparison.OrdinalIgnoreCase) &&
                    r.Roles.Any(x => roles.Contains(x, StringComparer.OrdinalIgnoreCase)));
                if (roleRoute != null)
                    return roleRoute.Route;
            }

            if (_routes.TryGetValue(page.Trim(), out var route))
                return route;

            // Fallback: usa il nome della pagina direttamente come route
            return "/" + page.Trim().ToLowerInvariant();
        }
    }
}
