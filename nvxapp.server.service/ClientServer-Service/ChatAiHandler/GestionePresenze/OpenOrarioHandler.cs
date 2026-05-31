using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina di modifica di un orario specifico.
    // Cerca l'orario per nome/codice e restituisce un NavigatePayload
    // con route /orariedit e state { id } per il router Angular.
    public class OpenOrarioHandler : BaseCommandHandler
    {
        private readonly IPar_OrarioService _par_OrarioService;

        public OpenOrarioHandler(IPar_OrarioService par_OrarioService)
        {
            _par_OrarioService = par_OrarioService;
        }

        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenOrario",
            DisplayName  = "Apri orario",
            Description  = "apre la pagina di modifica di un orario specifico.",
            IsNavigation = true,
            Keywords     = new() { "apri orario", "modifica orario", "apri l'orario", "modifica l'orario", "orario" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome o codice dell'orario da aprire",
                    Question           = "Quale orario vuoi aprire?",
                    Label              = "Orario",
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail("Nome orario non specificato.");

            var result = await _par_OrarioService.GetAll(
                new GenericRequest<Par_Orario_GetAllInModel>(new Par_Orario_GetAllInModel()), false);

            if (!result.Success || result.Data?.Par_Orario == null)
                return CommandResult.Fail("Impossibile caricare l'elenco orari.");

            var nomeTrimmed = nome.Trim();

            // Ricerca esatta per descrizione o codice
            var orario = result.Data.Par_Orario.FirstOrDefault(o =>
                string.Equals(o.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Codice,      nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            // Ricerca parziale se non trovato esatto
            if (orario == null)
                orario = result.Data.Par_Orario.FirstOrDefault(o =>
                    (o.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (o.Codice?.Contains(nomeTrimmed,      StringComparison.OrdinalIgnoreCase) ?? false));

            if (orario == null)
                return CommandResult.Fail($"Orario '{nome}' non trovato.");

            var payload = new NavigatePayload
            {
                Route = "/orariedit",
                State = new Dictionary<string, object> { ["id"] = orario.Id }
            };

            return CommandResult.Ok(
                $"Apertura orario '{orario.Descrizione}' (id: {orario.Id}).",
                payload);
        }
    }
}
