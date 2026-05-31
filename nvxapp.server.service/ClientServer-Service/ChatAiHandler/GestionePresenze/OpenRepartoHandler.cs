using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_SediRepartoService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina di modifica di un reparto specifico.
    // Cerca il reparto per descrizione e restituisce un NavigatePayload
    // con route /departmentedit e state { id } per il router Angular.
    public class OpenRepartoHandler : BaseCommandHandler
    {
        private readonly IAz_SediRepartoService _az_SediRepartoService;

        public OpenRepartoHandler(IAz_SediRepartoService az_SediRepartoService)
        {
            _az_SediRepartoService = az_SediRepartoService;
        }

        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenReparto",
            DisplayName  = "Apri reparto",
            Description  = "apre la pagina di modifica di un reparto specifico.",
            IsNavigation = true,
            Keywords     = new() { "reparto", "apri reparto", "modifica reparto", "apri il reparto" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome del reparto da aprire",
                    Question           = "Quale reparto vuoi aprire?",
                    Label              = "Reparto",
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail("Nome reparto non specificato.");

            var result = await _az_SediRepartoService.GetAll(
                new GenericRequest<Az_SediReparto_GetAll_InModel>(new Az_SediReparto_GetAll_InModel()), false);

            if (!result.Success || result.Data?.Az_SediReparto == null)
                return CommandResult.Fail("Impossibile caricare l'elenco reparti.");

            var nomeTrimmed = nome.Trim();
            var record = result.Data.Az_SediReparto.FirstOrDefault(r =>
                string.Equals(r.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                record = result.Data.Az_SediReparto.FirstOrDefault(r =>
                    r.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false);

            if (record == null)
                return CommandResult.Fail($"Reparto '{nome}' non trovato.");

            var payload = new NavigatePayload
            {
                Route = "/departmentedit",
                State = new Dictionary<string, object> { ["id"] = record.Id }
            };

            return CommandResult.Ok($"Apertura reparto '{record.Descrizione}' (id: {record.Id}).", payload);
        }
    }
}
