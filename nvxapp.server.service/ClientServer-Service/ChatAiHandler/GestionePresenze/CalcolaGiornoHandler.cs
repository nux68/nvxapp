using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.TimeSheet_EngineService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Esegue il calcolo del foglio presenze per un dipendente in un intervallo di date.
    // I parametri Dal/Al sono opzionali (default: oggi).
    // Year e Month vengono derivati da Dal.
    // I flag booleani (Approva_Richieste_*, Genera_*) sono tutti false per default.
    // Chiama ITimeSheet_EngineService.Calculate al termine della raccolta slot.
    public class CalcolaGiornoHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly ITimeSheet_EngineService _timeSheet_EngineService;

        public CalcolaGiornoHandler(
            IDip_AnagraficaService dip_AnagraficaService,
            ITimeSheet_EngineService timeSheet_EngineService
        ) : base(dip_AnagraficaService)
        {
            _timeSheet_EngineService = timeSheet_EngineService;

            var sb_Date = new StringBuilder();
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Data_Oggi(sb_Date);
            var datePromptDescription = sb_Date.ToString();

            _intentDefinition = new IntentDefinition
            {
                Name        = "CalcolaGiorno",
                DisplayName = "Calcola presenze",
                Description = "esegue il calcolo del foglio presenze per un dipendente in un intervallo di date (default: oggi).",
                Keywords    = new()
                {
                    "calcola", "ricalcola", "calcola presenze", "ricalcola presenze",
                    "calcola giorno", "ricalcola giorno", "calcola timesheet"
                },
                Slots = BuildSlots(
                    EmployeeNameSlot,
                    new()
                    {
                        Name               = "mese",
                        Type               = "MM/yyyy o nome mese",
                        Required           = false,
                        Default            = "",
                        PromptDescription  = "mese da calcolare nel formato MM/yyyy (es. 07/2025) oppure nome del mese con anno opzionale (es. luglio, luglio 2025)",
                        Question           = "Vuoi calcolare un intero mese? (es. luglio 2025 oppure 07/2025 — lascia vuoto per usare le date)",
                        Label              = "Mese",
                        HasRelevantContent = msg => MonthSlotHelper.TryParse(msg.Trim(), out _)
                    },
                    new()
                    {
                        Name               = "dal",
                        Type               = "dd/MM/yyyy",
                        Required           = false,
                        Default            = "oggi",
                        PromptDescription  = datePromptDescription,
                        Question           = "Da quale data vuoi calcolare? (lascia vuoto per oggi)",
                        Label              = "Dal",
                        Validator          = v =>
                        {
                            if (string.IsNullOrWhiteSpace(v) || v.Equals("oggi", StringComparison.OrdinalIgnoreCase)) return null;
                            if (DateOnly.TryParseExact(v, "yyyy-MM-dd", out _)) return null;
                            if (DateOnly.TryParseExact(v, "dd/MM/yyyy",  out _)) return null;
                            return SlotValidationResult.Failed(
                                SlotValidationError.InvalidFormat,
                                $"'{v}' non è una data valida. Usa il formato gg/mm/aaaa.",
                                "dal");
                        },
                        HasRelevantContent = msg =>
                        {
                            var words = new[] { "oggi", "domani", "ieri" };
                            return words.Any(w => msg.Contains(w, StringComparison.OrdinalIgnoreCase));
                        }
                    },
                    new()
                    {
                        Name               = "al",
                        Type               = "dd/MM/yyyy",
                        Required           = false,
                        Default            = "oggi",
                        PromptDescription  = datePromptDescription,
                        Question           = "Fino a quale data? (lascia vuoto per oggi)",
                        Label              = "Al",
                        Validator          = v =>
                        {
                            if (string.IsNullOrWhiteSpace(v) || v.Equals("oggi", StringComparison.OrdinalIgnoreCase)) return null;
                            if (DateOnly.TryParseExact(v, "yyyy-MM-dd", out _)) return null;
                            if (DateOnly.TryParseExact(v, "dd/MM/yyyy",  out _)) return null;
                            return SlotValidationResult.Failed(
                                SlotValidationError.InvalidFormat,
                                $"'{v}' non è una data valida. Usa il formato gg/mm/aaaa.",
                                "al");
                        },
                        HasRelevantContent = msg =>
                        {
                            var words = new[] { "oggi", "domani", "ieri" };
                            return words.Any(w => msg.Contains(w, StringComparison.OrdinalIgnoreCase));
                        }
                    }
                )
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var employeeName = slots.GetValueOrDefault("employeeName", "");
            var meseSlot     = slots.GetValueOrDefault("mese", "");
            var dalSlot      = slots.GetValueOrDefault("dal", "oggi");
            var alSlot       = slots.GetValueOrDefault("al",  "oggi");

            // --- Risolvi dipendente ---
            Dip_AnagraficaModel? dipendente = Get_Dip_Anagrafica(employeeName);
            if (dipendente == null)
                return CommandResult.Fail(
                    $"Dipendente '{employeeName}' non trovato o nome non completo. Nessun calcolo eseguito.");

            if (string.IsNullOrWhiteSpace(dipendente.IdAspNetUsers))
                return CommandResult.Fail(
                    $"Impossibile determinare l'utente di sistema per '{employeeName}'.");

            // --- Risolvi date ---
            DateTime ParseDate(string slot) =>
                slot.Equals("oggi", StringComparison.OrdinalIgnoreCase)
                    ? DateTime.Today
                    : DateOnly.TryParseExact(slot, "yyyy-MM-dd", out var d1) ? d1.ToDateTime(TimeOnly.MinValue)
                    : DateOnly.TryParseExact(slot, "dd/MM/yyyy",  out var d2) ? d2.ToDateTime(TimeOnly.MinValue)
                    : DateTime.Today;

            DateTime dal, al;

            // Se è stato indicato un mese, dal/al coprono l'intero mese.
            if (!string.IsNullOrWhiteSpace(meseSlot) && MonthSlotHelper.TryParse(meseSlot, out var meseParsed))
            {
                dal = new DateTime(meseParsed.Year, meseParsed.Month, 1);
                al  = new DateTime(meseParsed.Year, meseParsed.Month,
                          DateTime.DaysInMonth(meseParsed.Year, meseParsed.Month));
            }
            else
            {
                dal = ParseDate(dalSlot);

                // Se "al" non è stato indicato esplicitamente (rimasto al default "oggi")
                // ma "dal" è un giorno specifico, il range collassa su quel singolo giorno.
                var alEffettivo = alSlot.Equals("oggi", StringComparison.OrdinalIgnoreCase) &&
                                  !dalSlot.Equals("oggi", StringComparison.OrdinalIgnoreCase)
                    ? dalSlot
                    : alSlot;

                al = ParseDate(alEffettivo);
                if (al < dal) al = dal;
            }

            // --- Costruisci richiesta di calcolo ---
            var req = new GenericRequest<TimeSheet_CalculateInModel>
            {
                Data = new TimeSheet_CalculateInModel
                {
                    TimeSheet_Calculate = new TimeSheet_CalculateModel
                    {
                        SelectedUserId                  = new List<string> { dipendente.IdAspNetUsers },
                        Dal                             = dal,
                        Al                              = al,
                        Year                            = dal.Year,
                        Month                           = dal.Month,
                        Approva_Richieste_Timbrature    = true,
                        Approva_Richieste_Giustificativo = true,
                        Genera_Timbrature_Mancanti      = true,
                        Genera_Giustificativo_Assenza   = true
                    }
                }
            };

            var result = await _timeSheet_EngineService.Calculate(req, false);

            if (!result.Success)
                return CommandResult.Fail(
                    $"Errore durante il calcolo: {string.Join(", ", result.Messages.Select(m => m.Text))}");

            var dalLabel = dal.ToString("dd/MM/yyyy");
            var alLabel  = al.ToString("dd/MM/yyyy");
            var periodo  = dalLabel == alLabel ? dalLabel : $"{dalLabel} - {alLabel}";

            return CommandResult.Ok(
                $"Calcolo presenze completato per {employeeName} ({periodo}).");
        }
    }
}
