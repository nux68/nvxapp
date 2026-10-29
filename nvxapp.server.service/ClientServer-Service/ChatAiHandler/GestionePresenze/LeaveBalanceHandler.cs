using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ContatoriService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.ContatoriService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Saldi dei giustificativi con contatore (ferie, permessi, ROL...) di un dipendente a fine mese corrente
    // (ContatoriService, periodo "fine mese": riporto + maturato - goduto fino al mese corrente).
    // L'HR consulta tutti, gli altri ruoli solo i propri dati (dipendente non indicato = sé stessi).
    public class LeaveBalanceHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IContatoriService _contatoriService;

        public LeaveBalanceHandler(IDip_AnagraficaService dip_AnagraficaService,
                                   IContatoriService contatoriService,
                                   IChatUserContext chatUserContext
                                   ) : base(dip_AnagraficaService, chatUserContext)
        {
            _contatoriService = contatoriService;

            _intentDefinition = new IntentDefinition
            {
                Name = "LeaveBalance",
                Category = AttendanceTrackingMessages.Get("ChatCategoryLeave"),
                CategoryOrder = 300,
                DisplayName = AttendanceTrackingMessages.Get("ChatLeaveBalanceDisplayName"),
                Usable4Role = new List<string> { RuoloHR, RuoloAdmin, RuoloUser },
                Description = AttendanceTrackingMessages.Get("ChatLeaveBalanceDescription"),
                Keywords = new() { "saldo", "saldi", "residuo", "residui", "quante ferie", "balance", "remaining" },
                Examples = new() { "quante ferie restano a Rossi?", "quanti permessi mi rimangono?", "saldi di Bianchi", "how much leave do I have left?" },
                RequiresConfirmation = false,
                Pages = new() { "/usertimesheet", "/poweradmintimesheet", "/requestjustificationuser" },
                Slots = BuildSlots(
                    Facoltativo(EmployeeNameSlot, ChatSelf.Token),
                    new SlotDefinition
                    {
                        Name = "justification",
                        Type = "string",
                        Required = false,
                        PromptDescription = "giustificativo di cui si chiede il saldo, se indicato (es. ferie, permessi, ROL)",
                        Question = AttendanceTrackingMessages.Get("ChatRegisterJustificationTypeQuestion"),
                        Label = AttendanceTrackingMessages.Get("ChatRegisterJustificationDisplayName")
                    })
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var dipendente = await RisolviDipendente(slots.GetValueOrDefault("employeeName", ChatSelf.Token), _dip_AnagraficaService, _chatUserContext);
            if (dipendente == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmployeeSelfNotFound"));
            if (!PuoAccedere(_chatUserContext, dipendente))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatOnlyOwnData"));
            if (dipendente.Dip_RapportoLavoro.Count == 0)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmploymentNotFoundFor", Nome(dipendente)));

            var oggi = DateTime.Today;
            var res = await _contatoriService.CalcolaContatori(new GenericRequest<Contatori_Calcolo_InModel>
            {
                Data = new() { IdDip_RapportoLavoro = dipendente.Dip_RapportoLavoro[0].Id, Anno = oggi.Year, Mese = oggi.Month }
            }, true);
            if (!res.Success || res.Data == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatDataReadError", string.Join(", ", res.Messages.Select(m => m.Text))));

            // filtro facoltativo sul giustificativo ("ferie" -> "Ferie", "FER")
            var filtro = slots.GetValueOrDefault("justification", string.Empty).Trim();
            var risultati = res.Data.Risultati
                .Where(r => filtro.Length == 0 ||
                            r.Descrizione.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                            r.Codice.Contains(filtro, StringComparison.OrdinalIgnoreCase) ||
                            filtro.Contains(r.Descrizione, StringComparison.OrdinalIgnoreCase))
                .ToList();
            // il nome detto non corrisponde a nessun contatore ("permessi" con "ROL", "Banca ore"): tutti i saldi
            if (risultati.Count == 0)
                risultati = res.Data.Risultati;
            if (risultati.Count == 0)
                return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatLeaveBalanceNone", Nome(dipendente)));

            var sb = new StringBuilder();
            sb.AppendLine(AttendanceTrackingMessages.Get("ChatLeaveBalanceTitle", Nome(dipendente), Mese(oggi.Year, oggi.Month)));
            foreach (var r in risultati)
                sb.AppendLine("  • " + AttendanceTrackingMessages.Get("ChatLeaveBalanceLine", r.Descrizione,
                    OreContatore(r.PeriodoFineMese.Saldo), OreContatore(r.PeriodoFineMese.Maturato), OreContatore(r.PeriodoFineMese.Goduto)));

            return CommandResult.Ok(sb.ToString().TrimEnd());
        }
    }
}
