using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.infrastructure
{
    // Comando unico "apri <entità> <nome>": cerca l'elemento per descrizione o codice tra quelli
    // dell'entità (IOpenableEntity, dichiarate dai moduli) e apre la sua pagina di modifica.
    // Senza entità registrate (es. solo Infrastructure) il comando non compare: nessun ruolo lo può usare.
    public class OpenEntityHandler : BaseCommandHandler
    {
        private readonly List<IOpenableEntity> _entities;
        private readonly IChatUserContext _chatUserContext;
        private readonly IntentDefinition _intentDefinition;

        public OpenEntityHandler(IEnumerable<IOpenableEntity> entities, IChatUserContext chatUserContext)
        {
            _entities = entities.ToList();
            _chatUserContext = chatUserContext;

            var nomi = string.Join(", ", _entities.Select(e => e.DisplayName));

            // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
            _intentDefinition = new IntentDefinition
            {
                Name = "OpenEntity",
                DisplayName = ChatAIMessages.Get("OpenEntityDisplayName"),
                Description = ChatAIMessages.Get("OpenEntityDescription", nomi),
                Usable4Role = _entities.SelectMany(e => e.Roles).Distinct().ToList(),
                IsNavigation = true,
                Keywords = new List<string> { "apri", "modifica", "scheda", "open", "edit" }
                    .Concat(_entities.SelectMany(e => e.Words)).Distinct(StringComparer.OrdinalIgnoreCase).ToList(),
                Examples = new() { "apri la causale straordinario", "modifica il reparto produzione", "fammi vedere la commessa cantiere Milano", "open the holiday justification" },
                Pages = _entities.SelectMany(e => e.Pages).Distinct().ToList(),
                Slots =
                [
                    new()
                    {
                        Name = "entity",
                        Type = "string",
                        Required = true,
                        PromptDescription = $"tipo di elemento da aprire, uno tra: {nomi}",
                        Question = ChatAIMessages.Get("OpenEntityTypeQuestion", nomi),
                        Label = ChatAIMessages.Get("OpenEntityTypeLabel"),
                        Normalizer = v => Trova(v)?.Key,
                        Validator = v => Trova(v) != null
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, ChatAIMessages.Get("OpenEntityTypeQuestion", nomi), "entity")
                    },
                    new()
                    {
                        Name = "name",
                        Type = "string",
                        Required = true,
                        PromptDescription = "nome o codice dell'elemento da aprire, come detto dall'utente",
                        Question = ChatAIMessages.Get("OpenEntityNameQuestion"),
                        Label = ChatAIMessages.Get("OpenEntityNameLabel"),
                        HasRelevantContent = msg => msg.Length > 0
                    }
                ]
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        // entità dal valore dello slot: la chiave, oppure una delle sue parole contenuta nel testo ("la causale")
        private IOpenableEntity? Trova(string? valore)
        {
            // normalizzato: "l'attività" -> "l attività"
            var testo = $" {ChatAIKeywords.Normalize(valore)} ";
            return _entities.FirstOrDefault(e => e.Key.Equals(valore?.Trim(), StringComparison.OrdinalIgnoreCase))
                ?? _entities
                    .SelectMany(e => e.Words.Select(w => (Entity: e, Word: ChatAIKeywords.Normalize(w))))
                    .Where(x => testo.Contains($" {x.Word} "))
                    .OrderByDescending(x => x.Word.Length)
                    .Select(x => x.Entity)
                    .FirstOrDefault();
        }

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var entity = Trova(slots.GetValueOrDefault("entity"));
            if (entity == null)
                return CommandResult.Fail(ChatAIMessages.Get("OpenEntityTypeQuestion", string.Join(", ", _entities.Select(e => e.DisplayName))));

            // i servizi non controllano i ruoli: lo fa il comando
            if (!entity.Roles.Contains("*") && !_chatUserContext.IsInRole(entity.Roles.ToArray()))
                return CommandResult.Fail(ChatAIMessages.Get("OpenEntityNotAllowed", entity.DisplayName));

            var nome = slots.GetValueOrDefault("name", string.Empty).Trim();
            var elementi = await entity.LoadAsync();

            // prima per descrizione o codice uguali, poi contenuti
            var trovati = elementi.Where(e => string.Equals(e.Descrizione, nome, StringComparison.OrdinalIgnoreCase) ||
                                              string.Equals(e.Codice, nome, StringComparison.OrdinalIgnoreCase)).ToList();
            if (trovati.Count == 0)
                trovati = elementi.Where(e => e.Descrizione.Contains(nome, StringComparison.OrdinalIgnoreCase) ||
                                              (e.Codice?.Contains(nome, StringComparison.OrdinalIgnoreCase) ?? false)).ToList();

            if (trovati.Count == 0)
                return CommandResult.Fail(ChatAIMessages.Get("OpenEntityNotFound", entity.DisplayName, nome));
            if (trovati.Count > 1)
                return CommandResult.Fail(ChatAIMessages.Get("OpenEntityAmbiguous", trovati.Count, entity.DisplayName, nome,
                    string.Join(", ", trovati.Take(8).Select(e => e.Descrizione))));

            var elemento = trovati[0];
            var payload = new NavigatePayload
            {
                Route = entity.EditRoute,
                State = new Dictionary<string, object> { ["id"] = elemento.Id }
            };
            return CommandResult.Ok(ChatAIMessages.Get("OpenEntityOpening", entity.DisplayName, elemento.Descrizione), payload);
        }
    }
}
