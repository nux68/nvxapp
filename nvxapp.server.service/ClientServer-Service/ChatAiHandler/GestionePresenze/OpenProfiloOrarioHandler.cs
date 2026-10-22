using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_ProfiloOrarioService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina di modifica di un profilo orario specifico.
    // Cerca il profilo orario per nome/codice e restituisce un NavigatePayload
    // con route /profiliorariedit e state { id } per il router Angular.
    public class OpenProfiloOrarioHandler : BaseCommandHandler
    {
        private readonly IPar_ProfiloOrarioService _par_ProfiloOrarioService;

        public OpenProfiloOrarioHandler(IPar_ProfiloOrarioService par_ProfiloOrarioService)
        {
            _par_ProfiloOrarioService = par_ProfiloOrarioService;
        }

        // Definizione creata per istanza (handler Scoped): i testi sono nella lingua della richiesta.
        // Keywords: unione italiano + inglese, qualunque sia la lingua dell'interfaccia.
        private readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenProfiloOrario",
            DisplayName  = AttendanceTrackingMessages.Get("ChatOpenProfiloOrarioDisplayName"),
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = AttendanceTrackingMessages.Get("ChatOpenProfiloOrarioDescription"),
            IsNavigation = true,
            Keywords     = new() { "apri profilo orario", "modifica profilo orario", "profilo orario", "apri il profilo orario", "modifica il profilo orario",
                                   "open schedule profile", "edit schedule profile", "schedule profile" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome o codice del profilo orario da aprire",
                    Question           = AttendanceTrackingMessages.Get("ChatOpenProfiloOrarioQuestion"),
                    Label              = AttendanceTrackingMessages.Get("ChatOpenProfiloOrarioLabel"),
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenProfiloOrarioNameMissing"));

            var result = await _par_ProfiloOrarioService.GetAll(
                new GenericRequest<Par_ProfiloOrario_GetAllInModel>(new Par_ProfiloOrario_GetAllInModel()), false);

            if (!result.Success || result.Data?.Par_ProfiloOrario == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenProfiloOrarioLoadError"));

            var nomeTrimmed = nome.Trim();

            // Ricerca esatta per descrizione o codice
            var profilo = result.Data.Par_ProfiloOrario.FirstOrDefault(p =>
                string.Equals(p.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(p.Codice,      nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            // Ricerca parziale se non trovato esatto
            if (profilo == null)
                profilo = result.Data.Par_ProfiloOrario.FirstOrDefault(p =>
                    (p.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (p.Codice?.Contains(nomeTrimmed,      StringComparison.OrdinalIgnoreCase) ?? false));

            if (profilo == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOpenProfiloOrarioNotFound", nome));

            var payload = new NavigatePayload
            {
                Route = "/profiliorariedit",
                State = new Dictionary<string, object> { ["id"] = profilo.Id }
            };

            return CommandResult.Ok(
                AttendanceTrackingMessages.Get("ChatOpenProfiloOrarioOpening", profilo.Descrizione, profilo.Id),
                payload);
        }
    }
}
