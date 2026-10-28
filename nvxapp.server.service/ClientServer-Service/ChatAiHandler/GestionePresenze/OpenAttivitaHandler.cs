using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_AttivitaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina di modifica di un'attività specifica.
    // Cerca l'attività per descrizione e restituisce un NavigatePayload
    // con route /activityedit e state { id } per il router Angular.
    public class OpenAttivitaHandler : BaseCommandHandler
    {
        private readonly IPar_AttivitaService _par_AttivitaService;

        public OpenAttivitaHandler(IPar_AttivitaService par_AttivitaService)
        {
            _par_AttivitaService = par_AttivitaService;
        }

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        // Keywords: unione italiano + inglese, qualunque sia la lingua dell'interfaccia.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenAttivita",
            DisplayName  = AttendanceTrackingMessages.Get("ChatOpenAttivitaDisplayName"),
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = AttendanceTrackingMessages.Get("ChatOpenAttivitaDescription"),
            // frasi di esempio per il riconoscimento dell'intent (italiano e inglese)
            Examples = new() { "apri l'attività sviluppo", "modifica l'attività manutenzione" },
            Pages = new() { "/activitylist", "/activityedit" },
            IsNavigation = true,
            Keywords     = new() { "attività", "attivita", "apri attività", "modifica attività", "apri l'attività",
                                   "activity", "open activity", "edit activity", "open the activity" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome dell'attività da aprire",
                    Question           = AttendanceTrackingMessages.Get("ChatOpenAttivitaQuestion"),
                    Label              = AttendanceTrackingMessages.Get("ChatOpenAttivitaLabel"),
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenAttivitaNameMissing"));

            var result = await _par_AttivitaService.GetAll(
                new GenericRequest<Par_Attivita_GetAll_InModel>(new Par_Attivita_GetAll_InModel()), false);

            if (!result.Success || result.Data?.Par_Attivita == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenAttivitaLoadError"));

            var nomeTrimmed = nome.Trim();
            var record = result.Data.Par_Attivita.FirstOrDefault(r =>
                string.Equals(r.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                record = result.Data.Par_Attivita.FirstOrDefault(r =>
                    r.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false);

            if (record == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenAttivitaNotFound", nome));

            var payload = new NavigatePayload
            {
                Route = "/activityedit",
                State = new Dictionary<string, object> { ["id"] = record.Id }
            };

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatOpenAttivitaOpening", record.Descrizione, record.Id), payload);
        }
    }
}
