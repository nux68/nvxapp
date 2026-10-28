using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Elimina una timbratura di un dipendente, indicata da giorno (default oggi) e orario.
    // Solo HR, con conferma. Con la memoria della chat copre anche "annulla l'ultima timbratura".
    public class DeleteClockingHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;

        public DeleteClockingHandler(IDip_AnagraficaService dip_AnagraficaService,
                                     IDip_GG_TimbraturaService dip_GG_TimbraturaService,
                                     IChatUserContext chatUserContext
                                     ) : base(dip_AnagraficaService, chatUserContext)
        {
            _dip_GG_TimbraturaService = dip_GG_TimbraturaService;

            _intentDefinition = new IntentDefinition
            {
                Name = "DeleteClocking",
                DisplayName = AttendanceTrackingMessages.Get("ChatDeleteClockingDisplayName"),
                Usable4Role = new List<string> { RuoloHR },
                Description = AttendanceTrackingMessages.Get("ChatDeleteClockingDescription"),
                Keywords = new() { "elimina timbratura", "cancella timbratura", "togli la timbratura", "annulla l'ultima timbratura", "delete clocking", "remove clocking" },
                Examples = new() { "cancella la timbratura di Rossi delle 8 di ieri", "elimina l'uscita di Bianchi delle 17:30", "annulla l'ultima timbratura", "delete Rossi's 9 am clocking" },
                SummaryTemplate = AttendanceTrackingMessages.Get("ChatDeleteClockingSummary"),
                Pages = new() { "/poweradmintimesheet" },
                Slots = BuildSlots(
                    EmployeeNameSlot,
                    SlotOrario("time", true, AttendanceTrackingMessages.Get("ChatClockingWhichQuestion"), AttendanceTrackingMessages.Get("ChatTimeLabel"),
                        "orario della timbratura da eliminare"),
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

            var (timbratura, errore) = await ChatTimbratureHelper.Trova(_dip_GG_TimbraturaService, dipendente, giorno, orario);
            if (timbratura == null)
                return CommandResult.Fail(errore!);

            var res = await _dip_GG_TimbraturaService.Dip_GG_TimbraturaDelete(new GenericRequest<Dip_GG_Timbratura_DeleteInModel>
            {
                Data = new() { Id = timbratura.Id, ExcludeRicalc = false }
            }, false);
            if (!res.Success)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatDeleteClockingError", string.Join(", ", res.Messages.Select(m => m.Text))));

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatDeleteClockingDone", Nome(dipendente), orario, Data(giorno)));
        }
    }
}
