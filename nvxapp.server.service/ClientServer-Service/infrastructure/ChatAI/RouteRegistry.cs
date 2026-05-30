namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    // Registro centrale auto-costruito.
    // Non contiene mapping espliciti: ogni IRouteProvider dichiara
    // le proprie route e viene registrato automaticamente.
    // Per aggiungere un nuovo modulo basta creare un nuovo IRouteProvider
    // in qualsiasi cartella — non serve toccare questo file.
    public class RouteRegistry
    {
        private readonly Dictionary<string, string> _routes;

        public RouteRegistry(IEnumerable<IRouteProvider> providers)
        {
            _routes = new Dictionary<string, string>(StringComparer.OrdinalIgnoreCase);

            foreach (var provider in providers)
                foreach (var kv in provider.Routes)
                    _routes.TryAdd(kv.Key, kv.Value);   // primo provider vince in caso di duplicato
        }

        public string Resolve(string? page)
        {
            if (string.IsNullOrWhiteSpace(page)) return "/home";

            if (_routes.TryGetValue(page.Trim(), out var route))
                return route;

            // Fallback: usa il nome della pagina direttamente come route
            return "/" + page.Trim().ToLowerInvariant();
        }
    }
}
