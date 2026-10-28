using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_OrarioService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina di modifica di un orario specifico.
    // Cerca l'orario per nome/codice e restituisce un NavigatePayload
    // con route /orariedit e state { id } per il router Angular.
    public class OpenOrarioHandler : BaseCommandHandler
    {
        private readonly IPar_OrarioService _par_OrarioService;

        public OpenOrarioHandler(IPar_OrarioService par_OrarioService)
        {
            _par_OrarioService = par_OrarioService;
        }

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        // Keywords: unione italiano + inglese, qualunque sia la lingua dell'interfaccia.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenOrario",
            DisplayName  = AttendanceTrackingMessages.Get("ChatOpenOrarioDisplayName"),
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = AttendanceTrackingMessages.Get("ChatOpenOrarioDescription"),
            // frasi di esempio per il riconoscimento dell'intent (italiano e inglese)
            Examples = new() { "apri l'orario turno mattina", "modifica l'orario 8-17" },
            IsNavigation = true,
            Keywords     = new() { "apri orario", "modifica orario", "apri l'orario", "modifica l'orario", "orario",
                                   "open work schedule", "edit work schedule", "work schedule", "working hours" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome o codice dell'orario da aprire",
                    Question           = AttendanceTrackingMessages.Get("ChatOpenOrarioQuestion"),
                    Label              = AttendanceTrackingMessages.Get("ChatOpenOrarioLabel"),
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenOrarioNameMissing"));

            var result = await _par_OrarioService.GetAll(
                new GenericRequest<Par_Orario_GetAllInModel>(new Par_Orario_GetAllInModel()), false);

            if (!result.Success || result.Data?.Par_Orario == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenOrarioLoadError"));

            var nomeTrimmed = nome.Trim();

            // Ricerca esatta per descrizione o codice
            var orario = result.Data.Par_Orario.FirstOrDefault(o =>
                string.Equals(o.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(o.Codice,      nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            // Ricerca parziale se non trovato esatto
            if (orario == null)
                orario = result.Data.Par_Orario.FirstOrDefault(o =>
                    (o.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (o.Codice?.Contains(nomeTrimmed,      StringComparison.OrdinalIgnoreCase) ?? false));

            if (orario == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenOrarioNotFound", nome));

            var payload = new NavigatePayload
            {
                Route = "/orariedit",
                State = new Dictionary<string, object> { ["id"] = orario.Id }
            };

            return CommandResult.Ok(
                AttendanceTrackingMessages.Get("ChatOpenOrarioOpening", orario.Descrizione, orario.Id),
                payload);
        }
    }
}
