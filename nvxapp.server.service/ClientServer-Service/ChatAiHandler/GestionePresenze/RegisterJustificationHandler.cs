using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Registra un giustificativo (es. ferie, malattia, ROL) per un dipendente in una data,
    // con le ore indicate o, se non indicate, a giornata intera (ore teoriche del giorno).
    // Cerca il tipo di giustificativo per nome/codice tra i Par_Giustificativi aziendali,
    // poi salva il record tramite Dip_GG_GiustificativiPut.
    public class RegisterJustificationHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_GiustificativiService _dip_GG_GiustificativiService;
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;
        private readonly IPar_GiustificativiService _par_GiustificativiService;

        public RegisterJustificationHandler(
            IDip_AnagraficaService dip_AnagraficaService,
            IDip_GG_GiustificativiService dip_GG_GiustificativiService,
            IDip_GG_RichiestaService dip_GG_RichiestaService,
            IPar_GiustificativiService par_GiustificativiService,
            IChatUserContext chatUserContext
        ) : base(dip_AnagraficaService, chatUserContext)
        {
            _dip_GG_GiustificativiService = dip_GG_GiustificativiService;
            _dip_GG_RichiestaService      = dip_GG_RichiestaService;
            _par_GiustificativiService    = par_GiustificativiService;

            var sb_Date = new StringBuilder();
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Data_Oggi(sb_Date);
            var datePromptDescription = sb_Date.ToString();

            _intentDefinition = new IntentDefinition
            {
                Name        = "RegisterJustification",
                Category    = AttendanceTrackingMessages.Get("ChatCategoryJustifications"),
                CategoryOrder = 200,
                DisplayName = AttendanceTrackingMessages.Get("ChatRegisterJustificationDisplayName"),
                Usable4Role = new List<string> { "CompanyPowerAdmin" },
                Description = AttendanceTrackingMessages.Get("ChatRegisterJustificationDescription"),
                // frasi di esempio per il riconoscimento dell'intent (italiano e inglese)
                Examples = new() { "metti 4 ore di ferie a Rossi per domani", "Bianchi ieri ha fatto 2 ore di permesso", "Verdi è stato in malattia lunedì, 8 ore", "register 8 hours of holiday for Rossi" },
                Pages = new() { "/poweradmintimesheet" },
                SummaryTemplate = AttendanceTrackingMessages.Get("ChatRegisterJustificationSummary"),
                Keywords    = new()
                {
                    "giustificativo", "giustifica",
                    "leave", "absence", "register leave", "add leave", "justification"
                },
                Slots = BuildSlots(
                    EmployeeNameSlot,
                    new()
                    {
                        Name               = "justification",
                        Type               = "string",
                        Required           = true,
                        PromptDescription  = "nome o codice del tipo di giustificativo (es. Ferie, Malattia, ROL)",
                        Question           = AttendanceTrackingMessages.Get("ChatRegisterJustificationTypeQuestion"),
                        Label              = AttendanceTrackingMessages.Get("ChatRegisterJustificationDisplayName"),
                        HasRelevantContent = msg => msg.Length > 0
                    },
                    new()
                    {
                        Name               = "hours",
                        Type               = "HH:mm o numero testuale",
                        // non indicate: giornata intera ("metti in ferie Rossi oggi")
                        Required           = false,
                        PromptDescription  = "solo se indicate (altrimenti giornata intera). " + HoursSlotHelper.PromptDescription,
                        Normalizer         = HoursSlotHelper.Normalize,
                        Question           = AttendanceTrackingMessages.Get("ChatRegisterJustificationHoursQuestion"),
                        Label              = AttendanceTrackingMessages.Get("ChatHoursLabel"),
                        Validator          = v =>
                        {
                            if (HoursSlotHelper.TryParse(v, out _)) return null;
                            return SlotValidationResult.Failed(
                                SlotValidationError.InvalidFormat,
                                AttendanceTrackingMessages.Get("ChatInvalidHours", v),
                                "hours");
                        },
                        HasRelevantContent = msg => HoursSlotHelper.TryParse(msg.Trim(), out _)
                    },
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

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var employeeName  = slots.GetValueOrDefault("employeeName", "");
            var justification = slots.GetValueOrDefault("justification", "");
            var hoursSlot     = slots.GetValueOrDefault("hours");
            var dateSlot      = slots.GetValueOrDefault("date", "oggi");

            // --- Risolvi dipendente ---
            Dip_AnagraficaModel? dipendente = Get_Dip_Anagrafica(employeeName);
            if (dipendente == null)
                return CommandResult.Fail(
                    AttendanceTrackingMessages.Get("ChatRegisterJustificationEmployeeNotFound", employeeName));

            if (dipendente.Dip_RapportoLavoro == null || dipendente.Dip_RapportoLavoro.Count == 0)
                return CommandResult.Fail(
                    AttendanceTrackingMessages.Get("ChatEmploymentNotFoundFor", employeeName));

            // --- Risolvi tipo giustificativo ---
            var parResult = await _par_GiustificativiService.GetAll(
                new GenericRequest<Par_GiustificativiInModel>(new Par_GiustificativiInModel()), false);

            if (!parResult.Success || parResult.Data?.Par_Giustificativi == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatGiustificativiLoadError"));

            var nomeTrimmed    = justification.Trim();
            var parGiustificativo = parResult.Data.Par_Giustificativi.FirstOrDefault(g =>
                string.Equals(g.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(g.Codice,      nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            // Ricerca parziale se non trovato esatto
            if (parGiustificativo == null)
                parGiustificativo = parResult.Data.Par_Giustificativi.FirstOrDefault(g =>
                    (g.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (g.Codice?.Contains(nomeTrimmed,      StringComparison.OrdinalIgnoreCase) ?? false));

            if (parGiustificativo == null)
                return CommandResult.Fail(
                    AttendanceTrackingMessages.Get("ChatJustificationTypeNotFound", justification));

            // giustificativo a importo: si registra con l'importo (RegisterJustificationAmount), non con le ore
            if (parGiustificativo.Tipo == TipoValore.Importo)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatJustificationIsAmount", parGiustificativo.Descrizione));

            // --- Risolvi data ---
            DateTime competenzaDate = ChatSlotWords.ParseDateOrToday(dateSlot);

            var idRapportoLavoro = dipendente.Dip_RapportoLavoro[0].Id;

            // --- Risolvi ore: quelle indicate, oppure giornata intera con le ore teoriche del giorno ---
            bool giornataIntera = !HoursSlotHelper.TryParse(hoursSlot ?? string.Empty, out var oreSpan);
            if (giornataIntera)
            {
                var giorni = _dip_GG_RichiestaService.GiorniGiustificativo(idRapportoLavoro, competenzaDate.Date, competenzaDate.Date, parGiustificativo.Id, null, true);
                if (giorni.Count == 0)
                    return CommandResult.Fail(AttendanceTrackingMessages.Get("RequestedPeriodNoWorkingDays"));
                oreSpan = giorni.Values.First();
            }
            var oreLabel = $"{(int)oreSpan.TotalHours:D2}:{oreSpan.Minutes:D2}";

            // --- Salva giustificativo ---

            var model = new Dip_GG_GiustificativiModel
            {
                Id                   = 0,
                IdDip_RapportoLavoro = idRapportoLavoro,
                Data                 = competenzaDate,
                IdPar_Giustificativi = parGiustificativo.Id,
                InputType            = giornataIntera ? JustificationInputType.AllDay : JustificationInputType.Manual,
                Hours                = oreSpan,
                RichiestaStato       = StatoRichiesta.Diretta
            };

            var req = new GenericRequest<Dip_GG_GiustificativiPutInModel>
            {
                Data = new Dip_GG_GiustificativiPutInModel
                {
                    IdDip_RapportoLavoro = idRapportoLavoro,
                    ExcludeRicalc        = false,
                    Dip_GG_Giustificativi = model
                }
            };

            var result = await _dip_GG_GiustificativiService.Dip_GG_GiustificativiPut(req, false);

            if (!result.Success)
                return CommandResult.Fail(
                    AttendanceTrackingMessages.Get("ChatRegisterJustificationError", string.Join(", ", result.Messages.Select(m => m.Text))));

            var dateLabel = competenzaDate.ToString("dd/MM/yyyy");
            return CommandResult.Ok(
                AttendanceTrackingMessages.Get("ChatRegisterJustificationDone", parGiustificativo.Descrizione, oreLabel, employeeName, dateLabel));
        }
    }
}
