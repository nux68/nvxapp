using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Compliance;
using nvxapp.server.service.ClientServer_Service.Compliance.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using static nvxapp.server.service.ClientServer_Service.ChatAiHandler.Compliance.ChatCmpHelper;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Compliance
{
    // Scritture di Compliance dalla chat: riepilogo e conferma (anche a voce) prima di eseguire. Solo inserimenti,
    // nessuna modifica né cancellazione. ANALISI_COMPLIANCE_VOCALI.md §3.2

    // W1 — corso di sicurezza svolto da un dipendente (la scadenza la calcola il servizio dalle mansioni)
    public class CmpRegisterTrainingHandler : BaseCommandHandler
    {
        private readonly ICmp_SicurezzaService _service;
        private readonly IntentDefinition _intentDefinition;

        public CmpRegisterTrainingHandler(ICmp_SicurezzaService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "CmpRegisterTraining",
                Application = ApplicationType.Compliance,
                Category = ComplianceMessages.Get("ChatCategorySafety"),
                CategoryOrder = CategoryOrderSicurezza,
                DisplayName = ComplianceMessages.Get("ChatRegisterTrainingDisplayName"),
                Description = ComplianceMessages.Get("ChatRegisterTrainingDescription"),
                Usable4Role = RuoliAmministratori,
                Keywords = new() { "registra il corso", "registra corso", "ha fatto il corso", "ha frequentato", "corso antincendio", "corso primo soccorso", "corso sicurezza", "formazione sicurezza", "attended", "completed the course", "record training" },
                Examples = new() { "registra il corso antincendio di Rossi fatto il 10 novembre", "Bianchi ha fatto il corso di primo soccorso ieri", "segna la formazione generale di Verdi di oggi", "record first aid training for Rossi yesterday" },
                SummaryTemplate = ComplianceMessages.Get("ChatRegisterTrainingSummary"),
                Pages = new() { "/cmp81" },
                Slots = BuildSlots(
                    SlotDipendente(true),
                    new SlotDefinition
                    {
                        Name = "courseType",
                        Type = "string",
                        Required = true,
                        PromptDescription = "tipo di corso di sicurezza come detto dall'utente: formazione generale, formazione specifica, aggiornamento, antincendio, primo soccorso, preposti, dirigenti, RLS, RSPP/ASPP, attrezzature (carrelli, muletto, PLE), altro",
                        Question = ComplianceMessages.Get("ChatCourseTypeQuestion", TipiCorso()),
                        Label = ComplianceMessages.Get("ChatCourseTypeLabel"),
                        Validator = v => TrovaTipoCorso(v) != null
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, ComplianceMessages.Get("ChatCourseTypeNotFound", v, TipiCorso()), "courseType")
                    },
                    SlotData("date", false, "oggi", ComplianceMessages.Get("ChatCourseDateQuestion"), ComplianceMessages.Get("ChatCourseDateLabel"),
                        "giorno in cui si è svolto il corso"))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var tipo = TrovaTipoCorso(slots.GetValueOrDefault("courseType"));
            if (tipo == null)
                return CommandResult.Fail(ComplianceMessages.Get("ChatCourseTypeNotFound", slots.GetValueOrDefault("courseType"), TipiCorso()));

            var (lookup, errore) = await Lookup(_service);
            if (lookup == null) return CommandResult.Fail(errore!);
            var (dipendente, erroreDip) = RisolviDipendente(lookup, slots.GetValueOrDefault("person"));
            if (dipendente == null) return CommandResult.Fail(erroreDip!);

            var giorno = ParseData(slots.GetValueOrDefault("date"), DateTime.Today);
            if (giorno > DateTime.Today)
                return CommandResult.Fail(ComplianceMessages.Get("ChatCourseDateFuture"));

            var res = await _service.Cmp_FormazionePut(new GenericRequest<Cmp_PutInModel<Cmp_FormazioneModel>>(new Cmp_PutInModel<Cmp_FormazioneModel>
            {
                Item = new Cmp_FormazioneModel
                {
                    Id = 0,
                    IdDip_Anagrafica = dipendente.Id,
                    TipoCorso = tipo.Value,
                    Descrizione = TipoCorso(tipo.Value),
                    DataCorso = giorno
                }
            }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var scadenza = res.Data.Item.DataScadenza;
            return CommandResult.Ok(scadenza != null
                ? ComplianceMessages.Get("ChatRegisterTrainingDone", TipoCorso(tipo.Value), dipendente.Nome, Data(giorno), Data(scadenza.Value))
                : ComplianceMessages.Get("ChatRegisterTrainingDoneNoExpiry", TipoCorso(tipo.Value), dipendente.Nome, Data(giorno)));
        }
    }

    // W2 — consegna di un DPI a un dipendente: il servizio chiede al dipendente la presa visione (verbale di consegna)
    public class CmpPpeDeliveryHandler : BaseCommandHandler
    {
        private readonly ICmp_SicurezzaService _service;
        private readonly IntentDefinition _intentDefinition;

        public CmpPpeDeliveryHandler(ICmp_SicurezzaService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "CmpPpeDelivery",
                Application = ApplicationType.Compliance,
                Category = ComplianceMessages.Get("ChatCategorySafety"),
                CategoryOrder = CategoryOrderSicurezza,
                DisplayName = ComplianceMessages.Get("ChatPpeDeliveryDisplayName"),
                Description = ComplianceMessages.Get("ChatPpeDeliveryDescription"),
                Usable4Role = RuoliAmministratori,
                Keywords = new() { "dpi", "consegna dpi", "consegna dei dpi", "guanti", "scarpe antinfortunistiche", "elmetto", "casco", "occhiali protettivi", "mascherine", "ppe", "delivered", "gloves" },
                Examples = new() { "consegna due paia di guanti a Rossi", "ho dato le scarpe antinfortunistiche taglia 42 a Bianchi ieri", "registra la consegna dell'elmetto a Verdi", "deliver safety gloves to Rossi" },
                SummaryTemplate = ComplianceMessages.Get("ChatPpeDeliverySummary"),
                Pages = new() { "/cmp81" },
                Slots = BuildSlots(
                    SlotDipendente(true),
                    new SlotDefinition
                    {
                        Name = "ppe",
                        Type = "string",
                        Required = true,
                        PromptDescription = "dispositivo di protezione consegnato come detto dall'utente, senza quantità e taglia (es. 'guanti', 'scarpe antinfortunistiche', 'elmetto')",
                        Question = ComplianceMessages.Get("ChatPpeQuestion"),
                        Label = ComplianceMessages.Get("ChatPpeLabel")
                    },
                    new SlotDefinition
                    {
                        Name = "quantity",
                        Type = "int",
                        Required = false,
                        Default = "1",
                        PromptDescription = "quantità consegnata come numero (es. 'due paia' -> '2'); se non detta '1'",
                        Question = ComplianceMessages.Get("ChatQuantityQuestion"),
                        Label = ComplianceMessages.Get("ChatQuantityLabel"),
                        Validator = v => int.TryParse(v, out var q) && q > 0 && q <= 9999
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, ComplianceMessages.Get("ChatQuantityQuestion"), "quantity")
                    },
                    new SlotDefinition
                    {
                        Name = "size",
                        Type = "string",
                        Required = false,
                        PromptDescription = "taglia se l'utente la dice (es. '42', 'L', 'XL'), altrimenti ometti",
                        Question = ComplianceMessages.Get("ChatSizeQuestion"),
                        Label = ComplianceMessages.Get("ChatSizeLabel")
                    },
                    SlotData("date", false, "oggi", ComplianceMessages.Get("ChatDeliveryDateQuestion"), ComplianceMessages.Get("ChatDeliveryDateLabel"),
                        "giorno della consegna"))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (lookup, errore) = await Lookup(_service);
            if (lookup == null) return CommandResult.Fail(errore!);
            var (dipendente, erroreDip) = RisolviDipendente(lookup, slots.GetValueOrDefault("person"));
            if (dipendente == null) return CommandResult.Fail(erroreDip!);
            var (dpi, erroreDpi) = RisolviDpi(lookup, slots.GetValueOrDefault("ppe"));
            if (dpi == null) return CommandResult.Fail(erroreDpi!);

            var giorno = ParseData(slots.GetValueOrDefault("date"), DateTime.Today);
            if (giorno > DateTime.Today)
                return CommandResult.Fail(ComplianceMessages.Get("ChatDeliveryDateFuture"));
            var quantita = int.TryParse(slots.GetValueOrDefault("quantity"), out var q) && q > 0 ? q : 1;
            var taglia = slots.GetValueOrDefault("size")?.Trim();

            var res = await _service.Cmp_DpiConsegnaPut(new GenericRequest<Cmp_PutInModel<Cmp_DpiConsegnaModel>>(new Cmp_PutInModel<Cmp_DpiConsegnaModel>
            {
                Item = new Cmp_DpiConsegnaModel
                {
                    Id = 0,
                    IdDip_Anagrafica = dipendente.Id,
                    IdCmp_Dpi = dpi.Id,
                    DataConsegna = giorno,
                    Quantita = quantita,
                    Taglia = string.IsNullOrEmpty(taglia) ? null : taglia
                }
            }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var messaggio = ComplianceMessages.Get("ChatPpeDeliveryDone", quantita, dpi.Nome, dipendente.Nome, Data(giorno));
            if (res.Data.Item.PresaVisioneRichiesta)
                messaggio += " " + ComplianceMessages.Get("ChatPpeDeliveryAck");
            return CommandResult.Ok(messaggio);
        }
    }
}
