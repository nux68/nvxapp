using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.infrastructure
{
    // Cambia il tema dell'interfaccia (scuro / chiaro / alterna).
    // Nessun dato modificato sul server: restituisce un ClientActionPayload che il client esegue.
    public class ChangeThemeHandler : BaseCommandHandler
    {
        public const string Dark = "dark";
        public const string Light = "light";
        public const string Toggle = "toggle";

        private static readonly Dictionary<string, string> _words = new(StringComparer.OrdinalIgnoreCase)
        {
            ["scuro"] = Dark, ["scura"] = Dark, ["notte"] = Dark, ["notturno"] = Dark, ["nero"] = Dark, ["dark"] = Dark, ["night"] = Dark,
            ["chiaro"] = Light, ["chiara"] = Light, ["giorno"] = Light, ["diurno"] = Light, ["bianco"] = Light, ["light"] = Light, ["day"] = Light,
            ["alterna"] = Toggle, ["cambia"] = Toggle, ["inverti"] = Toggle, ["toggle"] = Toggle, ["switch"] = Toggle,
        };

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name        = "ChangeTheme",
            DisplayName = ChatAIMessages.Get("ThemeDisplayName"),
            Usable4Role = new List<string> { "*" },
            Description = ChatAIMessages.Get("ThemeDescription"),
            // non i valori dello slot ("scuro", "chiaro"): un valore uguale a una parola chiave viene scartato
            Keywords    = new() { "tema", "modalità notte", "theme", "dark mode", "light mode" },
            Examples    = new() { "metti il tema scuro", "passa alla modalità chiara", "cambia tema", "switch to dark mode" },
            RequiresConfirmation = false,
            Slots =
            [
                new()
                {
                    Name              = "mode",
                    Type              = "dark/light/toggle",
                    Required          = false,
                    Default           = Toggle,
                    PromptDescription = "tema richiesto come detto dall'utente (es. 'scuro', 'chiaro', 'dark'); se chiede solo di cambiarlo, ometti lo slot",
                    Question          = ChatAIMessages.Get("ThemeModeQuestion"),
                    Label             = ChatAIMessages.Get("ThemeModeLabel"),
                    Normalizer        = Normalize,
                    HasRelevantContent = msg => msg.Split(' ', StringSplitOptions.RemoveEmptyEntries).Any(w => _words.ContainsKey(w)),
                    Validator         = v => v is Dark or Light or Toggle
                        ? null
                        : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, ChatAIMessages.Get("ThemeModeQuestion"), "mode")
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        // "scuro" -> dark, "modalità chiara" -> light (una parola nota nel testo basta)
        private static string? Normalize(string value) =>
            value.Split(new[] { ' ', '\'', ',' }, StringSplitOptions.RemoveEmptyEntries)
                 .Select(w => _words.GetValueOrDefault(w))
                 .FirstOrDefault(m => m != null);

        public override Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var mode = slots.GetValueOrDefault("mode", Toggle);
            var message = mode switch
            {
                Dark => ChatAIMessages.Get("ThemeDone", ChatAIMessages.Get("ThemeDark")),
                Light => ChatAIMessages.Get("ThemeDone", ChatAIMessages.Get("ThemeLight")),
                _ => ChatAIMessages.Get("ThemeToggled")
            };

            return Task.FromResult(CommandResult.Ok(message, new ClientActionPayload { Action = "theme", Value = mode }));
        }
    }
}
