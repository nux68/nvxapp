using nvxapp.server.data.Localization;
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

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        // Keywords: unione italiano + inglese, qualunque sia la lingua dell'interfaccia.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenCommessa",
            DisplayName  = AttendanceTrackingMessages.Get("ChatOpenCommessaDisplayName"),
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = AttendanceTrackingMessages.Get("ChatOpenCommessaDescription"),
            // frasi di esempio per il riconoscimento dell'intent (italiano e inglese)
            Examples = new() { "apri la commessa 2026-01", "modifica la commessa cantiere Milano" },
            Pages = new() { "/commessalist", "/commessaedit" },
            IsNavigation = true,
            Keywords     = new() { "commessa", "apri commessa", "modifica commessa", "apri la commessa",
                                   "project", "job order", "open project", "edit project", "open job order" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome della commessa da aprire",
                    Question           = AttendanceTrackingMessages.Get("ChatOpenCommessaQuestion"),
                    Label              = AttendanceTrackingMessages.Get("ChatOpenCommessaLabel"),
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenCommessaNameMissing"));

            var result = await _az_CommessaService.GetAll(
                new GenericRequest<Az_Commessa_GetAll_InModel>(new Az_Commessa_GetAll_InModel()), false);

            if (!result.Success || result.Data?.Az_Commessa == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenCommessaLoadError"));

            var nomeTrimmed = nome.Trim();
            var record = result.Data.Az_Commessa.FirstOrDefault(r =>
                string.Equals(r.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                record = result.Data.Az_Commessa.FirstOrDefault(r =>
                    r.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false);

            if (record == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenCommessaNotFound", nome));

            var payload = new NavigatePayload
            {
                Route = "/commessaedit",
                State = new Dictionary<string, object> { ["id"] = record.Id }
            };

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatOpenCommessaOpening", record.Descrizione, record.Id), payload);
        }
    }
}
