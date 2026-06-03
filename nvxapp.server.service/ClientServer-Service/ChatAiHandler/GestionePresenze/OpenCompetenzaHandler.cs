using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CompetenzaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CompetenzaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina di modifica di una competenza specifica.
    // Cerca la competenza per descrizione e restituisce un NavigatePayload
    // con route /competenceedit e state { id } per il router Angular.
    public class OpenCompetenzaHandler : BaseCommandHandler
    {
        private readonly IPar_CompetenzaService _par_CompetenzaService;

        public OpenCompetenzaHandler(IPar_CompetenzaService par_CompetenzaService)
        {
            _par_CompetenzaService = par_CompetenzaService;
        }

        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenCompetenza",
            DisplayName  = "Apri competenza",
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = "apre la pagina di modifica di una competenza specifica.",
            IsNavigation = true,
            Keywords     = new() { "competenza", "apri competenza", "modifica competenza", "apri la competenza" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome della competenza da aprire",
                    Question           = "Quale competenza vuoi aprire?",
                    Label              = "Competenza",
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail("Nome competenza non specificato.");

            var result = await _par_CompetenzaService.GetAll(
                new GenericRequest<Par_Competenza_GetAll_InModel>(new Par_Competenza_GetAll_InModel()), false);

            if (!result.Success || result.Data?.Par_Competenza == null)
                return CommandResult.Fail("Impossibile caricare l'elenco competenze.");

            var nomeTrimmed = nome.Trim();
            var record = result.Data.Par_Competenza.FirstOrDefault(r =>
                string.Equals(r.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                record = result.Data.Par_Competenza.FirstOrDefault(r =>
                    r.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false);

            if (record == null)
                return CommandResult.Fail($"Competenza '{nome}' non trovata.");

            var payload = new NavigatePayload
            {
                Route = "/competenceedit",
                State = new Dictionary<string, object> { ["id"] = record.Id }
            };

            return CommandResult.Ok($"Apertura competenza '{record.Descrizione}' (id: {record.Id}).", payload);
        }
    }
}
