using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Giustificativo di un dipendente su un periodo (es. ferie dal 3 al 7), a giornata intera o con le ore
    // indicate. Si registra come richiesta già approvata dall'HR, come dal Calendario HR: il servizio
    // la applica ai giorni del periodo. Solo HR, con conferma.
    public class RegisterJustificationPeriodHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;
        private readonly IPar_GiustificativiService _par_GiustificativiService;

        public RegisterJustificationPeriodHandler(IDip_AnagraficaService dip_AnagraficaService,
                                                  IDip_GG_RichiestaService dip_GG_RichiestaService,
                                                  IPar_GiustificativiService par_GiustificativiService,
                                                  IChatUserContext chatUserContext
                                                  ) : base(dip_AnagraficaService, chatUserContext)
        {
            _dip_GG_RichiestaService = dip_GG_RichiestaService;
            _par_GiustificativiService = par_GiustificativiService;

            _intentDefinition = new IntentDefinition
            {
                Name = "RegisterJustificationPeriod",
                DisplayName = AttendanceTrackingMessages.Get("ChatJustificationPeriodDisplayName"),
                Usable4Role = new List<string> { RuoloHR },
                Description = AttendanceTrackingMessages.Get("ChatJustificationPeriodDescription"),
                Keywords = new() { "dal", "al", "periodo", "settimana di", "from", "until" },
                Examples = new() { "metti in ferie Rossi dal 3 al 7 novembre", "Bianchi in malattia da lunedì a mercoledì", "register holiday for Rossi from 3 to 7 November" },
                SummaryTemplate = AttendanceTrackingMessages.Get("ChatJustificationPeriodSummary"),
                Pages = new() { "/poweradmintimesheet", "/vacationplan" },
                CrossValidator = slots =>
                    slots.TryGetValue("from", out var dal) && slots.TryGetValue("to", out var al) && ParseData(al) < ParseData(dal)
                        ? SlotValidationResult.Failed(SlotValidationError.BusinessRuleViolation, AttendanceTrackingMessages.Get("ChatPeriodInvalid"), "to")
                        : null,
                Slots = BuildSlots(
                    EmployeeNameSlot,
                    new SlotDefinition
                    {
                        Name = "justification",
                        Type = "string",
                        Required = true,
                        PromptDescription = "nome o codice del tipo di giustificativo (es. Ferie, Malattia, ROL)",
                        Question = AttendanceTrackingMessages.Get("ChatRegisterJustificationTypeQuestion"),
                        Label = AttendanceTrackingMessages.Get("ChatRegisterJustificationDisplayName")
                    },
                    SlotData("from", true, string.Empty, AttendanceTrackingMessages.Get("ChatPeriodFromQuestion"), AttendanceTrackingMessages.Get("ChatFromLabel"), SignificatoDal),
                    SlotData("to", true, string.Empty, AttendanceTrackingMessages.Get("ChatPeriodToQuestion"), AttendanceTrackingMessages.Get("ChatToLabel"), SignificatoAl),
                    new SlotDefinition
                    {
                        Name = "hours",
                        Type = "HH:mm o numero testuale",
                        Required = false,
                        PromptDescription = "ore al giorno, solo se indicate (altrimenti giornata intera). " + HoursSlotHelper.PromptDescription,
                        Question = AttendanceTrackingMessages.Get("ChatRegisterJustificationHoursQuestion"),
                        Label = AttendanceTrackingMessages.Get("ChatHoursLabel"),
                        Normalizer = HoursSlotHelper.Normalize
                    })
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var dipendente = await RisolviDipendente(slots.GetValueOrDefault("employeeName"), _dip_AnagraficaService, _chatUserContext);
            if (dipendente == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmployeeNotFoundRetry", slots.GetValueOrDefault("employeeName", "-")));

            var nomeGiustificativo = slots.GetValueOrDefault("justification", string.Empty);
            var giustificativo = TrovaGiustificativo(await Giustificativi(_par_GiustificativiService), nomeGiustificativo);
            if (giustificativo == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatJustificationTypeNotFound", nomeGiustificativo));

            var dal = ParseData(slots.GetValueOrDefault("from"));
            var al = ParseData(slots.GetValueOrDefault("to"));
            TimeSpan? ore = slots.TryGetValue("hours", out var h) && HoursSlotHelper.TryParse(h, out var ts) ? ts : null;

            var errore = await ChatInvioRichiestaHelper.Giustificativo(_dip_GG_RichiestaService, dipendente.IdAspNetUsers, true,
                dal, al, giustificativo.Id, ore);
            if (errore != null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatRegisterJustificationError", errore));

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatJustificationPeriodDone",
                giustificativo.Descrizione ?? nomeGiustificativo, Nome(dipendente), Data(dal), Data(al)));
        }
    }
}
