using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.DocumentManagement
{
    // Pagine del Documentale per il comando Navigate.
    // Derivate da route-document-management.service.ts / main-menu-document-management.service.ts (client).
    public class DocumentManagementRouteProvider : IRouteProvider
    {
        private static readonly Dictionary<string, string> _routes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["documenti"]                 = "/doclist",
            ["documentale"]               = "/doclist",
            ["archivio"]                  = "/doclist",
            ["archivio documenti"]        = "/doclist",
            ["i miei documenti"]          = "/docmine",
            ["miei documenti"]            = "/docmine",
            ["tipi documento"]            = "/doctypelist",
            ["tipi di documento"]         = "/doctypelist",
            ["cartelle"]                  = "/docfolderlist",
            ["dipendenti documentale"]    = "/docemployeelist",
            ["anagrafica documentale"]    = "/docemployeelist",
            ["abilitazioni documentale"]  = "/docpermissions",
            ["permessi documenti"]        = "/docpermissions",
            ["impostazioni documentale"]  = "/docsettings",
            ["registro accessi"]          = "/doclog",
            ["log accessi"]               = "/doclog",
            ["spazio archivio"]           = "/docstorage",
            ["spazio occupato"]           = "/docstorage",
            ["manuale documentale"]       = "/usermanual/documents",

            // nomi inglesi delle stesse pagine
            ["documents"]                 = "/doclist",
            ["document archive"]          = "/doclist",
            ["my documents"]              = "/docmine",
            ["document types"]            = "/doctypelist",
            ["folders"]                   = "/docfolderlist",
            ["document employees"]        = "/docemployeelist",
            ["document permissions"]      = "/docpermissions",
            ["document settings"]         = "/docsettings",
            ["access log"]                = "/doclog",
            ["storage"]                   = "/docstorage",
            ["documents manual"]          = "/usermanual/documents",
        };

        public IReadOnlyDictionary<string, string> Routes => _routes;

        // "documenti": l'archivio per HR e amministrazione, i miei documenti per il dipendente
        public IReadOnlyList<RoleRoute> RoleRoutes { get; } = new[]
        {
            new RoleRoute("documenti", "/doclist", "CompanyPowerAdmin", "CompanyAdmin"),
            new RoleRoute("documents", "/doclist", "CompanyPowerAdmin", "CompanyAdmin"),
            new RoleRoute("documenti", "/docmine", "User"),
            new RoleRoute("documents", "/docmine", "User"),
            new RoleRoute("archivio", "/docmine", "User"),
        };

        public IReadOnlyList<string> PromptPages { get; } = new[]
        {
            "documenti", "archivio documenti", "i miei documenti", "tipi documento", "cartelle", "dipendenti documentale",
            "abilitazioni documentale", "impostazioni documentale", "registro accessi", "spazio archivio", "manuale documentale",
            "documents", "my documents", "folders", "access log", "storage"
        };
    }
}
