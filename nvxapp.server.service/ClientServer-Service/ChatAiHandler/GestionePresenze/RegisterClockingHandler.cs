using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using System.Globalization;
using nvxapp.server.data.Localization;
using nvxapp.server.data.Entities.Tenant;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;
using static System.Runtime.InteropServices.JavaScript.JSType;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Gestisce la registrazione di una timbratura entrata/uscita.
    // TODO: sostituire il placeholder con la chiamata API reale
    //       e il lookup del dipendente per nome -> EmployeeId.
    public class RegisterClockingHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;

        public RegisterClockingHandler(IDip_AnagraficaService dip_AnagraficaService,
                                       IDip_GG_TimbraturaService dip_GG_TimbraturaService,
                                       IChatUserContext chatUserContext
                                       ) : base(dip_AnagraficaService, chatUserContext)
        {
            _dip_GG_TimbraturaService = dip_GG_TimbraturaService;

            var sb_Date = new StringBuilder();
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Data_Oggi(sb_Date);
            var DatePromptDescription = sb_Date.ToString();

            var sb_Time = new StringBuilder();
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Orario(sb_Time);
            var TimePromptDescription = sb_Time.ToString();



            _intentDefinition = new IntentDefinition
            {
                Name = "RegisterClocking",
                Usable4Role = new List<string> { "CompanyPowerAdmin" },
                DisplayName = AttendanceTrackingMessages.Get("ChatRegisterClockingDisplayName"),
                Description = AttendanceTrackingMessages.Get("ChatRegisterClockingDescription"),
                // frasi di esempio per il riconoscimento dell'intent (italiano e inglese)
                Examples = new() { "Rossi è entrato alle 8", "segna l'uscita di Bianchi alle 17:30", "timbrami adesso", "registra l'entrata di Verdi ieri alle 9", "clock in Rossi at 9" },
                SummaryTemplate = AttendanceTrackingMessages.Get("ChatRegisterClockingSummary"),
                Keywords = new() { "timbratura", "timbra","timbrare" ,
                                       "entrata","entra", "entrato", 
                                       "uscita", "esce", "uscito",
                                       "orario", "clocking",
                                       "clock in", "clock out", "punch in", "punch out", "check in", "check out"
                                    },
                Slots = BuildSlots(
                EmployeeNameSlot,
                new()
                {
                    Name = "time",
                    Type = "HH:mm",
                    Required = true,
                    PromptDescription =TimePromptDescription,
                    Question = AttendanceTrackingMessages.Get("ChatRegisterClockingTimeQuestion"),
                    Label = AttendanceTrackingMessages.Get("ChatTimeLabel"),
                    Validator = v =>
                    {
                        // 1. Controllo formato base HH:mm tramite TimeOnly.TryParse
                        if (TimeOnly.TryParse(v, out _))
                            return null;

                        // 2. Se non è valido, restituisco un errore dettagliato
                        return SlotValidationResult.Failed(
                            SlotValidationError.InvalidFormat,
                            AttendanceTrackingMessages.Get("ChatInvalidTime", v),
                            "time"
                        );
                    },
                    HasRelevantContent = msg =>
                    {
                        if (msg.Any(char.IsDigit)) return true;
                        var numericWords = new[]
                        {
                            "uno","due","tre","quattro","cinque","sei","sette","otto","nove","dieci",
                            "undici","dodici","tredici","quattordici","quindici","sedici","diciassette",
                            "diciotto","diciannove","venti","ventuno","ventidue","ventitre",
                            "one","two","three","four","five","six","seven","eight","nine","ten",
                            "eleven","twelve","noon","midnight"
                        };
                        var retVal = numericWords.Any(w => msg.Contains(w, StringComparison.OrdinalIgnoreCase));
                        return retVal;
                    }
                },
                new()
                {
                    Name = "date",
                    Type = "dd/MM/yyyy",
                    Required = false,
                    Default = "oggi",
                    
                    PromptDescription=DatePromptDescription,

                    Question = AttendanceTrackingMessages.Get("ChatDateQuestion"),
                    Label = AttendanceTrackingMessages.Get("ChatDateLabel"),
                    Validator = v =>
                    {
                        // Prova formato ISO
                        if (DateOnly.TryParseExact(v, "yyyy-MM-dd", out _))
                            return null;

                        // Prova formato italiano
                        if (DateOnly.TryParseExact(v, "dd/MM/yyyy", out var d))
                            return null;
                        // 2. In caso di errore, restituisce un messaggio dettagliat

                        return SlotValidationResult.Failed(
                            SlotValidationError.InvalidFormat,
                            AttendanceTrackingMessages.Get("ChatInvalidDate", v),
                            "date"
                        );
                    },
                    HasRelevantContent = ChatSlotWords.HasDateContent
                },
                new()
                {
                    Name = "direction",
                    Type = "IN/OUT",
                    Required = false,
                    Default = "IN",
                    PromptDescription = "valore IN oppure OUT. Se dice 'entrata/entra/inizia/inizio' restituisci IN, se dice 'uscita/esce/fine/finisce' restituisci OUT.",
                    Question = AttendanceTrackingMessages.Get("ChatRegisterClockingDirectionQuestion"),
                    Label = AttendanceTrackingMessages.Get("ChatRegisterClockingDirectionLabel"),
                    //Validator = v =>
                    //{
                    //        return null;
                    //},
                    HasRelevantContent = msg =>
                    {
                        var Words = new[]
                        {
                            "IN","OUT",
                            "entrata","entra","entrato","entr",
                            "uscita","esce","uscito","usc",
                            "entry","enter","exit","leave","clock in","clock out"
                        };
                        var retVal = Words.Any(w => msg.Contains(w, StringComparison.OrdinalIgnoreCase));
                        return retVal;
                    }
                }
            )
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var employeeName = slots.GetValueOrDefault("employeeName", "");
            var timeSlot     = slots.GetValueOrDefault("time", "00:00");
            var dateSlot     = slots.GetValueOrDefault("date", "oggi");
            var direction    = slots.GetValueOrDefault("direction", "IN");
            var directionLabel = direction.Equals("OUT", StringComparison.OrdinalIgnoreCase)
                ? AttendanceTrackingMessages.Get("ChatClockingDirectionOut")
                : AttendanceTrackingMessages.Get("ChatClockingDirectionIn");

            // --- Risolvi dipendente ---
            Dip_AnagraficaModel? Dip_Anagrafica = Get_Dip_Anagrafica(employeeName);
            if (Dip_Anagrafica == null)
                return CommandResult.Fail(
                    AttendanceTrackingMessages.Get("ChatRegisterClockingEmployeeNotFound", employeeName));

            if (Dip_Anagrafica.Dip_RapportoLavoro == null || Dip_Anagrafica.Dip_RapportoLavoro.Count == 0)
                return CommandResult.Fail(
                    AttendanceTrackingMessages.Get("ChatEmploymentNotFoundFor", employeeName));

            // --- Data e ora della timbratura ---
            // date: "oggi" (default) oppure dd/MM/yyyy / yyyy-MM-dd; time: sempre HH:mm.
            DateTime competenzaDate = ChatSlotWords.ParseDateOrToday(dateSlot);

            DateTime timbraturaDateTime = TimeOnly.TryParse(timeSlot, CultureInfo.InvariantCulture, out var parsedTime)
                ? competenzaDate.Add(parsedTime.ToTimeSpan())
                : competenzaDate;

            // Come nel resto dell'applicazione la timbratura si registra senza verso:
            // entrata/uscita le assegna il motore di calcolo accoppiando le timbrature del giorno.
            // Il verso detto dall'utente resta solo nel messaggio di conferma.
            Dip_GG_TimbraturaModel Dip_GG_Timbratura = new Dip_GG_TimbraturaModel()
            {
                IdDip_RapportoLavoro      = Dip_Anagrafica.Dip_RapportoLavoro[0].Id,
                Timbratura                = timbraturaDateTime,
                TimbraturaOriginale       = timbraturaDateTime,
                GiornoCompetenza          = competenzaDate.Date,
                TimbraturaTipo            = TipoTimbratura.SenzaVerso,
                RichiestaStato            = StatoRichiesta.Diretta,
                IdAz_SubCommessaAttivita  = Dip_Anagrafica.Dip_RapportoLavoro[0].IdAz_SubCommessaAttivita
            };

            var req_1 = new GenericRequest<Dip_GG_TimbraturaPutInModel>()
            {
                Data = new Dip_GG_TimbraturaPutInModel()
                {
                    IdDip_RapportoLavoro = Dip_Anagrafica.Dip_RapportoLavoro[0].Id,
                    Dip_GG_Timbratura    = Dip_GG_Timbratura,
                    ExcludeRicalc        = false
                }
            };

            var result = await _dip_GG_TimbraturaService.Dip_GG_TimbraturaPut(req_1, true);

            if (!result.Success)
                return CommandResult.Fail(
                    AttendanceTrackingMessages.Get("ChatRegisterClockingError", string.Join(", ", result.Messages.Select(m => m.Text))));

            return CommandResult.Ok(
                AttendanceTrackingMessages.Get("ChatRegisterClockingDone", directionLabel, employeeName,
                    timbraturaDateTime.ToString("HH:mm"), competenzaDate.ToString("dd/MM/yyyy")));
        }
    }
}
