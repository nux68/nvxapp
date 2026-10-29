using nvxapp.server.data.Entities.Tenant;
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
    // Timbrature di un dipendente in un giorno (default oggi), comprese quelle richieste e in attesa.
    // L'HR consulta tutti, gli altri ruoli solo i propri dati (dipendente non indicato = sé stessi).
    public class ClockingsOfDayHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;

        public ClockingsOfDayHandler(IDip_AnagraficaService dip_AnagraficaService,
                                     IDip_GG_TimbraturaService dip_GG_TimbraturaService,
                                     IChatUserContext chatUserContext
                                     ) : base(dip_AnagraficaService, chatUserContext)
        {
            _dip_GG_TimbraturaService = dip_GG_TimbraturaService;

            _intentDefinition = new IntentDefinition
            {
                Name = "ClockingsOfDay",
                Category = AttendanceTrackingMessages.Get("ChatCategoryClockings"),
                CategoryOrder = 100,
                DisplayName = AttendanceTrackingMessages.Get("ChatClockingsOfDayDisplayName"),
                Usable4Role = new List<string> { RuoloHR, RuoloAdmin, RuoloUser },
                Description = AttendanceTrackingMessages.Get("ChatClockingsOfDayDescription"),
                Keywords = new() { "che timbrature", "quali timbrature", "timbrature di", "le mie timbrature", "clockings of", "my clockings" },
                Examples = new() { "che timbrature ha Rossi ieri?", "a che ora sono entrato oggi?", "fammi vedere le timbrature di Bianchi di lunedì", "show my clockings for today" },
                RequiresConfirmation = false,
                Pages = new() { "/timeclockuser", "/usertimesheet", "/poweradmintimesheet" },
                Slots = BuildSlots(
                    Facoltativo(EmployeeNameSlot, ChatSelf.Token),
                    SlotData("date", false, "oggi", AttendanceTrackingMessages.Get("ChatDateQuestion"), AttendanceTrackingMessages.Get("ChatDateLabel")))
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

            var giorno = ParseData(slots.GetValueOrDefault("date", "oggi"));
            var res = await _dip_GG_TimbraturaService.Dip_GG_Timbratura_Get_4Calculation(
                new GenericRequest<Dip_GG_Timbratura_Get_4Calculation_InModel>
                {
                    Data = new() { UsersId = new List<string> { dipendente.IdAspNetUsers }, Dal = giorno, Al = giorno }
                }, true);
            if (!res.Success || res.Data == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatDataReadError", string.Join(", ", res.Messages.Select(m => m.Text))));

            var timbrature = res.Data.Dip_GG_Timbratura
                .Where(t => t.RichiestaStato != StatoRichiesta.Cancellata && t.RichiestaStato != StatoRichiesta.Rifiutata)
                .OrderBy(t => t.Timbratura)
                .ToList();
            if (timbrature.Count == 0)
                return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatClockingsNone", Nome(dipendente), Data(giorno)));

            // le richieste non ancora approvate si segnalano
            var orari = timbrature.Select(t => StatoValido(t.RichiestaStato)
                ? t.Timbratura.ToString("HH:mm")
                : $"{t.Timbratura:HH:mm} {AttendanceTrackingMessages.Get("ChatClockingPending")}");

            return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatClockingsTitle", Nome(dipendente), Data(giorno), string.Join(", ", orari)));
        }
    }
}
