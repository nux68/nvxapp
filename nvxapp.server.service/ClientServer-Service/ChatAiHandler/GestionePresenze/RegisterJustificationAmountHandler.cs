using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Registra un giustificativo a importo (es. rimborso, trasferta) per un dipendente in una data:
    // come RegisterJustification, ma con l'importo in euro al posto delle ore (Par_Giustificativi.Tipo = Importo).
    public class RegisterJustificationAmountHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_GiustificativiService _dip_GG_GiustificativiService;
        private readonly IPar_GiustificativiService _par_GiustificativiService;

        public RegisterJustificationAmountHandler(
            IDip_AnagraficaService dip_AnagraficaService,
            IDip_GG_GiustificativiService dip_GG_GiustificativiService,
            IPar_GiustificativiService par_GiustificativiService,
            IChatUserContext chatUserContext
        ) : base(dip_AnagraficaService, chatUserContext)
        {
            _dip_GG_GiustificativiService = dip_GG_GiustificativiService;
            _par_GiustificativiService    = par_GiustificativiService;

            var sb_Date = new StringBuilder();
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Data_Oggi(sb_Date);
            var datePromptDescription = sb_Date.ToString();

            _intentDefinition = new IntentDefinition
            {
                Name        = "RegisterJustificationAmount",
                Category    = AttendanceTrackingMessages.Get("ChatCategoryJustifications"),
                CategoryOrder = 201,
                DisplayName = AttendanceTrackingMessages.Get("ChatRegisterJustificationAmountDisplayName"),
                Usable4Role = new List<string> { "CompanyPowerAdmin" },
                Description = AttendanceTrackingMessages.Get("ChatRegisterJustificationAmountDescription"),
                Examples = new() { "metti un rimborso di 25 euro a Rossi per oggi", "trasferta di Bianchi ieri, 45,50 euro", "register a 30 euro refund for Rossi" },
                Pages = new() { "/poweradmintimesheet" },
                SummaryTemplate = AttendanceTrackingMessages.Get("ChatRegisterJustificationAmountSummary"),
                Keywords    = new() { "importo", "euro", "rimborso", "trasferta", "amount", "refund" },
                Slots = BuildSlots(
                    EmployeeNameSlot,
                    new()
                    {
                        Name               = "justification",
                        Type               = "string",
                        Required           = true,
                        PromptDescription  = "nome o codice del giustificativo a importo (es. Rimborso, Trasferta)",
                        Question           = AttendanceTrackingMessages.Get("ChatRegisterJustificationTypeQuestion"),
                        Label              = AttendanceTrackingMessages.Get("ChatRegisterJustificationDisplayName"),
                        HasRelevantContent = msg => msg.Length > 0
                    },
                    AmountSlot(true),
                    new()
                    {
                        Name               = "date",
                        Type               = "dd/MM/yyyy",
                        Required           = false,
                        Default            = "oggi",
                        PromptDescription  = datePromptDescription,
                        Question           = AttendanceTrackingMessages.Get("ChatDateQuestion"),
                        Label              = AttendanceTrackingMessages.Get("ChatDateLabel"),
                        Validator          = v =>
                        {
                            if (DateOnly.TryParseExact(v, "yyyy-MM-dd", out _)) return null;
                            if (DateOnly.TryParseExact(v, "dd/MM/yyyy",  out _)) return null;
                            return SlotValidationResult.Failed(
                                SlotValidationError.InvalidFormat,
                                AttendanceTrackingMessages.Get("ChatInvalidDate", v),
                                "date");
                        },
                        HasRelevantContent = ChatSlotWords.HasDateContent
                    }
                )
            };
        }

        /// <summary>slot "amount": importo in euro (obbligatorio qui, facoltativo nelle richieste)</summary>
        public static SlotDefinition AmountSlot(bool required) => new()
        {
            Name               = "amount",
            Type               = "importo in euro",
            Required           = required,
            PromptDescription  = (required ? "" : "solo per i giustificativi a importo (rimborsi, trasferte), se indicato. ") + ImportoSlotHelper.PromptDescription,
            Normalizer         = ImportoSlotHelper.Normalize,
            Question           = AttendanceTrackingMessages.Get("ChatAmountQuestion"),
            Label              = AttendanceTrackingMessages.Get("ChatAmountLabel"),
            Validator          = v => ImportoSlotHelper.TryParse(v, out _)
                ? null
                : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, AttendanceTrackingMessages.Get("ChatInvalidAmount", v), "amount"),
            HasRelevantContent = msg => System.Text.RegularExpressions.Regex.IsMatch(msg, @"\d")
        };

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var employeeName  = slots.GetValueOrDefault("employeeName", "");
            var justification = slots.GetValueOrDefault("justification", "");
            var dateSlot      = slots.GetValueOrDefault("date", "oggi");
            if (!ImportoSlotHelper.TryParse(slots.GetValueOrDefault("amount"), out var importo))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatInvalidAmount", slots.GetValueOrDefault("amount", "-")));

            Dip_AnagraficaModel? dipendente = Get_Dip_Anagrafica(employeeName);
            if (dipendente == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatRegisterJustificationEmployeeNotFound", employeeName));
            if (dipendente.Dip_RapportoLavoro == null || dipendente.Dip_RapportoLavoro.Count == 0)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmploymentNotFoundFor", employeeName));

            var parResult = await _par_GiustificativiService.GetAll(new GenericRequest<Par_GiustificativiInModel>(new Par_GiustificativiInModel()), false);
            if (!parResult.Success || parResult.Data?.Par_Giustificativi == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatGiustificativiLoadError"));

            // solo tra i giustificativi a importo: esatto, poi parziale
            var aImporto = parResult.Data.Par_Giustificativi.Where(g => g.Tipo == TipoValore.Importo).ToList();
            var nome = justification.Trim();
            var parGiustificativo = aImporto.FirstOrDefault(g =>
                                        string.Equals(g.Descrizione, nome, StringComparison.OrdinalIgnoreCase) ||
                                        string.Equals(g.Codice, nome, StringComparison.OrdinalIgnoreCase))
                                    ?? aImporto.FirstOrDefault(g =>
                                        (g.Descrizione?.Contains(nome, StringComparison.OrdinalIgnoreCase) ?? false) ||
                                        (g.Codice?.Contains(nome, StringComparison.OrdinalIgnoreCase) ?? false));
            if (parGiustificativo == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatJustificationNotAmount", justification));

            DateTime competenzaDate = ChatSlotWords.ParseDateOrToday(dateSlot);
            var idRapportoLavoro = dipendente.Dip_RapportoLavoro[0].Id;

            var req = new GenericRequest<Dip_GG_GiustificativiPutInModel>
            {
                Data = new Dip_GG_GiustificativiPutInModel
                {
                    IdDip_RapportoLavoro = idRapportoLavoro,
                    ExcludeRicalc        = false,
                    Dip_GG_Giustificativi = new Dip_GG_GiustificativiModel
                    {
                        Id                   = 0,
                        IdDip_RapportoLavoro = idRapportoLavoro,
                        Data                 = competenzaDate,
                        IdPar_Giustificativi = parGiustificativo.Id,
                        InputType            = JustificationInputType.Manual,
                        Importo              = importo,
                        RichiestaStato       = StatoRichiesta.Diretta
                    }
                }
            };

            var result = await _dip_GG_GiustificativiService.Dip_GG_GiustificativiPut(req, false);
            if (!result.Success)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatRegisterJustificationError", string.Join(", ", result.Messages.Select(m => m.Text))));

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatRegisterJustificationAmountDone",
                parGiustificativo.Descrizione, ImportoSlotHelper.Testo(importo), employeeName, competenzaDate.ToString("dd/MM/yyyy")));
        }
    }
}
