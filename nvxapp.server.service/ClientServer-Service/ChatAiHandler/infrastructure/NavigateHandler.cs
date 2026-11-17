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

            // pagine dei moduli (CRM, documentale...) dichiarate dai loro IRouteProvider
            var pagineModuli = _routeRegistry?.PromptPages ?? [];
            if (pagineModuli.Count > 0)
                _intentDefinition.Slots[0].PromptDescription += ". Altre pagine: " + string.Join(", ", pagineModuli);
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

        // Pagina richiesta -> route.
        // 1. nome dichiarato da un IRouteProvider (anche per ruolo, es. "richieste"), se la sua pagina e' nel menu
        //    dell'utente o il menu non e' noto;
        // 2. titolo di una voce del menu dell'utente (MenuPageMatcher): "tabelle" -> /crmparlist; con piu' voci
        //    dallo stesso titolo (in sezioni diverse) si chiede quale;
        // 3. nome dichiarato per una pagina fuori menu (login, logout, impersonate...);
        // 4. la route stessa di una voce ("crmparlist");
        // 5. menu non noto (client che non lo manda): come prima, il nome diventa la route.
        // Altrimenti la pagina non c'e' nel menu dell'utente: errore invece di navigare verso una route inesistente.
        private (string? Route, string? Error) ResolveRoute(string? page)
        {
            if (string.IsNullOrWhiteSpace(page)) return ("/home", null);

            var menu = _chatUserContext.MenuPages ?? [];
            var declared = _routeRegistry.Find(page, _chatUserContext.Roles);
            if (declared != null && (menu.Count == 0 || menu.Any(m => MenuPageMatcher.SameRoute(m.Route, declared))))
                return (declared, null);

            var matches = MenuPageMatcher.Match(page, menu);
            if (matches.Count == 1)
                return (matches[0].Route, null);
            if (matches.Count > 1)
                return (null, ChatAIMessages.Get("NavigatePageAmbiguous", page.Trim(),
                    string.Join(", ", matches.Select(MenuPageMatcher.Label))));

            if (declared != null)
                return (declared, null);

            var asRoute = menu.FirstOrDefault(m => MenuPageMatcher.SameRoute(m.Route, page));
            if (asRoute != null)
                return (asRoute.Route, null);

            if (menu.Count == 0)
                return (_routeRegistry.Resolve(page, _chatUserContext.Roles), null);

            return (null, ChatAIMessages.Get("NavigatePageNotFound", page.Trim()));
        }

        public override Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("page", out var page);
            slots.TryGetValue("params", out var rawParams);

            // nome dichiarato dai provider (anche per ruolo) o titolo di una voce del menu dell'utente
            var (route, error) = ResolveRoute(page);
            if (route == null)
                return Task.FromResult(CommandResult.Fail(error ?? ChatAIMessages.Get("NavigatePageNotFound", page?.Trim() ?? string.Empty)));

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
