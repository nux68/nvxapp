using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina di modifica di un giustificativo specifico.
    // Cerca il giustificativo per nome/codice e restituisce un NavigatePayload
    // con route /justificationedit e state { id } per il router Angular.
    public class OpenGiustificativoHandler : BaseCommandHandler
    {
        private readonly IPar_GiustificativiService _par_GiustificativiService;

        public OpenGiustificativoHandler(IPar_GiustificativiService par_GiustificativiService)
        {
            _par_GiustificativiService = par_GiustificativiService;
        }

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        // Keywords: unione italiano + inglese, qualunque sia la lingua dell'interfaccia.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenGiustificativo",
            DisplayName  = AttendanceTrackingMessages.Get("ChatOpenGiustificativoDisplayName"),
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = AttendanceTrackingMessages.Get("ChatOpenGiustificativoDescription"),
            // frasi di esempio per il riconoscimento dell'intent (italiano e inglese)
            Examples = new() { "apri il giustificativo ferie", "modifica il giustificativo ROL" },
            IsNavigation = true,
            Keywords     = new() { "modifica giustificativo", "apri giustificativo", "apri il giustificativo", "modifica il giustificativo", "giustificativo",
                                   "edit leave type", "open leave type", "leave type", "absence type" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome o codice del giustificativo da aprire",
                    Question           = AttendanceTrackingMessages.Get("ChatOpenGiustificativoQuestion"),
                    Label              = AttendanceTrackingMessages.Get("ChatOpenGiustificativoLabel"),
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenGiustificativoNameMissing"));

            var result = await _par_GiustificativiService.GetAll(
                new GenericRequest<Par_GiustificativiInModel>(new Par_GiustificativiInModel()), false);

            if (!result.Success || result.Data?.Par_Giustificativi == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatGiustificativiLoadError"));

            var nomeTrimmed = nome.Trim();

            // Ricerca esatta per descrizione o codice
            var giustificativo = result.Data.Par_Giustificativi.FirstOrDefault(g =>
                string.Equals(g.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(g.Codice, nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            // Ricerca parziale se non trovato esatto
            if (giustificativo == null)
                giustificativo = result.Data.Par_Giustificativi.FirstOrDefault(g =>
                    (g.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (g.Codice?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false));

            if (giustificativo == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenGiustificativoNotFound", nome));

            var payload = new NavigatePayload
            {
                Route = "/justificationedit",
                State = new Dictionary<string, object> { ["id"] = giustificativo.Id }
            };

            return CommandResult.Ok(
                AttendanceTrackingMessages.Get("ChatOpenGiustificativoOpening", giustificativo.Descrizione, giustificativo.Id),
                payload);
        }
    }
}
