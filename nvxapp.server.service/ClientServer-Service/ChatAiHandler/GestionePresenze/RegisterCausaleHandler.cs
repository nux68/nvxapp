using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_AnagraficaService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Dip_GG_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService;
using nvxapp.server.service.ClientServer_Service.GestionePresenze.Par_CausaliService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze.ChatAI.Commands.Handlers
{
    // Registra una causale per un dipendente in una data con un valore ore.
    // Cerca il tipo di causale per nome/codice tra i Par_Causali aziendali,
    // poi salva il record tramite Dip_GG_CausaliPut.
    public class RegisterCausaleHandler : BaseCommandDipeHandler
    {
        private readonly IntentDefinition _intentDefinition;
        private readonly IDip_GG_CausaliService _dip_GG_CausaliService;
        private readonly IPar_CausaliService _par_CausaliService;

        public RegisterCausaleHandler(
            IDip_AnagraficaService dip_AnagraficaService,
            IDip_GG_CausaliService dip_GG_CausaliService,
            IPar_CausaliService par_CausaliService
        ) : base(dip_AnagraficaService)
        {
            _dip_GG_CausaliService = dip_GG_CausaliService;
            _par_CausaliService    = par_CausaliService;

            var sb_Date = new StringBuilder();
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Data_Oggi(sb_Date);
            var datePromptDescription = sb_Date.ToString();

            _intentDefinition = new IntentDefinition
            {
                Name        = "RegisterCausale",
                DisplayName = "Inserisci causale",
                Description = "registra una causale per un dipendente in una data con un numero di ore.",
                Keywords    = new()
                {
                    "causale", "inserisci causale", "registra causale", "aggiungi causale"
                },
                Slots = BuildSlots(
                    EmployeeNameSlot,
                    new()
                    {
                        Name               = "causale",
                        Type               = "string",
                        Required           = true,
                        PromptDescription  = "nome o codice della causale (es. Straordinario, Reperibilità)",
                        Question           = "Quale causale vuoi inserire?",
                        Label              = "Causale",
                        HasRelevantContent = msg => msg.Length > 0
                    },
                    new()
                    {
                        Name               = "hours",
                        Type               = "HH:mm o numero testuale",
                        Required           = true,
                        PromptDescription  = "ore da registrare nel formato HH:mm (es. 08:00, 02:30) oppure come numero in lettere (es. due, otto)",
                        Question           = "Quante ore vuoi registrare? (es. 08:00 oppure due)",
                        Label              = "Ore",
                        Validator          = v =>
                        {
                            if (HoursSlotHelper.TryParse(v, out _)) return null;
                            return SlotValidationResult.Failed(
                                SlotValidationError.InvalidFormat,
                                $"'{v}' non è un formato ore valido. Usa HH:mm (es. 08:00) oppure un numero in lettere (es. otto).",
                                "hours");
                        },
                        HasRelevantContent = msg => HoursSlotHelper.TryParse(msg.Trim(), out _)
                    },
                    new()
                    {
                        Name               = "date",
                        Type               = "dd/MM/yyyy",
                        Required           = false,
                        Default            = "oggi",
                        PromptDescription  = datePromptDescription,
                        Question           = "Per quale data?",
                        Label              = "Data",
                        Validator          = v =>
                        {
                            if (DateOnly.TryParseExact(v, "yyyy-MM-dd", out _)) return null;
                            if (DateOnly.TryParseExact(v, "dd/MM/yyyy",  out _)) return null;
                            return SlotValidationResult.Failed(
                                SlotValidationError.InvalidFormat,
                                $"'{v}' non è una data valida. Usa il formato gg/mm/aaaa.",
                                "date");
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
            var causaleNome  = slots.GetValueOrDefault("causale", "");
            var hoursSlot    = slots.GetValueOrDefault("hours", "0");
            var dateSlot     = slots.GetValueOrDefault("date", "oggi");

            // --- Risolvi dipendente ---
            Dip_AnagraficaModel? dipendente = Get_Dip_Anagrafica(employeeName);
            if (dipendente == null)
                return CommandResult.Fail(
                    $"Dipendente '{employeeName}' non trovato o nome non completo. Nessuna causale registrata.");

            if (dipendente.Dip_RapportoLavoro == null || dipendente.Dip_RapportoLavoro.Count == 0)
                return CommandResult.Fail(
                    $"Nessun rapporto di lavoro trovato per '{employeeName}'.");

            // --- Risolvi tipo causale ---
            var parResult = await _par_CausaliService.GetAll(
                new GenericRequest<Par_CausaliInModel>(new Par_CausaliInModel()), false);

            if (!parResult.Success || parResult.Data?.Par_Causali == null)
                return CommandResult.Fail("Impossibile caricare l'elenco causali.");

            var nomeTrimmed = causaleNome.Trim();
            var parCausale = parResult.Data.Par_Causali.FirstOrDefault(c =>
                string.Equals(c.Descrizione, nomeTrimmed, StringComparison.OrdinalIgnoreCase) ||
                string.Equals(c.Codice,      nomeTrimmed, StringComparison.OrdinalIgnoreCase));

            // Ricerca parziale se non trovata esatta
            if (parCausale == null)
                parCausale = parResult.Data.Par_Causali.FirstOrDefault(c =>
                    (c.Descrizione?.Contains(nomeTrimmed, StringComparison.OrdinalIgnoreCase) ?? false) ||
                    (c.Codice?.Contains(nomeTrimmed,      StringComparison.OrdinalIgnoreCase) ?? false));

            if (parCausale == null)
                return CommandResult.Fail($"Causale '{causaleNome}' non trovata.");

            // --- Risolvi data ---
            DateTime competenzaDate = dateSlot.Equals("oggi", StringComparison.OrdinalIgnoreCase)
                ? DateTime.Today
                : DateOnly.TryParse(dateSlot, out var parsedDate)
                    ? parsedDate.ToDateTime(TimeOnly.MinValue)
                    : DateTime.Today;

            // --- Risolvi ore ---
            HoursSlotHelper.TryParse(hoursSlot, out var oreSpan);
            var valore   = new TimeOnly(oreSpan.Hours, oreSpan.Minutes);
            var oreLabel = $"{(int)oreSpan.TotalHours:D2}:{oreSpan.Minutes:D2}";

            // --- Salva causale ---
            var idRapportoLavoro = dipendente.Dip_RapportoLavoro[0].Id;

            var model = new Dip_GG_CausaliModel
            {
                Id                   = 0,
                IdDip_RapportoLavoro = idRapportoLavoro,
                Data                 = competenzaDate,
                IdPar_Causali        = parCausale.Id,
                Valore               = valore
            };

            var req = new GenericRequest<Dip_GG_CausaliPutInModel>
            {
                Data = new Dip_GG_CausaliPutInModel
                {
                    IdDip_RapportoLavoro = idRapportoLavoro,
                    ExcludeRicalc        = false,
                    Dip_GG_Causali       = model
                }
            };

            var result = await _dip_GG_CausaliService.Dip_GG_CausaliPut(req, false);

            if (!result.Success)
                return CommandResult.Fail(
                    $"Errore durante la registrazione della causale: {string.Join(", ", result.Messages.Select(m => m.Text))}");

            var dateLabel = competenzaDate.ToString("dd/MM/yyyy");
            return CommandResult.Ok(
                $"Causale '{parCausale.Descrizione}' di {oreLabel} ore registrata per {employeeName} il {dateLabel}.");
        }
    }
}
