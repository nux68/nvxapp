using nvxapp.server.data.Localization;
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

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        // Keywords: unione italiano + inglese, qualunque sia la lingua dell'interfaccia.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenReparto",
            DisplayName  = AttendanceTrackingMessages.Get("ChatOpenRepartoDisplayName"),
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = AttendanceTrackingMessages.Get("ChatOpenRepartoDescription"),
            // frasi di esempio per il riconoscimento dell'intent (italiano e inglese)
            Examples = new() { "apri il reparto produzione", "modifica il reparto magazzino" },
            IsNavigation = true,
            Keywords     = new() { "reparto", "apri reparto", "modifica reparto", "apri il reparto",
                                   "department", "open department", "edit department" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome del reparto da aprire",
                    Question           = AttendanceTrackingMessages.Get("ChatOpenRepartoQuestion"),
                    Label              = AttendanceTrackingMessages.Get("ChatOpenRepartoLabel"),
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenRepartoNameMissing"));

            var result = await _az_SediRepartoService.GetAll(
                new GenericRequest<Az_SediReparto_GetAll_InModel>(new Az_SediReparto_GetAll_InModel()), false);

            if (!result.Success || result.Data?.Az_SediReparto == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenRepartoLoadError"));

            var nomeTrimmed = nome.Trim();
            var record = result.Data.Az_SediReparto.FirstOrDefault(r =>
                string.Equals(r.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                record = result.Data.Az_SediReparto.FirstOrDefault(r =>
                    r.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false);

            if (record == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenRepartoNotFound", nome));

            var payload = new NavigatePayload
            {
                Route = "/departmentedit",
                State = new Dictionary<string, object> { ["id"] = record.Id }
            };

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatOpenRepartoOpening", record.Descrizione, record.Id), payload);
        }
    }
}
