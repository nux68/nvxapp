using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_CommessaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina di modifica di una commessa specifica.
    // Cerca la commessa per descrizione e restituisce un NavigatePayload
    // con route /commessaedit e state { id } per il router Angular.
    public class OpenCommessaHandler : BaseCommandHandler
    {
        private readonly IAz_CommessaService _az_CommessaService;

        public OpenCommessaHandler(IAz_CommessaService az_CommessaService)
        {
            _az_CommessaService = az_CommessaService;
        }

        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenCommessa",
            DisplayName  = "Apri commessa",
            Description  = "apre la pagina di modifica di una commessa specifica.",
            IsNavigation = true,
            Keywords     = new() { "commessa", "apri commessa", "modifica commessa", "apri la commessa" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome della commessa da aprire",
                    Question           = "Quale commessa vuoi aprire?",
                    Label              = "Commessa",
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail("Nome commessa non specificato.");

            var result = await _az_CommessaService.GetAll(
                new GenericRequest<Az_Commessa_GetAll_InModel>(new Az_Commessa_GetAll_InModel()), false);

            if (!result.Success || result.Data?.Az_Commessa == null)
                return CommandResult.Fail("Impossibile caricare l'elenco commesse.");

            var nomeTrimmed = nome.Trim();
            var record = result.Data.Az_Commessa.FirstOrDefault(r =>
                string.Equals(r.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                record = result.Data.Az_Commessa.FirstOrDefault(r =>
                    r.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false);

            if (record == null)
                return CommandResult.Fail($"Commessa '{nome}' non trovata.");

            var payload = new NavigatePayload
            {
                Route = "/commessaedit",
                State = new Dictionary<string, object> { ["id"] = record.Id }
            };

            return CommandResult.Ok($"Apertura commessa '{record.Descrizione}' (id: {record.Id}).", payload);
        }
    }
}
