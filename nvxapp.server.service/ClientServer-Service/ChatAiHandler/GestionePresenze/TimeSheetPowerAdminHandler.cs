using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.GestionePresenze
{
    // Naviga alla pagina Calendario HR (TimeSheetPowerAdminPage).
    // Se non viene passato alcun parametro si comporta come una normale navigazione.
    // Se viene passato il nominativo del dipendente e/o anno/mese,
    // questi vengono risolti e trasmessi come history.state al client Angular.
    public class TimeSheetPowerAdminHandler : BaseCommandHandler
    {
        private readonly IDip_AnagraficaService _dip_AnagraficaService;
        private readonly IntentDefinition _intentDefinition;

        public TimeSheetPowerAdminHandler(IDip_AnagraficaService dip_AnagraficaService)
        {
            _dip_AnagraficaService = dip_AnagraficaService;

            _intentDefinition = new IntentDefinition
            {
                Name         = "OpenTimeSheetPowerAdmin",
                DisplayName  = "Calendario HR",
                Usable4Role = new List<string> { "CompanyPowerAdmin" },
                Description  = "apre il Calendario HR (timesheet power admin). Facoltativamente filtra per dipendente e/o periodo anno/mese.",
                IsNavigation = true,
                Keywords     = new()
                {
                    "calendario hr", "timesheet hr", "timesheet power admin",
                    "calendario admin", "presenze hr", "foglio presenze hr"
                },
                Slots =
                [
                    new()
                    {
                        Name               = "employeeName",
                        Type               = "string",
                        Required           = false,
                        PromptDescription  = "nome e/o cognome del dipendente (facoltativo)",
                        Question           = "Per quale dipendente? (lascia vuoto per non filtrare)",
                        Label              = "Dipendente",
                        Validator          = v => DipendenteLookupHelper.ValidateEmployeeSlot(v, _dip_AnagraficaService),
                        HasRelevantContent = msg => msg.Any(char.IsLetter)
                    },
                    new()
                    {
                        Name               = "year",
                        Type               = "int",
                        Required           = false,
                        PromptDescription  = "anno numerico (es. 2025)",
                        Question           = "Per quale anno?",
                        Label              = "Anno",
                        HasRelevantContent = msg => msg.Any(char.IsDigit)
                    },
                    new()
                    {
                        Name               = "month",
                        Type               = "int (1-12)",
                        Required           = false,
                        PromptDescription  = "mese numerico 1-12 oppure nome del mese in italiano",
                        Question           = "Per quale mese?",
                        Label              = "Mese",
                        HasRelevantContent = msg =>
                        {
                            if (msg.Any(char.IsDigit)) return true;
                            var mesi = new[]
                            {
                                "gennaio","febbraio","marzo","aprile","maggio","giugno",
                                "luglio","agosto","settembre","ottobre","novembre","dicembre"
                            };
                            return mesi.Any(m => msg.Contains(m, StringComparison.OrdinalIgnoreCase));
                        }
                    }
                ]
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var state = new Dictionary<string, object>();
            var descrizione = new List<string>();

            // --- Dipendente ---
            slots.TryGetValue("employeeName", out var employeeName);
            if (!string.IsNullOrWhiteSpace(employeeName))
            {
                var dip = DipendenteLookupHelper.GetDipendente(employeeName, _dip_AnagraficaService);
                if (dip == null)
                    return CommandResult.Fail($"Dipendente '{employeeName}' non trovato.");

                state["userId"] = dip.IdAspNetUsers;
                descrizione.Add($"dipendente {dip.Cognome} {dip.Nome}");
            }

            // --- Anno ---
            slots.TryGetValue("year", out var yearStr);
            if (!string.IsNullOrWhiteSpace(yearStr) && int.TryParse(yearStr, out var year))
            {
                state["year"] = year;
                descrizione.Add($"anno {year}");
            }

            // --- Mese ---
            slots.TryGetValue("month", out var monthStr);
            if (!string.IsNullOrWhiteSpace(monthStr))
            {
                var month = ParseMonth(monthStr);
                if (month > 0)
                {
                    state["month"] = month;
                    descrizione.Add($"mese {month}");
                }
            }

            var payload = new NavigatePayload
            {
                Route = "/poweradmintimesheet",
                State = state
            };

            var msg = descrizione.Count > 0
                ? $"Apertura Calendario HR ({string.Join(", ", descrizione)})."
                : "Apertura Calendario HR.";

            return await Task.FromResult(CommandResult.Ok(msg, payload));
        }

        private static int ParseMonth(string value)
        {
            if (int.TryParse(value, out var n) && n >= 1 && n <= 12)
                return n;

            var mesi = new[]
            {
                "gennaio","febbraio","marzo","aprile","maggio","giugno",
                "luglio","agosto","settembre","ottobre","novembre","dicembre"
            };

            for (int i = 0; i < mesi.Length; i++)
                if (mesi[i].Equals(value.Trim(), StringComparison.OrdinalIgnoreCase))
                    return i + 1;

            return 0;
        }
    }
}
