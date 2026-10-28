using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Sposta una timbratura di un dipendente a un altro orario dello stesso giorno (default oggi).
    // Come la modifica dal Calendario HR: cambia l'orario e conserva l'originale. Solo HR, con conferma.
    public class UpdateClockingHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;

        public UpdateClockingHandler(IDip_AnagraficaService dip_AnagraficaService,
                                     IDip_GG_TimbraturaService dip_GG_TimbraturaService,
                                     IChatUserContext chatUserContext
                                     ) : base(dip_AnagraficaService, chatUserContext)
        {
            _dip_GG_TimbraturaService = dip_GG_TimbraturaService;

            _intentDefinition = new IntentDefinition
            {
                Name = "UpdateClocking",
                DisplayName = AttendanceTrackingMessages.Get("ChatUpdateClockingDisplayName"),
                Usable4Role = new List<string> { RuoloHR },
                Description = AttendanceTrackingMessages.Get("ChatUpdateClockingDescription"),
                Keywords = new() { "correggi timbratura", "sposta la timbratura", "modifica timbratura", "cambia l'orario della timbratura", "fix clocking", "move clocking" },
                Examples = new() { "sposta l'uscita di Rossi di ieri dalle 17 alle 18", "correggi la timbratura di Bianchi delle 8:10, era alle 8", "move Rossi's 9 am clocking to 9:30" },
                SummaryTemplate = AttendanceTrackingMessages.Get("ChatUpdateClockingSummary"),
                Pages = new() { "/poweradmintimesheet" },
                Slots = BuildSlots(
                    EmployeeNameSlot,
                    SlotOrario("time", true, AttendanceTrackingMessages.Get("ChatClockingWhichQuestion"), AttendanceTrackingMessages.Get("ChatClockingCurrentTimeLabel"),
                        "orario ATTUALE della timbratura da correggere (quello sbagliato)"),
                    SlotOrario("newTime", true, AttendanceTrackingMessages.Get("ChatClockingNewTimeQuestion"), AttendanceTrackingMessages.Get("ChatClockingNewTimeLabel"),
                        "NUOVO orario da dare alla timbratura (quello giusto)"),
                    SlotData("date", false, "oggi", AttendanceTrackingMessages.Get("ChatDateQuestion"), AttendanceTrackingMessages.Get("ChatDateLabel")))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var dipendente = await RisolviDipendente(slots.GetValueOrDefault("employeeName"), _dip_AnagraficaService, _chatUserContext);
            if (dipendente == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatEmployeeNotFoundRetry", slots.GetValueOrDefault("employeeName", "-")));

            var giorno = ParseData(slots.GetValueOrDefault("date", "oggi"));
            var orario = slots.GetValueOrDefault("time", string.Empty);
            var nuovoOrario = slots.GetValueOrDefault("newTime", string.Empty);
            if (!TimeOnly.TryParseExact(nuovoOrario, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var nuovo))
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatInvalidTime", nuovoOrario));

            var (timbratura, errore) = await ChatTimbratureHelper.Trova(_dip_GG_TimbraturaService, dipendente, giorno, orario);
            if (timbratura == null)
                return CommandResult.Fail(errore!);

            // nuovo orario sullo stesso giorno di competenza; TimbraturaOriginale resta quella registrata
            timbratura.Timbratura = timbratura.GiornoCompetenza.Date.Add(nuovo.ToTimeSpan());

            var res = await _dip_GG_TimbraturaService.Dip_GG_TimbraturaPut(new GenericRequest<Dip_GG_TimbraturaPutInModel>
            {
                Data = new()
                {
                    IdDip_RapportoLavoro = timbratura.IdDip_RapportoLavoro,
                    Dip_GG_Timbratura = timbratura,
                    ExcludeRicalc = false
                }
            }, false);
            if (!res.Success)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatUpdateClockingError", string.Join(", ", res.Messages.Select(m => m.Text))));

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatUpdateClockingDone", Nome(dipendente), Data(giorno), orario, nuovoOrario));
        }
    }
}
