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

        private static readonly IntentDefinition _intentDefinition = new()
        {
            Name         = "OpenCliente",
            DisplayName  = "Apri cliente",
            Usable4Role = new List<string> { "CompanyPowerAdmin" },
            Description  = "apre la pagina di modifica di un cliente specifico.",
            IsNavigation = true,
            Keywords     = new() { "cliente", "apri cliente", "modifica cliente", "apri il cliente" },
            Slots        =
            [
                new()
                {
                    Name               = "nome",
                    Type               = "string",
                    Required           = true,
                    PromptDescription  = "nome del cliente da aprire",
                    Question           = "Quale cliente vuoi aprire?",
                    Label              = "Cliente",
                    HasRelevantContent = msg => msg.Length > 0
                }
            ]
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            slots.TryGetValue("nome", out var nome);

            if (string.IsNullOrWhiteSpace(nome))
                return CommandResult.Fail("Nome cliente non specificato.");

            var result = await _az_ClienteService.GetAll(
                new GenericRequest<Az_Cliente_GetAll_InModel>(new Az_Cliente_GetAll_InModel()), false);

            if (!result.Success || result.Data?.Az_Cliente == null)
                return CommandResult.Fail("Impossibile caricare l'elenco clienti.");

            var nomeTrimmed = nome.Trim();
            var record = result.Data.Az_Cliente.FirstOrDefault(r =>
                string.Equals(r.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            if (record == null)
                record = result.Data.Az_Cliente.FirstOrDefault(r =>
                    r.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false);

            if (record == null)
                return CommandResult.Fail($"Cliente '{nome}' non trovato.");

            var payload = new NavigatePayload
            {
                Route = "/customeredit",
                State = new Dictionary<string, object> { ["id"] = record.Id }
            };

            return CommandResult.Ok($"Apertura cliente '{record.Descrizione}' (id: {record.Id}).", payload);
        }
    }
}
