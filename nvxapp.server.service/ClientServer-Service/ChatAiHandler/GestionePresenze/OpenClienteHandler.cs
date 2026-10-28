using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_ClienteService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Az_ClienteService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina di modifica di un cliente specifico.
    // Cerca il cliente per descrizione e restituisce un NavigatePayload
    // con route /customeredit e state { id } per il router Angular.
    public class OpenClienteHandler : BaseCommandHandler
    {
        private readonly IAz_ClienteService _az_ClienteService;

        public OpenClienteHandler(IAz_ClienteService az_ClienteService)
        {
            _az_ClienteService = az_ClienteService;
        }

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        // Keywords: unione italiano + inglese, qualunque sia la lingua dell'interfaccia.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenCliente",
            DisplayName  = AttendanceTrackingMessages.Get("ChatOpenClienteDisplayName"),
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = AttendanceTrackingMessages.Get("ChatOpenClienteDescription"),
            // frasi di esempio per il riconoscimento dell'intent (italiano e inglese)
            Examples = new() { "apri il cliente Acme", "fammi vedere la scheda del cliente Rossi srl" },
            IsNavigation = true,
            Keywords     = new() { "cliente", "apri cliente", "modifica cliente", "apri il cliente",
                                   "customer", "client", "open customer", "edit customer", "open client" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome del cliente da aprire",
                    Question           = AttendanceTrackingMessages.Get("ChatOpenClienteQuestion"),
                    Label              = AttendanceTrackingMessages.Get("ChatOpenClienteLabel"),
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenClienteNameMissing"));

            var result = await _az_ClienteService.GetAll(
                new GenericRequest<Az_Cliente_GetAll_InModel>(new Az_Cliente_GetAll_InModel()), false);

            if (!result.Success || result.Data?.Az_Cliente == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenClienteLoadError"));

            var nomeTrimmed = nome.Trim();
            var record = result.Data.Az_Cliente.FirstOrDefault(r =>
                string.Equals(r.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                record = result.Data.Az_Cliente.FirstOrDefault(r =>
                    r.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false);

            if (record == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenClienteNotFound", nome));

            var payload = new NavigatePayload
            {
                Route = "/customeredit",
                State = new Dictionary<string, object> { ["id"] = record.Id }
            };

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatOpenClienteOpening", record.Descrizione, record.Id), payload);
        }
    }
}
