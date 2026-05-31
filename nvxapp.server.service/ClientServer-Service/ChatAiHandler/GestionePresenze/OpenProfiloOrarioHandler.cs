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

        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenProfiloOrario",
            DisplayName  = "Apri profilo orario",
            Description  = "apre la pagina di modifica di un profilo orario specifico.",
            IsNavigation = true,
            Keywords     = new() { "apri profilo orario", "modifica profilo orario", "profilo orario", "apri il profilo orario", "modifica il profilo orario" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome o codice del profilo orario da aprire",
                    Question           = "Quale profilo orario vuoi aprire?",
                    Label              = "Profilo orario",
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail("Nome profilo orario non specificato.");

            var result = await _par_ProfiloOrarioService.GetAll(
                new GenericRequest<Par_ProfiloOrario_GetAllInModel>(new Par_ProfiloOrario_GetAllInModel()), false);

            if (!result.Success || result.Data?.Par_ProfiloOrario == null)
                return CommandResult.Fail("Impossibile caricare l'elenco profili orario.");

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
                return CommandResult.Fail($"Profilo orario '{nome}' non trovato.");

            var payload = new NavigatePayload
            {
                Route = "/profiliorariedit",
                State = new Dictionary<string, object> { ["id"] = profilo.Id }
            };

            return CommandResult.Ok(
                $"Apertura profilo orario '{profilo.Descrizione}' (id: {profilo.Id}).",
                payload);
        }
    }
}
