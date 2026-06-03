using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina di modifica di un giustificativo specifico.
    // Cerca il giustificativo per nome/codice e restituisce un NavigatePayload
    // con route /justificationedit e state { id } per il router Angular.
    public class OpenGiustificativoHandler : BaseCommandHandler
    {
        private readonly IPar_GiustificativiService _par_GiustificativiService;

        public OpenGiustificativoHandler(IPar_GiustificativiService par_GiustificativiService)
        {
            _par_GiustificativiService = par_GiustificativiService;
        }

        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenGiustificativo",
            DisplayName  = "Apri giustificativo",
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = "apre la pagina di modifica di un giustificativo specifico.",
            IsNavigation = true,
            Keywords     = new() { "modifica giustificativo", "apri giustificativo", "apri il giustificativo", "modifica il giustificativo", "giustificativo" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome o codice del giustificativo da aprire",
                    Question           = "Quale giustificativo vuoi aprire?",
                    Label              = "Giustificativo",
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail("Nome giustificativo non specificato.");

            var result = await _par_GiustificativiService.GetAll(
                new GenericRequest<Par_GiustificativiInModel>(new Par_GiustificativiInModel()), false);

            if (!result.Success || result.Data?.Par_Giustificativi == null)
                return CommandResult.Fail("Impossibile caricare l'elenco giustificativi.");

            var nomeTrimmed = nome.Trim();

            // Ricerca esatta per descrizione o codice
            var giustificativo = result.Data.Par_Giustificativi.FirstOrDefault(g =>
                string.Equals(g.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(g.Codice, nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            // Ricerca parziale se non trovato esatto
            if (giustificativo == null)
                giustificativo = result.Data.Par_Giustificativi.FirstOrDefault(g =>
                    (g.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (g.Codice?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false));

            if (giustificativo == null)
                return CommandResult.Fail($"Giustificativo '{nome}' non trovato.");

            var payload = new NavigatePayload
            {
                Route = "/justificationedit",
                State = new Dictionary<string, object> { ["id"] = giustificativo.Id }
            };

            return CommandResult.Ok(
                $"Apertura giustificativo '{giustificativo.Descrizione}' (id: {giustificativo.Id}).",
                payload);
        }
    }
}
