using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.infrastructure
{
    // Handler per la navigazione lato client.
    // Non effettua chiamate dati: restituisce un NavigatePayload in CommandResult.Data
    // che il frontend Angular usa per spostarsi sulla route desiderata.
    // Le route sono fornite dai provider IRouteProvider registrati nella DI
    // (InfrastructureRouteProvider, AttendanceTrackingRouteProvider, …).
    public class NavigateHandler : BaseCommandHandler
    {
        private readonly RouteRegistry _routeRegistry;

        public NavigateHandler(RouteRegistry routeRegistry)
        {
            _routeRegistry = routeRegistry;
        }

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

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("page", out var page);
            slots.TryGetValue("params", out var rawParams);

            var route = _routeRegistry.Resolve(page);

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
    }
}
