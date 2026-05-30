using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.infrastructure
{
    // Handler per la navigazione lato client.
    // Non effettua chiamate dati: restituisce un NavigatePayload in CommandResult.Data
    // che il frontend Angular usa per spostarsi sulla route desiderata.
    public class NavigateHandler : BaseCommandHandler
    {
        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name        = "Navigate",
            DisplayName = "Vai a",
            Description = "naviga a una pagina o sezione dell'applicazione.",
            IsNavigation = true,
            ExecutionStrategy = ExecutionStrategy.ContinueOnError,
            Keywords    = new() { "vai", "apri", "mostra", "naviga", "portami", "visualizza", "pagina" },
            Slots       =
            [
                new()
                {
                    Name              = "page",
                    Type              = "string",
                    Required          = true,
                    PromptDescription = "nome della pagina. Pagine disponibili: " +
                        "home, impersonate, login, logout, utente, " +
                        "timbratura, richieste, calendario, richiesta giustificativo, richiesta timbratura, " +
                        "richieste admin, calendario hr, calendario admin, utenti reparto, " +
                        "export causali, statistiche attività, personale presente, piano ferie, " +
                        "giustificativi, causali, configurazione, orari, profili orari, modelli export, " +
                        "commesse, clienti, reparti, sedi, attività, competenze",
                    Question          = "A quale pagina vuoi andare?",
                    Label             = "Pagina"
                },
                new()
                {
                    Name              = "params",
                    Type              = "string",
                    Required          = false,
                    PromptDescription = "parametri aggiuntivi (es. dipendente, data, tab) separati da virgola",
                    Question          = "Hai parametri aggiuntivi? (opzionale)",
                    Label             = "Parametri"
                }
            ]
        };

        // Mappa dei nomi pagina ? route Angular
        // Derivata da MainMenuInfrastructureService e MainMenuAttendanceTrackingService
        private static readonly Dictionary<string, string> _routeMap = new(StringComparer.OrdinalIgnoreCase)
        {
            // --- Infrastructure ---
            ["home"]                        = "/home",
            ["impersonate"]                 = "/userimpersonate",
            ["login"]                       = "/login",
            ["logout"]                      = "/logout",
            ["utente"]                      = "/user",
            ["user"]                        = "/user",
            ["superuser"]                   = "/superuser",
            ["admin"]                       = "/admin",
            ["poweradmin"]                  = "/poweradmin",
            ["utenti"]                      = "/userlist",
            ["userlist"]                    = "/userlist",
            ["centri"]                      = "/dealerlist",
            ["dealerlist"]                  = "/dealerlist",
            ["centro power admin"]          = "/dealerpoweradmin",
            ["centro admin"]                = "/dealeradmin",
            ["utenti centro"]               = "/userdealerlist",
            ["studi"]                       = "/financialadvisorlist",
            ["studio power admin"]          = "/financialadvisorpoweradmin",
            ["studio admin"]                = "/financialadvisoradmin",
            ["utenti studio"]               = "/userfinancialadvisorlist",
            ["aziende"]                     = "/companylist",
            ["azienda power admin"]         = "/companypoweradmin",
            ["azienda admin"]               = "/companyadmin",
            ["utenti azienda"]              = "/usercompanylist",

            // --- AttendanceTracking User ---
            ["timbratura"]                  = "/timeclockuser",
            ["timeclockuser"]               = "/timeclockuser",
            ["richieste"]                   = "/requestlistuser",
            ["requestlistuser"]             = "/requestlistuser",
            ["calendario"]                  = "/usertimesheet",
            ["usertimesheet"]               = "/usertimesheet",
            ["richiesta giustificativo"]    = "/requestjustificationuser",
            ["giustificativo"]              = "/requestjustificationuser",
            ["richiesta timbratura"]        = "/requestclockinguser",

            // --- AttendanceTracking CompanyAdmin/PowerAdmin ---
            ["richieste hr"]                = "/requestlistadmin",
            ["requestlistadmin"]            = "/requestlistadmin",
            ["calendario hr"]               = "/poweradmintimesheet",
            ["poweradmintimesheet"]         = "/poweradmintimesheet",
            ["calendario admin"]            = "/admintimesheet",
            ["admintimesheet"]              = "/admintimesheet",
            ["utenti reparto"]              = "/userdepartmentlist",
            ["export causali"]              = "/exportcausali",
            ["statistiche attività"]        = "/activitystatistics",
            ["statistiche"]                 = "/activitystatistics",
            ["personale presente"]          = "/presentstaff",
            ["piano ferie"]                 = "/vacationplan",
            ["giustificativi"]              = "/justificationlist",
            ["causali"]                     = "/causalilist",
            ["configurazione"]              = "/companycfgedit",
            ["orari"]                       = "/orarilist",
            ["profili orari"]               = "/profiliorarilist",
            ["modelli export"]              = "/exportcaulist",
            ["commesse"]                    = "/commessalist",
            ["clienti"]                     = "/customerlist",
            ["reparti"]                     = "/departmentlist",
            ["sedi"]                        = "/azsedilist",
            ["attività"]                    = "/activitylist",
            ["competenze"]                  = "/competencelist",

            // --- Alias generici ---
            ["dashboard"]                   = "/home",
            ["presenze"]                    = "/timeclockuser",
            ["impostazioni"]                = "/companycfgedit",
            ["settings"]                    = "/companycfgedit",
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("page", out var page);
            slots.TryGetValue("params", out var rawParams);

            var route = ResolveRoute(page);

            var navParams = new Dictionary<string, string>();
            if (!string.IsNullOrWhiteSpace(rawParams))
            {
                foreach (var part in rawParams.Split(',', StringSplitOptions.RemoveEmptyEntries))
                {
                    var kv = part.Split('=', 2);
                    if (kv.Length == 2)
                        navParams[kv[0].Trim()] = kv[1].Trim();
                    else
                        navParams[part.Trim()] = string.Empty;
                }
            }

            var payload = new NavigatePayload
            {
                Route  = route,
                Params = navParams
            };

            return Task.FromResult(CommandResult.Ok($"Navigazione verso '{route}'.", payload));
        }

        private static string ResolveRoute(string? page)
        {
            if (string.IsNullOrWhiteSpace(page)) return "/dashboard";

            if (_routeMap.TryGetValue(page.Trim(), out var route))
                return route;

            // Fallback: usa il nome della pagina direttamente come route
            return "/" + page.Trim().ToLowerInvariant();
        }
    }
}
