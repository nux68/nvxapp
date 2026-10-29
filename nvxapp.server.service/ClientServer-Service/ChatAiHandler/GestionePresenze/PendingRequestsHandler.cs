using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Richieste dei dipendenti in attesa di approvazione (timbrature, giustificativi...), di tutta
    // l'azienda o di un dipendente.
    public class PendingRequestsHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;
        private readonly IPar_GiustificativiService _par_GiustificativiService;

        public PendingRequestsHandler(IDip_AnagraficaService dip_AnagraficaService,
                                      IDip_GG_RichiestaService dip_GG_RichiestaService,
                                      IPar_GiustificativiService par_GiustificativiService,
                                      IChatUserContext chatUserContext
                                      ) : base(dip_AnagraficaService, chatUserContext)
        {
            _dip_GG_RichiestaService = dip_GG_RichiestaService;
            _par_GiustificativiService = par_GiustificativiService;

            _intentDefinition = new IntentDefinition
            {
                Name = "PendingRequests",
                Category = AttendanceTrackingMessages.Get("ChatCategoryRequests"),
                CategoryOrder = 400,
                DisplayName = AttendanceTrackingMessages.Get("ChatPendingRequestsDisplayName"),
                Usable4Role = new List<string> { RuoloHR },
                Description = AttendanceTrackingMessages.Get("ChatPendingRequestsDescription"),
                Keywords = new() { "richieste in attesa", "da approvare", "richieste aperte", "pending requests", "to approve" },
                Examples = new() { "quali richieste devo approvare?", "ci sono richieste in attesa?", "richieste di Rossi da approvare", "any pending requests?" },
                RequiresConfirmation = false,
                Pages = new() { "/requestlistadmin" },
                Slots = BuildSlots(Facoltativo(EmployeeNameSlot, string.Empty))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var dipendenti = await Dipendenti(_dip_AnagraficaService);
            var perRapporto = PerRapporto(dipendenti);
            var richieste = await ChatRichiesteHelper.InAttesa(_dip_GG_RichiestaService);

            // filtro facoltativo sul dipendente
            if (slots.TryGetValue("employeeName", out var nome) && !string.IsNullOrWhiteSpace(nome))
            {
                var dipendente = await RisolviDipendente(nome, _dip_AnagraficaService, _chatUserContext);
                if (dipendente == null)
                    return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmployeeNotFoundRetry", nome));
                var rapporti = dipendente.Dip_RapportoLavoro.Select(r => r.Id).ToHashSet();
                richieste = richieste.Where(r => rapporti.Contains(r.IdDip_RapportoLavoro)).ToList();
            }

            if (richieste.Count == 0)
                return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatPendingNone"));

            var nomiGiustificativi = (await Giustificativi(_par_GiustificativiService))
                .ToDictionary(g => g.Id, g => g.Descrizione ?? g.Codice ?? string.Empty);

            var righe = richieste
                .OrderBy(r => ChatRichiesteHelper.Data(r.Data) ?? DateTime.MaxValue)
                .Select(r => ChatRichiesteHelper.Descrivi(r, perRapporto.GetValueOrDefault(r.IdDip_RapportoLavoro), nomiGiustificativi))
                .ToList();

            var sb = new StringBuilder();
            sb.AppendLine(AttendanceTrackingMessages.Get("ChatPendingTitle", righe.Count));
            foreach (var riga in righe.Take(20))
                sb.AppendLine("  • " + riga);
            if (righe.Count > 20)
                sb.AppendLine("  " + AttendanceTrackingMessages.Get("ChatListMore", righe.Count - 20));

            return CommandResult.Ok(sb.ToString().TrimEnd());
        }
    }
}
