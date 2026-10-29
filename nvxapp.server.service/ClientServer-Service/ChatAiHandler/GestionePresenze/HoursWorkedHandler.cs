using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_ResultService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Ore lavorate da un dipendente in un mese (default il corrente) o in un giorno, dai risultati
    // giornalieri calcolati (Dip_GG_Result): lavorate, previste, straordinari, assenze.
    // L'HR consulta tutti, gli altri ruoli solo i propri dati (dipendente non indicato = sé stessi).
    public class HoursWorkedHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_ResultService _dip_GG_ResultService;

        public HoursWorkedHandler(IDip_AnagraficaService dip_AnagraficaService,
                                  IDip_GG_ResultService dip_GG_ResultService,
                                  IChatUserContext chatUserContext
                                  ) : base(dip_AnagraficaService, chatUserContext)
        {
            _dip_GG_ResultService = dip_GG_ResultService;

            _intentDefinition = new IntentDefinition
            {
                Name = "HoursWorked",
                Category = AttendanceTrackingMessages.Get("ChatCategoryAttendance"),
                CategoryOrder = 500,
                DisplayName = AttendanceTrackingMessages.Get("ChatHoursWorkedDisplayName"),
                Usable4Role = new List<string> { RuoloHR, RuoloAdmin, RuoloUser },
                Description = AttendanceTrackingMessages.Get("ChatHoursWorkedDescription"),
                Keywords = new() { "ore lavorate", "quante ore", "ore fatte", "straordinari", "hours worked", "how many hours" },
                Examples = new() { "quante ore ha fatto Rossi a ottobre?", "quante ore ho lavorato questo mese?", "ore di Bianchi di ieri", "how many hours did Rossi work last month?" },
                RequiresConfirmation = false,
                Pages = new() { "/usertimesheet", "/poweradmintimesheet" },
                Slots = BuildSlots(
                    Facoltativo(EmployeeNameSlot, ChatSelf.Token),
                    SlotMese("month", AttendanceTrackingMessages.Get("ChatMonthQuestion"), AttendanceTrackingMessages.Get("ChatMonthLabel")),
                    SlotData("date", false, string.Empty, AttendanceTrackingMessages.Get("ChatDateQuestion"), AttendanceTrackingMessages.Get("ChatDateLabel")))
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

            // un giorno preciso oppure un mese
            DateTime? giorno = slots.TryGetValue("date", out var d) ? ParseData(d) : null;
            var (anno, mese) = giorno != null ? (giorno.Value.Year, giorno.Value.Month) : ParseMese(slots.GetValueOrDefault("month"));
            var periodo = giorno != null ? Data(giorno.Value) : Mese(anno, mese);

            var res = await _dip_GG_ResultService.GetAll(new GenericRequest<Dip_GG_Result_GetAll_InModel>
            {
                Data = new() { IdAspNetUsers = dipendente.IdAspNetUsers, Year = anno, Month = mese }
            }, true);
            if (!res.Success || res.Data == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatDataReadError", string.Join(", ", res.Messages.Select(m => m.Text))));

            // solo i giorni già trascorsi (i giorni futuri hanno le ore previste ma niente di lavorato)
            var giorni = res.Data.Dip_GG_Result
                .Where(r => giorno != null ? r.Data.Date == giorno.Value : r.Data.Date <= DateTime.Today)
                .ToList();
            if (giorni.Count == 0)
                return CommandResult.Ok(AttendanceTrackingMessages.Get("ChatHoursWorkedNoData", Nome(dipendente), periodo));

            int lavorate = giorni.Sum(r => (int)r.HH_Lav.ToTimeSpan().TotalMinutes);
            int previste = giorni.Sum(r => (int)r.HH_Teo.ToTimeSpan().TotalMinutes);
            int straordinari = giorni.Sum(r => r.Min_Strao + r.Min_StraoNott + r.Min_StraoFest + r.Min_StraoFestNott + r.Min_Suppl + r.Min_SupplEcc);
            int assenze = giorni.Sum(r => r.Min_Assenze);

            var messaggio = AttendanceTrackingMessages.Get("ChatHoursWorkedResult", Nome(dipendente), periodo, Ore(lavorate), Ore(previste));
            if (straordinari > 0 || assenze > 0)
                messaggio += " " + AttendanceTrackingMessages.Get("ChatHoursWorkedExtra", Ore(straordinari), Ore(assenze));

            return CommandResult.Ok(messaggio);
        }
    }
}
