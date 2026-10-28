using nvxapp.server.data.Localization;
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

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        // Keywords: unione italiano + inglese, qualunque sia la lingua dell'interfaccia.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenCompetenza",
            DisplayName  = AttendanceTrackingMessages.Get("ChatOpenCompetenzaDisplayName"),
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = AttendanceTrackingMessages.Get("ChatOpenCompetenzaDescription"),
            // frasi di esempio per il riconoscimento dell'intent (italiano e inglese)
            Examples = new() { "apri la competenza saldatura", "modifica la competenza inglese" },
            Pages = new() { "/competencelist", "/competenceedit" },
            IsNavigation = true,
            Keywords     = new() { "competenza", "apri competenza", "modifica competenza", "apri la competenza",
                                   "skill", "competence", "open skill", "edit skill" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome della competenza da aprire",
                    Question           = AttendanceTrackingMessages.Get("ChatOpenCompetenzaQuestion"),
                    Label              = AttendanceTrackingMessages.Get("ChatOpenCompetenzaLabel"),
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenCompetenzaNameMissing"));

            var result = await _par_CompetenzaService.GetAll(
                new GenericRequest<Par_Competenza_GetAll_InModel>(new Par_Competenza_GetAll_InModel()), false);

            if (!result.Success || result.Data?.Par_Competenza == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenCompetenzaLoadError"));

            var nomeTrimmed = nome.Trim();
            var record = result.Data.Par_Competenza.FirstOrDefault(r =>
                string.Equals(r.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                record = result.Data.Par_Competenza.FirstOrDefault(r =>
                    r.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false);

            if (record == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenCompetenzaNotFound", nome));

            var payload = new NavigatePayload
            {
                Route = "/competenceedit",
                State = new Dictionary<string, object> { ["id"] = record.Id }
            };

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatOpenCompetenzaOpening", record.Descrizione, record.Id), payload);
        }
    }
}
