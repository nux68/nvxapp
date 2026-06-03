using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.infrastructure
{
    public class HelpHandler : BaseCommandHandler
    {
        private readonly IServiceProvider _serviceProvider;

        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name        = "Help",
            DisplayName = "Aiuto",
            Usable4Role = new List<string> { "*" },
            Description = "fornisce la lista dei comandi disponibili.",
            Keywords    = new() { "aiuto", "help", "comandi", "cosa sai fare", "menu" }
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
            sb.AppendLine("Ecco i comandi disponibili:");

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

