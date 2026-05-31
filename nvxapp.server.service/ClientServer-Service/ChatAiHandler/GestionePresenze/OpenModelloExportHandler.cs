using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ExportCauService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina di modifica di un modello di export specifico.
    // Cerca il modello per nome/codice e restituisce un NavigatePayload
    // con route /exportcauedit e state { id } per il router Angular.
    public class OpenModelloExportHandler : BaseCommandHandler
    {
        private readonly IPar_ExportCauService _par_ExportCauService;

        public OpenModelloExportHandler(IPar_ExportCauService par_ExportCauService)
        {
            _par_ExportCauService = par_ExportCauService;
        }

        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenModelloExport",
            DisplayName  = "Apri modello export",
            Description  = "apre la pagina di modifica di un modello di export specifico.",
            IsNavigation = true,
            Keywords     = new() { "modello export", "apri modello export", "modifica modello export", "export causali" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome o codice del modello di export da aprire",
                    Question           = "Quale modello di export vuoi aprire?",
                    Label              = "Modello export",
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail("Nome modello export non specificato.");

            var result = await _par_ExportCauService.GetAll(
                new GenericRequest<Par_ExportCau_GetAll_InModel>(new Par_ExportCau_GetAll_InModel()), false);

            if (!result.Success || result.Data?.Par_ExportCau == null)
                return CommandResult.Fail("Impossibile caricare l'elenco modelli export.");

            var nomeTrimmed = nome.Trim();
            var record = result.Data.Par_ExportCau.FirstOrDefault(r =>
                string.Equals(r.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r.Codice,      nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                record = result.Data.Par_ExportCau.FirstOrDefault(r =>
                    (r.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.Codice?.Contains(nomeTrimmed,      StringComparison.OrdinalIgnoreCase) ?? false));

            if (record == null)
                return CommandResult.Fail($"Modello export '{nome}' non trovato.");

            var payload = new NavigatePayload
            {
                Route = "/exportcauedit",
                State = new Dictionary<string, object> { ["id"] = record.Id }
            };

            return CommandResult.Ok($"Apertura modello export '{record.Descrizione}' (id: {record.Id}).", payload);
        }
    }
}
