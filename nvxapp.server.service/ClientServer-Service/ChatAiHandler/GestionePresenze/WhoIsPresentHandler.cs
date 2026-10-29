using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_GiustificativiService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_TimbraturaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_GiustificativiService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers.ChatPresenzeHelper;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Chi è presente in un giorno (default oggi), dalle timbrature valide del giorno:
    // numero dispari = in servizio, pari = uscito, nessuna = assente (con il giustificativo, se c'è).
    // Le timbrature si salvano senza verso, quindi non si usa PresentStaffService (IsPresent non affidabile).
    public class WhoIsPresentHandler : BaseCommandHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_AnagraficaService _dip_AnagraficaService;
        private readonly IDip_GG_TimbraturaService _dip_GG_TimbraturaService;
        private readonly IDip_GG_GiustificativiService _dip_GG_GiustificativiService;
        private readonly IPar_GiustificativiService _par_GiustificativiService;

        public WhoIsPresentHandler(IDip_AnagraficaService dip_AnagraficaService,
                                   IDip_GG_TimbraturaService dip_GG_TimbraturaService,
                                   IDip_GG_GiustificativiService dip_GG_GiustificativiService,
                                   IPar_GiustificativiService par_GiustificativiService)
        {
            _dip_AnagraficaService = dip_AnagraficaService;
            _dip_GG_TimbraturaService = dip_GG_TimbraturaService;
            _dip_GG_GiustificativiService = dip_GG_GiustificativiService;
            _par_GiustificativiService = par_GiustificativiService;

            _intentDefinition = new IntentDefinition
            {
                Name = "WhoIsPresent",
                Category = AttendanceTrackingMessages.Get("ChatCategoryAttendance"),
                CategoryOrder = 500,
                DisplayName = AttendanceTrackingMessages.Get("ChatWhoIsPresentDisplayName"),
                Usable4Role = new List<string> { RuoloHR },
                Description = AttendanceTrackingMessages.Get("ChatWhoIsPresentDescription"),
                Keywords = new() { "presenti", "presente", "assenti", "chi c'è", "in servizio", "who is in", "present", "absent" },
                Examples = new() { "chi è presente oggi?", "chi manca oggi?", "chi c'era ieri?", "who is in today?" },
                RequiresConfirmation = false,
                Pages = new() { "/presentstaff" },
                Slots = BuildSlots(
                    SlotData("date", false, "oggi", AttendanceTrackingMessages.Get("ChatDateQuestion"), AttendanceTrackingMessages.Get("ChatDateLabel")))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var giorno = ParseData(slots.GetValueOrDefault("date", "oggi"));

            var dipendenti = await Dipendenti(_dip_AnagraficaService);
            if (dipendenti.Count == 0)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatNoEmployeesAvailable"));
            var perRapporto = PerRapporto(dipendenti);
            var utenti = dipendenti.Select(d => d.IdAspNetUsers).ToList();

            var timbrature = await _dip_GG_TimbraturaService.Dip_GG_Timbratura_Get_4Calculation(
                new GenericRequest<Dip_GG_Timbratura_Get_4Calculation_InModel>
                {
                    Data = new() { UsersId = utenti, Dal = giorno, Al = giorno }
                }, true);
            if (!timbrature.Success || timbrature.Data == null)
                return CommandResult.Fail(AttendanceTrackingMessages.Get("ChatDataReadError", string.Join(", ", timbrature.Messages.Select(m => m.Text))));

            var giustificativi = await _dip_GG_GiustificativiService.Dip_GG_Giustificativi_Get_4Calculation(
                new GenericRequest<Dip_GG_Giustificativi_Get_4Calculation_InModel>
                {
                    Data = new() { UsersId = utenti, Dal = giorno, Al = giorno }
                }, true);
            // i giustificativi a importo (rimborsi, trasferte) non dicono nulla sulla presenza: esclusi
            var nomiGiustificativi = (await Giustificativi(_par_GiustificativiService))
                .Where(g => g.Tipo != nvxapp.server.data.Entities.Tenant.TipoValore.Importo)
                .ToDictionary(g => g.Id, g => g.Descrizione ?? g.Codice ?? string.Empty);

            var timbratureValide = timbrature.Data.Dip_GG_Timbratura
                .Where(t => StatoValido(t.RichiestaStato))
                .GroupBy(t => t.IdDip_RapportoLavoro)
                .ToDictionary(g => g.Key, g => g.OrderBy(t => t.Timbratura).ToList());

            var giustificati = (giustificativi.Data?.Dip_GG_Giustificativi ?? new())
                .Where(g => StatoValido(g.RichiestaStato) && nomiGiustificativi.ContainsKey(g.IdPar_Giustificativi))
                .GroupBy(g => g.IdDip_RapportoLavoro)
                .ToDictionary(g => g.Key, g => string.Join(", ", g.Select(x => nomiGiustificativi.GetValueOrDefault(x.IdPar_Giustificativi, "?")).Distinct()));

            var inServizio = new List<string>();
            var usciti = new List<string>();
            var assenti = new List<string>();

            foreach (var dipendente in dipendenti.OrderBy(Nome))
            {
                var rapporti = dipendente.Dip_RapportoLavoro.Select(r => r.Id).ToList();
                if (rapporti.Count == 0) continue;

                var sue = rapporti.SelectMany(r => timbratureValide.GetValueOrDefault(r) ?? new()).OrderBy(t => t.Timbratura).ToList();
                var giustificativo = rapporti.Select(r => giustificati.GetValueOrDefault(r)).FirstOrDefault(g => g != null);

                if (sue.Count == 0)
                    assenti.Add(giustificativo != null ? $"{Nome(dipendente)} ({giustificativo})" : Nome(dipendente));
                else if (sue.Count % 2 == 1)
                    inServizio.Add(AttendanceTrackingMessages.Get("ChatPresentSince", Nome(dipendente), sue[0].Timbratura.ToString("HH:mm")));
                else
                    usciti.Add(Nome(dipendente));
            }

            var sb = new StringBuilder();
            sb.AppendLine(AttendanceTrackingMessages.Get("ChatPresentTitle", Data(giorno)));
            sb.AppendLine(AttendanceTrackingMessages.Get("ChatPresentLine", inServizio.Count, Elenco(inServizio)));
            sb.AppendLine(AttendanceTrackingMessages.Get("ChatLeftLine", usciti.Count, Elenco(usciti)));
            sb.AppendLine(AttendanceTrackingMessages.Get("ChatAbsentLine", assenti.Count, Elenco(assenti)));

            return CommandResult.Ok(sb.ToString().TrimEnd());
        }
    }
}
