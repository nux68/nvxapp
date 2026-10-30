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
        private readonly IRouteRegistry _routeRegistry;
        private readonly IChatUserContext _chatUserContext;

        public NavigateHandler(IRouteRegistry routeRegistry, IChatUserContext chatUserContext)
        {
            _routeRegistry = routeRegistry;
            _chatUserContext = chatUserContext;
        }

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        // Keywords e nomi pagina: unione italiano + inglese, qualunque sia la lingua dell'interfaccia.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name        = "Navigate",
            DisplayName = ChatAIMessages.Get("NavigateDisplayName"),
            Usable4Role = new List<string> { "*" },
            Category    = ChatAIMessages.Get("ChatCategoryNavigation"),
            CategoryOrder = IntentCategoryOrder.Navigation,
            Description = ChatAIMessages.Get("NavigateDescription"),
            IsNavigation = true,
            ExecutionStrategy = ExecutionStrategy.ContinueOnError,
            Keywords    = new() { "vai", "apri", "mostra", "naviga", "portami", "visualizza", "pagina",
                                  "go to", "open", "show", "navigate", "take me", "view", "page",
                                  "logout", "log out", "impersona", "impersonate" },
            Examples    = new() { "portami alla lista delle aziende", "fammi vedere gli utenti", "voglio andare alla home", "fai il logout", "voglio impersonare un utente", "take me to the companies page" },
            Slots       =
            [
                new()
                {
                    Name              = "page",
                    Type              = "string",
                    Required          = true,
                    PromptDescription = "nome della pagina. Pagine disponibili: " +
                        "home, impersonate, login, logout, utente, " +
                        "timbratura, richieste, le mie richieste, calendario, il mio calendario, richiesta giustificativo, richiesta timbratura, " +
                        "richieste admin, calendario hr, calendario admin, utenti reparto, " +
                        "(usa il nome come lo dice l'utente, es. \"richieste\": la pagina giusta per il suo ruolo la sceglie l'applicazione) " +
                        "export causali, statistiche attività, personale presente, piano ferie, " +
                        "giustificativi, causali, configurazione, orari, profili orari, modelli export, " +
                        "commesse, clienti, reparti, sedi, attività, competenze, " +
                        "utenti, centri, studi, aziende, utenti centro, utenti studio, utenti azienda. " +
                        "Pagine in inglese (se l'utente scrive in inglese restituisci il nome inglese): " +
                        "home, dashboard, impersonate, login, logout, user, users, dealers, financial advisors, " +
                        "companies, dealer users, financial advisor users, company users",
                    Question          = ChatAIMessages.Get("NavigatePageQuestion"),
                    Label             = ChatAIMessages.Get("NavigatePageLabel")
                },
                new()
                {
                    Name              = "params",
                    Type              = "string",
                    Required          = false,
                    PromptDescription = "parametri aggiuntivi (es. dipendente, data, tab) separati da virgola",
                    Question          = ChatAIMessages.Get("NavigateParamsQuestion"),
                    Label             = ChatAIMessages.Get("NavigateParamsLabel")
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("page", out var page);
            slots.TryGetValue("params", out var rawParams);

            // stessa voce di menu, pagina diversa secondo il ruolo (es. "richieste" per l'amministratore)
            var route = _routeRegistry.Resolve(page, _chatUserContext.Roles);

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

            // nel testo c'e' la route: il client la sostituisce con il nome della voce di menu
            return Task.FromResult(CommandResult.Ok(ChatAIMessages.Get("NavigateResult", route), payload));
        }
    }
}
