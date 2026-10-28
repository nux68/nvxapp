using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.infrastructure
{
    public class HelpHandler : BaseCommandHandler
    {
        private readonly IServiceProvider _serviceProvider;

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        // Keywords: unione italiano + inglese, qualunque sia la lingua dell'interfaccia.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name        = "Help",
            DisplayName = ChatAIMessages.Get("HelpDisplayName"),
            Usable4Role = new List<string> { "*" },
            Description = ChatAIMessages.Get("HelpDescription"),
            Keywords    = new() { "aiuto", "help", "comandi", "cosa sai fare", "menu", "commands", "what can you do" },
            Examples    = new() { "che cosa posso chiederti?", "non so cosa fare, aiutami", "cosa posso fare in questa pagina?", "what can I ask you?" },
            // elenco dei comandi: nessun dato modificato, si mostra subito
            RequiresConfirmation = false
        };

        public HelpHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            // Risolti lazily per evitare la dipendenza circolare in fase di registrazione.
            // Il catalogo della richiesta è già filtrato sui ruoli dell'utente.
            var catalog = _serviceProvider.GetService(typeof(IIntentCatalog)) as IIntentCatalog;
            var userContext = _serviceProvider.GetService(typeof(IChatUserContext)) as IChatUserContext;

            // Salta l'help stesso dalla lista
            var intents = (catalog?.Intents ?? new List<IntentDefinition>())
                .Where(i => !i.Name.Equals("Help", StringComparison.OrdinalIgnoreCase))
                .ToList();

            // Comandi utili nella pagina corrente (IntentDefinition.Pages) per primi
            var route = userContext?.CurrentRoute ?? string.Empty;
            var pageIntents = string.IsNullOrEmpty(route)
                ? new List<IntentDefinition>()
                : intents.Where(i => i.Pages.Any(p => route.StartsWith(p, StringComparison.OrdinalIgnoreCase))).ToList();

            var sb = new System.Text.StringBuilder();
            if (pageIntents.Count > 0)
            {
                sb.AppendLine(ChatAIMessages.Get("HelpPageTitle"));
                foreach (var intent in pageIntents)
                    sb.AppendLine(FormatIntent(intent));
                sb.AppendLine();
                sb.AppendLine(ChatAIMessages.Get("HelpOtherTitle"));
            }
            else
                sb.AppendLine(ChatAIMessages.Get("HelpCommandsTitle"));

            foreach (var intent in intents.Except(pageIntents))
                sb.AppendLine(FormatIntent(intent));

            return Task.FromResult(CommandResult.Ok(sb.ToString().TrimEnd()));
        }

        // "• Timbratura — registra una timbratura... (es. "Rossi è entrato alle 8")"
        private static string FormatIntent(IntentDefinition intent)
        {
            var label = !string.IsNullOrEmpty(intent.DisplayName) ? intent.DisplayName : intent.Name;
            var example = intent.Examples.FirstOrDefault();
            return string.IsNullOrEmpty(example)
                ? $"  • {label} — {intent.Description}"
                : $"  • {label} — {intent.Description} ({ChatAIMessages.Get("HelpExample", example)})";
        }
    }
}

