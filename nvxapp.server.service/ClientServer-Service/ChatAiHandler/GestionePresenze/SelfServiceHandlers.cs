using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze._utility;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Comandi self-service del dipendente: agiscono sempre sull'utente corrente, come le pagine
    // "Timbratura", "Richiesta giustificativo" e "Richiesta timbratura". Tutti con conferma.

    // Timbratura dell'utente in questo momento (come il pulsante della pagina Timbratura).
    public class ClockNowHandler : BaseCommandHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;

        public ClockNowHandler(IDip_GG_TimbraturaService dip_GG_TimbraturaService)
        {
            _dip_GG_TimbraturaService = dip_GG_TimbraturaService;

            _intentDefinition = new IntentDefinition
            {
                Name = "ClockNow",
                Category = AttendanceTrackingMessages.Get("ChatCategoryClockings"),
                CategoryOrder = 100,
                DisplayName = AttendanceTrackingMessages.Get("ChatClockNowDisplayName"),
                Usable4Role = new List<string> { RuoloUser, RuoloAdmin, RuoloHR },
                Description = AttendanceTrackingMessages.Get("ChatClockNowDescription"),
                Keywords = new() { "timbra adesso", "timbrami", "timbro", "sono arrivato", "sto uscendo", "clock me in", "clock me out" },
                Examples = new() { "timbra la mia entrata", "sono arrivato, timbrami", "sto uscendo", "clock me in now" },
                SummaryTemplate = AttendanceTrackingMessages.Get("ChatClockNowSummary"),
                Pages = new() { "/timeclockuser" }
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var adesso = DateTime.Now;
            // attività 0: il servizio usa quella proposta per il dipendente
            var res = await _dip_GG_TimbraturaService.Stamp(new GenericRequest<Dip_GG_Timbratura_Stamp_InModel>
            {
                Data = new()
                {
                    DateStamp = adesso.ToString("yyyy-MM-ddTHH:mm:ss", CultureInfo.InvariantCulture),
                    IdAz_SubCommessaAttivita = 0,
                    ExcludeRicalc = false
                }
            }, false);

            if (!res.Success)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatClockNowError", string.Join(", ", res.Messages.Select(m => m.Text))));

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatClockNowDone", adesso.ToString("HH:mm")));
        }
    }

    // Richiesta di un giustificativo (ferie, permesso...) per sé, da approvare.
    public class RequestLeaveHandler : BaseCommandHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;
        private readonly IPar_GiustificativiService _par_GiustificativiService;

        public RequestLeaveHandler(IDip_GG_RichiestaService dip_GG_RichiestaService,
                                   IPar_GiustificativiService par_GiustificativiService)
        {
            _dip_GG_RichiestaService = dip_GG_RichiestaService;
            _par_GiustificativiService = par_GiustificativiService;

            _intentDefinition = new IntentDefinition
            {
                Name = "RequestLeave",
                Category = AttendanceTrackingMessages.Get("ChatCategoryLeave"),
                CategoryOrder = 300,
                DisplayName = AttendanceTrackingMessages.Get("ChatRequestLeaveDisplayName"),
                Usable4Role = new List<string> { RuoloUser, RuoloAdmin, RuoloHR },
                Description = AttendanceTrackingMessages.Get("ChatRequestLeaveDescription"),
                Keywords = new() { "chiedo", "vorrei", "richiedo", "richiesta di ferie", "prendere ferie", "request leave", "i'd like" },
                Examples = new() { "chiedo ferie dal 3 al 7 novembre", "vorrei 2 ore di permesso venerdì", "richiedo un giorno di ROL per lunedì", "I'd like to take holiday from 3 to 7 November" },
                SummaryTemplate = AttendanceTrackingMessages.Get("ChatRequestLeaveSummary"),
                Pages = new() { "/requestjustificationuser", "/requestlistuser" },
                CrossValidator = slots =>
                    slots.TryGetValue("from", out var dal) && slots.TryGetValue("to", out var al) && ParseData(al) < ParseData(dal)
                        ? SlotValidationResult.Failed(SlotValidationError.BusinessRuleViolation, AttendanceTrackingMessages.Get("ChatPeriodInvalid"), "to")
                        : null,
                Slots = BuildSlots(
                    new SlotDefinition
                    {
                        Name = "justification",
                        Type = "string",
                        Required = true,
                        PromptDescription = "nome o codice del giustificativo richiesto (es. Ferie, Permesso, ROL)",
                        Question = AttendanceTrackingMessages.Get("ChatRegisterJustificationTypeQuestion"),
                        Label = AttendanceTrackingMessages.Get("ChatRegisterJustificationDisplayName")
                    },
                    SlotData("from", true, string.Empty, AttendanceTrackingMessages.Get("ChatPeriodFromQuestion"), AttendanceTrackingMessages.Get("ChatFromLabel"), SignificatoDal),
                    SlotData("to", false, string.Empty, AttendanceTrackingMessages.Get("ChatPeriodToQuestion"), AttendanceTrackingMessages.Get("ChatToLabel"), SignificatoAl),
                    new SlotDefinition
                    {
                        Name = "hours",
                        Type = "HH:mm o numero testuale",
                        Required = false,
                        PromptDescription = "ore al giorno, solo se indicate (altrimenti giornata intera). " + HoursSlotHelper.PromptDescription,
                        Question = AttendanceTrackingMessages.Get("ChatRegisterJustificationHoursQuestion"),
                        Label = AttendanceTrackingMessages.Get("ChatHoursLabel"),
                        Normalizer = HoursSlotHelper.Normalize
                    },
                    RegisterJustificationAmountHandler.AmountSlot(false))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var nomeGiustificativo = slots.GetValueOrDefault("justification", string.Empty);
            var giustificativo = TrovaGiustificativo(await Giustificativi(_par_GiustificativiService), nomeGiustificativo);
            if (giustificativo == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatJustificationTypeNotFound", nomeGiustificativo));

            var dal = ParseData(slots.GetValueOrDefault("from"));
            var al = slots.TryGetValue("to", out var a) ? ParseData(a) : dal;
            TimeSpan? ore = slots.TryGetValue("hours", out var h) && HoursSlotHelper.TryParse(h, out var ts) ? ts : null;

            // giustificativo a importo (es. rimborso): l'importo (per giorno) e' obbligatorio
            decimal? importo = null;
            if (giustificativo.Tipo == nvxapp.server.data.Entities.Tenant.TipoValore.Importo)
            {
                if (!ImportoSlotHelper.TryParse(slots.GetValueOrDefault("amount"), out var imp))
                    return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatJustificationAmountMissing", giustificativo.Descrizione ?? nomeGiustificativo));
                importo = imp;
            }

            // null = utente corrente; non dall'HR: resta in attesa di approvazione
            var errore = await ChatInvioRichiestaHelper.Giustificativo(_dip_GG_RichiestaService, null, false, dal, al, giustificativo.Id, ore, importo);
            if (errore != null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatRequestError", errore));

            var periodo = dal == al ? Data(dal) : $"{Data(dal)} - {Data(al)}";
            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatRequestSent", $"{giustificativo.Descrizione ?? nomeGiustificativo} {periodo}"));
        }
    }

    // Richiesta di una timbratura per sé (es. dimenticata), da approvare.
    public class RequestClockingHandler : BaseCommandHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;
        private readonly IGestionePresenzeUserUtility _gestionePresenzeUserUtility;
        private readonly IChatUserContext _chatUserContext;

        public RequestClockingHandler(IDip_GG_RichiestaService dip_GG_RichiestaService,
                                      IGestionePresenzeUserUtility gestionePresenzeUserUtility,
                                      IChatUserContext chatUserContext)
        {
            _dip_GG_RichiestaService = dip_GG_RichiestaService;
            _gestionePresenzeUserUtility = gestionePresenzeUserUtility;
            _chatUserContext = chatUserContext;

            _intentDefinition = new IntentDefinition
            {
                Name = "RequestClocking",
                Category = AttendanceTrackingMessages.Get("ChatCategoryClockings"),
                CategoryOrder = 100,
                DisplayName = AttendanceTrackingMessages.Get("ChatRequestClockingDisplayName"),
                Usable4Role = new List<string> { RuoloUser, RuoloAdmin, RuoloHR },
                Description = AttendanceTrackingMessages.Get("ChatRequestClockingDescription"),
                Keywords = new() { "ho dimenticato di timbrare", "richiesta timbratura", "dimenticato", "forgot to clock" },
                Examples = new() { "ho dimenticato di timbrare ieri alle 9", "chiedo una timbratura per stamattina alle 8:30", "I forgot to clock in yesterday at 9" },
                SummaryTemplate = AttendanceTrackingMessages.Get("ChatRequestClockingSummary"),
                Pages = new() { "/requestclockinguser", "/requestlistuser" },
                Slots = BuildSlots(
                    SlotOrario("time", true, AttendanceTrackingMessages.Get("ChatRegisterClockingTimeQuestion"), AttendanceTrackingMessages.Get("ChatTimeLabel")),
                    SlotData("date", false, "oggi", AttendanceTrackingMessages.Get("ChatDateQuestion"), AttendanceTrackingMessages.Get("ChatDateLabel")))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var orario = slots.GetValueOrDefault("time", string.Empty);
            if (!TimeOnly.TryParseExact(orario, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var ora))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatInvalidTime", orario));
            var giorno = ParseData(slots.GetValueOrDefault("date", "oggi"));

            // attività del rapporto di lavoro dell'utente (quella proposta dalla pagina)
            var utente = await _gestionePresenzeUserUtility.Get_DipAna_DipRapp(_chatUserContext.UserId, false);
            if (utente?.dip_RapportoLavoro == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmployeeSelfNotFound"));

            var errore = await ChatInvioRichiestaHelper.Timbratura(_dip_GG_RichiestaService, null, false, giorno, ora,
                utente.dip_RapportoLavoro.IdAz_SubCommessaAttivita);
            if (errore != null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatRequestError", errore));

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatRequestSent", $"{orario} {Data(giorno)}"));
        }
    }
}
