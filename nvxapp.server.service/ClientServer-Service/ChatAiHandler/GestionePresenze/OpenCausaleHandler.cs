using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina di modifica di una causale specifica.
    // Cerca la causale per nome/codice e restituisce un NavigatePayload
    // con route /causaliedit e state { id } per il router Angular.
    public class OpenCausaleHandler : BaseCommandHandler
    {
        private readonly IPar_CausaliService _par_CausaliService;

        public OpenCausaleHandler(IPar_CausaliService par_CausaliService)
        {
            _par_CausaliService = par_CausaliService;
        }

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        // Keywords: unione italiano + inglese, qualunque sia la lingua dell'interfaccia.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenCausale",
            DisplayName  = AttendanceTrackingMessages.Get("ChatOpenCausaleDisplayName"),
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = AttendanceTrackingMessages.Get("ChatOpenCausaleDescription"),
            // frasi di esempio per il riconoscimento dell'intent (italiano e inglese)
            Examples = new() { "apri la causale straordinario", "modifica la causale ferie" },
            Pages = new() { "/causalilist", "/causaliedit" },
            IsNavigation = true,
            Keywords     = new() { "modifica causale", "apri causale", "apri la causale", "modifica la causale", "causale",
                                   "edit attendance code", "open attendance code", "attendance code", "pay code" },
            Slots       =
            [
                new()
                {
                    Name              = "nome",
                    Type              = "string",
                    Required          = true,
                    PromptDescription = "nome o codice della causale da aprire",
                    Question          = AttendanceTrackingMessages.Get("ChatOpenCausaleQuestion"),
                    Label             = AttendanceTrackingMessages.Get("ChatOpenCausaleLabel"),
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenCausaleNameMissing"));

            // Carica tutte le causali e cerca per descrizione o codice (case-insensitive)
            var result = await _par_CausaliService.GetAll(
                new GenericRequest<Par_CausaliInModel>(new Par_CausaliInModel()), false);

            if (!result.Success || result.Data?.Par_Causali == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatCausaliLoadError"));

            var nomeTrimmed = nome.Trim();
            var causale = result.Data.Par_Causali.FirstOrDefault(c =>
                string.Equals(c.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Codice, nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            // Ricerca parziale se non trovata esatta
            if (causale == null)
                causale = result.Data.Par_Causali.FirstOrDefault(c =>
                    (c.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Codice?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false));

            if (causale == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatCausaleNotFound", nome));

            var payload = new NavigatePayload
            {
                Route = "/causaliedit",
                State = new Dictionary<string, object> { ["id"] = causale.Id }
            };

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatOpenCausaleOpening", causale.Descrizione, causale.Id), payload);
        }
    }
}
