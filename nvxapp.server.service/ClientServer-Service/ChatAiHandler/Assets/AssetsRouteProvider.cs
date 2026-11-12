using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Assets
{
    // Pagine di Risorse aziendali per il comando Navigate. Derivate da route-assets.service.ts / main-menu-assets.service.ts (client).
    // "impostazioni", "calendario", "sedi" restano delle presenze (primo provider vince): qui i nomi sono qualificati.
    // I permessi veri li controllano le pagine e i servizi (veicoli, beni, risorse: amministratori).
    public class AssetsRouteProvider : IRouteProvider
    {
        private static readonly Dictionary<string, string> _routes = new(StringComparer.OrdinalIgnoreCase)
        {
            ["risorse aziendali"]               = "/astbookings",
            ["prenotazioni"]                    = "/astbookings",
            ["prenotazione"]                    = "/astbookings",
            ["prenotazioni sale"]               = "/astbookings",
            ["calendario prenotazioni"]         = "/astbookings",
            ["flotta"]                          = "/astvehicles",
            ["flotta aziendale"]                = "/astvehicles",
            ["veicoli"]                         = "/astvehicles",
            ["auto aziendali"]                  = "/astvehicles",
            ["parco auto"]                      = "/astvehicles",
            ["parco veicoli"]                   = "/astvehicles",
            ["beni"]                            = "/astgoods",
            ["beni aziendali"]                  = "/astgoods",
            ["cespiti"]                         = "/astgoods",
            ["inventario beni"]                 = "/astgoods",
            ["risorse prenotabili"]             = "/astresources",
            ["sale"]                            = "/astresources",
            ["sale riunioni"]                   = "/astresources",
            ["postazioni"]                      = "/astresources",
            ["le mie risorse"]                  = "/astmine",
            ["mie risorse"]                     = "/astmine",
            ["la mia dotazione"]                = "/astmine",
            ["impostazioni risorse aziendali"]  = "/astsettings",
            ["configurazione risorse aziendali"] = "/astsettings",
            ["manuale risorse aziendali"]       = "/usermanual/assets",

            // nomi inglesi delle stesse pagine
            ["company resources"]               = "/astbookings",
            ["bookings"]                        = "/astbookings",
            ["reservations"]                    = "/astbookings",
            ["room booking"]                    = "/astbookings",
            ["fleet"]                           = "/astvehicles",
            ["vehicles"]                        = "/astvehicles",
            ["company cars"]                    = "/astvehicles",
            ["company assets"]                  = "/astgoods",
            ["goods"]                           = "/astgoods",
            ["bookable resources"]              = "/astresources",
            ["meeting rooms"]                   = "/astresources",
            ["desks"]                           = "/astresources",
            ["my resources"]                    = "/astmine",
            ["company resources settings"]      = "/astsettings",
            ["assets settings"]                 = "/astsettings",
            ["company resources manual"]        = "/usermanual/assets",
            ["assets manual"]                   = "/usermanual/assets",
        };

        public IReadOnlyDictionary<string, string> Routes => _routes;

        public IReadOnlyList<string> PromptPages { get; } = new[]
        {
            "prenotazioni", "flotta", "veicoli", "beni", "risorse prenotabili", "le mie risorse", "impostazioni risorse aziendali", "manuale risorse aziendali",
            "bookings", "fleet", "company assets", "bookable resources", "my resources"
        };
    }
}
