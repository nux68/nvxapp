using nvxapp.server.data.Localization;
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

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        // Keywords: unione italiano + inglese, qualunque sia la lingua dell'interfaccia.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenModelloExport",
            DisplayName  = AttendanceTrackingMessages.Get("ChatOpenModelloExportDisplayName"),
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = AttendanceTrackingMessages.Get("ChatOpenModelloExportDescription"),
            // frasi di esempio per il riconoscimento dell'intent (italiano e inglese)
            Examples = new() { "apri il modello di export Zucchetti", "modifica il modello export paghe" },
            Pages = new() { "/exportcaulist", "/exportcauedit" },
            IsNavigation = true,
            Keywords     = new() { "modello export", "apri modello export", "modifica modello export", "export causali",
                                   "export template", "open export template", "edit export template", "export layout" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome o codice del modello di export da aprire",
                    Question           = AttendanceTrackingMessages.Get("ChatOpenModelloExportQuestion"),
                    Label              = AttendanceTrackingMessages.Get("ChatOpenModelloExportLabel"),
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenModelloExportNameMissing"));

            var result = await _par_ExportCauService.GetAll(
                new GenericRequest<Par_ExportCau_GetAll_InModel>(new Par_ExportCau_GetAll_InModel()), false);

            if (!result.Success || result.Data?.Par_ExportCau == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenModelloExportLoadError"));

            var nomeTrimmed = nome.Trim();
            var record = result.Data.Par_ExportCau.FirstOrDefault(r =>
                string.Equals(r.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(r.Codice,      nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                record = result.Data.Par_ExportCau.FirstOrDefault(r =>
                    (r.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (r.Codice?.Contains(nomeTrimmed,      StringComparison.OrdinalIgnoreCase) ?? false));

            if (record == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenModelloExportNotFound", nome));

            var payload = new NavigatePayload
            {
                Route = "/exportcauedit",
                State = new Dictionary<string, object> { ["id"] = record.Id }
            };

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatOpenModelloExportOpening", record.Descrizione, record.Id), payload);
        }
    }
}
