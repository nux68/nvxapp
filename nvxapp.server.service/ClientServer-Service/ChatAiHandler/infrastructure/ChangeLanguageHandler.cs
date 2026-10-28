using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.infrastructure
{
    // Cambia la lingua dell'interfaccia (italiano / inglese).
    // Nessun dato modificato sul server: restituisce un ClientActionPayload che il client esegue
    // (il cambio di lingua ricarica la pagina).
    public class ChangeLanguageHandler : BaseCommandHandler
    {
        private static readonly Dictionary<string, string> _words = new(StringComparer.OrdinalIgnoreCase)
        {
            ["italiano"] = "it", ["italiana"] = "it", ["italian"] = "it", ["it"] = "it", ["ita"] = "it",
            ["inglese"] = "en", ["english"] = "en", ["en"] = "en", ["eng"] = "en",
        };

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name        = "ChangeLanguage",
            DisplayName = ChatAIMessages.Get("LanguageDisplayName"),
            Usable4Role = new List<string> { "*" },
            Category    = ChatAIMessages.Get("ChatCategorySettings"),
            CategoryOrder = IntentCategoryOrder.Settings,
            Description = ChatAIMessages.Get("LanguageDescription"),
            // non i valori dello slot ("italiano", "inglese"): un valore uguale a una parola chiave viene scartato
            Keywords    = new() { "lingua", "language" },
            Examples    = new() { "passa all'inglese", "metti la lingua italiana", "switch to Italian", "change language to English" },
            RequiresConfirmation = false,
            Slots =
            [
                new()
                {
                    Name              = "language",
                    Type              = "it/en",
                    Required          = true,
                    PromptDescription = "lingua richiesta come detta dall'utente (es. 'inglese', 'italiano', 'English')",
                    Question          = ChatAIMessages.Get("LanguageQuestion"),
                    Label             = ChatAIMessages.Get("LanguageLabel"),
                    Normalizer        = Normalize,
                    Validator         = v => v is "it" or "en"
                        ? null
                        : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, ChatAIMessages.Get("LanguageInvalid", v), "language")
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        // "inglese" -> en, "lingua italiana" -> it (una parola nota nel testo basta)
        private static string? Normalize(string value) =>
            value.Split(new[] { ' ', '\'', ',' }, StringSplitOptions.RemoveEmptyEntries)
                 .Select(w => _words.GetValueOrDefault(w))
                 .FirstOrDefault(l => l != null);

        public override Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var language = slots.GetValueOrDefault("language", "it");
            var name = ChatAIMessages.Get(language == "en" ? "LanguageEn" : "LanguageIt");

            return Task.FromResult(CommandResult.Ok(ChatAIMessages.Get("LanguageDone", name),
                new ClientActionPayload { Action = "language", Value = language }));
        }
    }
}
