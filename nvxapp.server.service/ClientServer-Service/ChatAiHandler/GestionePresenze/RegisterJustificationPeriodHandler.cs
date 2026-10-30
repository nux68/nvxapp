using nvxapp.server.data.Localization;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.data.Entities.Tenant.GestionePresenze;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_RichiestaService;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Giustificativo di un dipendente su un periodo (es. ferie dal 3 al 7), a giornata intera o con le ore
    // indicate. Si registra come dal Calendario HR: un giustificativo diretto per ogni giorno del periodo
    // (giorni e ore con le regole delle richieste), che l'HR puo' poi modificare o cancellare. Solo HR, con conferma.
    // Il cartellino si ricalcola una volta sola per il periodo (un job per mese), non giorno per giorno.
    public class RegisterJustificationPeriodHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_RichiestaService _dip_GG_RichiestaService;
        private readonly IDip_GG_GiustificativiService _dip_GG_GiustificativiService;
        private readonly IPar_GiustificativiService _par_GiustificativiService;
        private readonly ITimeSheet_EngineService_OnlyCalculate _timeSheet_EngineService;

        public RegisterJustificationPeriodHandler(IDip_AnagraficaService dip_AnagraficaService,
                                                  IDip_GG_RichiestaService dip_GG_RichiestaService,
                                                  IDip_GG_GiustificativiService dip_GG_GiustificativiService,
                                                  IPar_GiustificativiService par_GiustificativiService,
                                                  ITimeSheet_EngineService_OnlyCalculate timeSheet_EngineService,
                                                  IChatUserContext chatUserContext
                                                  ) : base(dip_AnagraficaService, chatUserContext)
        {
            _dip_GG_RichiestaService = dip_GG_RichiestaService;
            _dip_GG_GiustificativiService = dip_GG_GiustificativiService;
            _par_GiustificativiService = par_GiustificativiService;
            _timeSheet_EngineService = timeSheet_EngineService;

            _intentDefinition = new IntentDefinition
            {
                Name = "RegisterJustificationPeriod",
                Category = AttendanceTrackingMessages.Get("ChatCategoryJustifications"),
                CategoryOrder = 200,
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
                    },
                    RegisterJustificationAmountHandler.AmountSlot(false))
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

            // giustificativo a importo: l'importo (per giorno) e' obbligatorio
            decimal? importo = null;
            if (giustificativo.Tipo == nvxapp.server.data.Entities.Tenant.TipoValore.Importo)
            {
                if (!ImportoSlotHelper.TryParse(slots.GetValueOrDefault("amount"), out var imp))
                    return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatJustificationAmountMissing", giustificativo.Descrizione ?? nomeGiustificativo));
                importo = imp;
            }

            if (al < dal) al = dal;

            // rapporto di lavoro in corso all'inizio del periodo
            var rapporto = dipendente.Dip_RapportoLavoro?
                .Where(r => (r.DataAss == null || r.DataAss.Value.Date <= dal) && (r.DataLic == null || r.DataLic.Value.Date >= dal))
                .FirstOrDefault() ?? dipendente.Dip_RapportoLavoro?.FirstOrDefault();
            if (rapporto == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmploymentNotFoundFor", Nome(dipendente)));

            // giorni e ore come per una richiesta: giorni senza orario saltati, giornata intera = ore teoriche
            bool aImporto = importo != null;
            bool giornataIntera = ore == null && !aImporto;
            var giorni = _dip_GG_RichiestaService.GiorniGiustificativo(rapporto.Id, dal, al, giustificativo.Id, ore, giornataIntera);
            if (giorni.Count == 0)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("RequestedPeriodNoWorkingDays"));

            // un giustificativo diretto per giorno, come dal Calendario HR: l'HR lo puo' modificare e cancellare
            // (una richiesta approvata invece non si puo' piu' toccare dal calendario)
            foreach (var (giorno, oreGiorno) in giorni.OrderBy(g => g.Key))
            {
                var res = await _dip_GG_GiustificativiService.Dip_GG_GiustificativiPut(new GenericRequest<Dip_GG_GiustificativiPutInModel>
                {
                    Data = new Dip_GG_GiustificativiPutInModel
                    {
                        IdDip_RapportoLavoro = rapporto.Id,
                        // ricalcolo unico per il periodo, sotto
                        ExcludeRicalc = true,
                        Dip_GG_Giustificativi = new Dip_GG_GiustificativiModel
                        {
                            Id = 0,
                            IdDip_RapportoLavoro = rapporto.Id,
                            Data = giorno,
                            IdPar_Giustificativi = giustificativo.Id,
                            InputType = giornataIntera ? JustificationInputType.AllDay : JustificationInputType.Manual,
                            Hours = aImporto ? null : oreGiorno,
                            Importo = importo,
                            RichiestaStato = StatoRichiesta.Diretta
                        }
                    }
                }, false);
                if (!res.Success)
                    return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatRegisterJustificationError",
                        $"{Data(giorno)}: {string.Join(", ", res.Messages.Select(m => m.Text))}"));
            }

            // un solo ricalcolo per mese, dal primo all'ultimo giorno inserito (giorni consecutivi del calendario):
            // un job invece di uno per giorno. Per mese perche' i client ricaricano il mese indicato dal job
            foreach (var mese in giorni.Keys.GroupBy(g => new { g.Year, g.Month }))
            {
                var res = await _timeSheet_EngineService.Calculate(new GenericRequest<TimeSheet_CalculateInModel>
                {
                    Data = new TimeSheet_CalculateInModel
                    {
                        TimeSheet_Calculate = new TimeSheet_CalculateModel
                        {
                            SelectedUserId = new List<string> { dipendente.IdAspNetUsers },
                            Dal = mese.Min().Date,
                            Al = mese.Max().Date,
                            Year = mese.Key.Year,
                            Month = mese.Key.Month
                        }
                    }
                }, false);
                if (!res.Success)
                    return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatCalcolaGiornoError", string.Join(", ", res.Messages.Select(m => m.Text))));
            }

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatJustificationPeriodDone",
                giustificativo.Descrizione ?? nomeGiustificativo, Nome(dipendente), Data(dal), Data(al)));
        }
    }
}
