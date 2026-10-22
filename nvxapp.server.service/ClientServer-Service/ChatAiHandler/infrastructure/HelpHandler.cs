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
            Keywords    = new() { "aiuto", "help", "comandi", "cosa sai fare", "menu", "commands", "what can you do" }
        };

        public HelpHandler(IServiceProvider serviceProvider)
        {
            _serviceProvider = serviceProvider;
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            // Risolve gli handler lazily per evitare la dipendenza circolare in fase di registrazione.
            var handlers = _serviceProvider.GetService(typeof(IEnumerable<ICommandHandler>))
                           as IEnumerable<ICommandHandler> ?? Enumerable.Empty<ICommandHandler>();

            var sb = new System.Text.StringBuilder();
            sb.AppendLine(ChatAIMessages.Get("HelpCommandsTitle"));

            foreach (var handler in handlers)
            {
                var intent = handler.IntentDefinition;

                // Salta l'help stesso dalla lista
                if (intent.Name.Equals("Help", StringComparison.OrdinalIgnoreCase)) continue;

                var label = !string.IsNullOrEmpty(intent.DisplayName) ? intent.DisplayName : intent.Name;
                sb.AppendLine($"  • {label} — {intent.Description}");
            }

            return Task.FromResult(CommandResult.Ok(sb.ToString().TrimEnd()));
        }
    }
}

