using nvxapp.server.data.Entities.Tenant.Crm;
using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Crm;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using static nvxapp.server.service.ClientServer_Service.ChatAiHandler.Crm.ChatCrmHelper;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Crm
{
    // Scritture del CRM dalla chat: riepilogo e conferma (anche a voce) prima di eseguire.
    // ANALISI_CRM_VOCALI.md §3.2

    // W1 — nuova attività per un cliente, assegnata all'utente
    public class CrmNewActivityHandler : BaseCommandHandler
    {
        private const string TipoPredefinito = "chiamata";
        private const string OraPredefinita = "09:00";

        private readonly ICrm_AttivitaService _attivitaService;
        private readonly ICrm_ClienteService _clienteService;
        private readonly ICrm_ConfigService _configService;
        private readonly IntentDefinition _intentDefinition;

        public CrmNewActivityHandler(ICrm_AttivitaService attivitaService, ICrm_ClienteService clienteService, ICrm_ConfigService configService)
        {
            _attivitaService = attivitaService;
            _clienteService = clienteService;
            _configService = configService;
            _intentDefinition = new IntentDefinition
            {
                Name = "CrmNewActivity",
                Application = ApplicationType.Crm,
                Category = CrmMessages.Get("ChatCategoryAgenda"),
                CategoryOrder = CategoryOrderAgenda,
                DisplayName = CrmMessages.Get("ChatNewActivityDisplayName"),
                Description = CrmMessages.Get("ChatNewActivityDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "fissa", "pianifica", "programma", "ricordami di chiamare", "appuntamento con", "chiamata con", "schedule", "remind me to call" },
                Examples = new() { "fissa una chiamata con Rossi domani alle 10", "appuntamento con Bianchi srl lunedì alle 15 per la demo", "ricordami di mandare una mail a Verdi venerdì", "schedule a meeting with Rossi tomorrow at 3 pm" },
                SummaryTemplate = CrmMessages.Get("ChatNewActivitySummary"),
                Pages = new() { "/crmactivitylist", "/crmactivityedit", "/crmcustomeredit" },
                Slots = BuildSlots(
                    SlotTipoAttivita(TipoPredefinito),
                    SlotCliente(true),
                    SlotData("date", false, "oggi", CrmMessages.Get("ChatDateQuestion"), CrmMessages.Get("ChatDateLabel")),
                    SlotOrario("time", false, OraPredefinita, CrmMessages.Get("ChatTimeQuestion"), CrmMessages.Get("ChatTimeLabel")),
                    SlotTesto("subject", false, "oggetto dell'attività se l'utente lo dice (es. 'per la demo' -> 'demo'), altrimenti ometti",
                        CrmMessages.Get("ChatSubjectQuestion"), CrmMessages.Get("ChatSubjectLabel")))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (cliente, errore) = await RisolviCliente(_clienteService, slots.GetValueOrDefault("customerName"));
            if (cliente == null) return CommandResult.Fail(errore!);

            var setup = await Setup(_configService);
            var nomeTipo = slots.GetValueOrDefault("activityType", TipoPredefinito);
            var tipo = setup == null ? null : TrovaTipo(setup.TipiAttivita, nomeTipo);
            if (tipo == null)
                return CommandResult.Fail(CrmMessages.Get("ChatActivityTypeNotFound", nomeTipo,
                    string.Join(", ", setup?.TipiAttivita.Where(t => t.Attivo).Select(t => t.Descrizione) ?? Enumerable.Empty<string>())));

            var giorno = ParseData(slots.GetValueOrDefault("date"), DateTime.Today);
            var ora = ParseOra(slots.GetValueOrDefault("time", OraPredefinita)) ?? new TimeOnly(9, 0);
            var inizio = giorno.Add(ora.ToTimeSpan());
            var oggetto = slots.GetValueOrDefault("subject");
            if (string.IsNullOrWhiteSpace(oggetto))
                oggetto = $"{tipo.Descrizione} {cliente.Descrizione}";

            var res = await _attivitaService.Crm_AttivitaPut(new GenericRequest<Crm_AttivitaPutInModel>(new Crm_AttivitaPutInModel
            {
                Attivita = new Crm_AttivitaModel
                {
                    Id = 0,
                    IdCrm_Par_TipoAttivita = tipo.Id,
                    Oggetto = oggetto.Trim(),
                    Inizio = inizio,
                    Fine = tipo.DurataMinuti > 0 ? inizio.AddMinutes(tipo.DurataMinuti) : null,
                    Stato = Crm_StatoAttivita.DaFare,
                    IdAz_Cliente = cliente.Id
                }
            }), false);
            if (!res.Success) return Fallito(res);

            return CommandResult.Ok(CrmMessages.Get("ChatNewActivityDone", tipo.Descrizione, cliente.Descrizione, Data(inizio), Ora(inizio)));
        }
    }

    // W2 — segna come fatta (o annullata) un'attività da fare di un cliente
    public class CrmCompleteActivityHandler : BaseCommandHandler
    {
        private readonly ICrm_AttivitaService _attivitaService;
        private readonly ICrm_ClienteService _clienteService;
        private readonly ICrm_ConfigService _configService;
        private readonly IntentDefinition _intentDefinition;

        public CrmCompleteActivityHandler(ICrm_AttivitaService attivitaService, ICrm_ClienteService clienteService, ICrm_ConfigService configService)
        {
            _attivitaService = attivitaService;
            _clienteService = clienteService;
            _configService = configService;
            _intentDefinition = new IntentDefinition
            {
                Name = "CrmCompleteActivity",
                Application = ApplicationType.Crm,
                Category = CrmMessages.Get("ChatCategoryAgenda"),
                CategoryOrder = CategoryOrderAgenda,
                DisplayName = CrmMessages.Get("ChatCompleteActivityDisplayName"),
                Description = CrmMessages.Get("ChatCompleteActivityDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "fatta", "completata", "chiudi l'attività", "ho chiamato", "esito", "done", "completed", "mark as done" },
                Examples = new() { "segna come fatta la chiamata con Rossi", "ho chiamato Bianchi, esito: richiamare a gennaio", "annulla l'appuntamento con Verdi di domani", "mark the call with Rossi as done" },
                SummaryTemplate = CrmMessages.Get("ChatCompleteActivitySummary"),
                Pages = new() { "/crmactivitylist", "/crmactivityedit" },
                Slots = BuildSlots(
                    SlotCliente(true),
                    new SlotDefinition
                    {
                        Name = "activityType",
                        Type = "string",
                        Required = false,
                        PromptDescription = "tipo di attività se detto: chiamata, appuntamento, email, attività",
                        Question = CrmMessages.Get("ChatActivityTypeQuestion"),
                        Label = CrmMessages.Get("ChatActivityTypeLabel")
                    },
                    SlotData("date", false, string.Empty, CrmMessages.Get("ChatDateQuestion"), CrmMessages.Get("ChatDateLabel"),
                        "giorno dell'attività se l'utente lo dice, altrimenti ometti"),
                    SlotTesto("outcome", false, "esito dell'attività se l'utente lo dice (es. 'richiamare a gennaio'), altrimenti ometti",
                        CrmMessages.Get("ChatOutcomeQuestion"), CrmMessages.Get("ChatOutcomeLabel")),
                    new SlotDefinition
                    {
                        Name = "cancelled",
                        Type = "bool",
                        Required = false,
                        Default = "false",
                        PromptDescription = "\"true\" se l'utente annulla l'attività invece di segnarla come fatta ('annulla l'appuntamento'), altrimenti \"false\"",
                        Question = CrmMessages.Get("ChatCancelledQuestion"),
                        Label = CrmMessages.Get("ChatCancelledLabel"),
                        HasRelevantContent = msg => !string.IsNullOrWhiteSpace(msg)
                    })
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (cliente, errore) = await RisolviCliente(_clienteService, slots.GetValueOrDefault("customerName"));
            if (cliente == null) return CommandResult.Fail(errore!);

            var oggi = DateTime.Today;
            var res = await _attivitaService.Crm_AttivitaList(new GenericRequest<Crm_AttivitaListInModel>(
                new Crm_AttivitaListInModel { Dal = oggi.AddDays(-90), Al = oggi.AddDays(90), SoloMie = true, SoloDaFare = true }), false);
            if (!res.Success || res.Data == null) return Fallito(res);

            var candidate = res.Data.Attivita.Where(a => a.Stato == Crm_StatoAttivita.DaFare && a.IdAz_Cliente == cliente.Id).ToList();

            if (slots.TryGetValue("activityType", out var nomeTipo) && !string.IsNullOrWhiteSpace(nomeTipo))
            {
                var setup = await Setup(_configService);
                var tipo = setup == null ? null : TrovaTipo(setup.TipiAttivita, nomeTipo);
                if (tipo != null) candidate = candidate.Where(a => a.IdCrm_Par_TipoAttivita == tipo.Id).ToList();
            }
            if (slots.TryGetValue("date", out var giorno) && !string.IsNullOrWhiteSpace(giorno))
            {
                var data = ParseData(giorno, oggi);
                candidate = candidate.Where(a => a.Inizio.Date == data).ToList();
            }

            if (candidate.Count == 0)
                return CommandResult.Fail(CrmMessages.Get("ChatCompleteActivityNone", cliente.Descrizione));
            if (candidate.Count > 1)
                return CommandResult.Fail(CrmMessages.Get("ChatCompleteActivityAmbiguous", candidate.Count, cliente.Descrizione,
                    string.Join("; ", candidate.OrderBy(a => a.Inizio).Take(6).Select(a => $"{a.Tipo} {Data(a.Inizio)} {Ora(a.Inizio)} — {a.Oggetto}"))));

            var attivita = candidate[0];
            var annullata = bool.TryParse(slots.GetValueOrDefault("cancelled"), out var a) && a;
            var esito = await _attivitaService.Crm_AttivitaCompleta(new GenericRequest<Crm_AttivitaCompletaInModel>(
                new Crm_AttivitaCompletaInModel { Id = attivita.Id, Esito = slots.GetValueOrDefault("outcome"), Annullata = annullata }), false);
            if (!esito.Success) return Fallito(esito);

            return CommandResult.Ok(CrmMessages.Get(annullata ? "ChatCompleteActivityCancelled" : "ChatCompleteActivityDone",
                attivita.Tipo, cliente.Descrizione, Data(attivita.Inizio)));
        }
    }

    // W3 — nota sulla scheda del cliente
    public class CrmAddNoteHandler : BaseCommandHandler
    {
        private readonly ICrm_NotaAllegatoService _notaService;
        private readonly ICrm_ClienteService _clienteService;
        private readonly IntentDefinition _intentDefinition;

        public CrmAddNoteHandler(ICrm_NotaAllegatoService notaService, ICrm_ClienteService clienteService)
        {
            _notaService = notaService;
            _clienteService = clienteService;
            _intentDefinition = new IntentDefinition
            {
                Name = "CrmAddNote",
                Application = ApplicationType.Crm,
                Category = CrmMessages.Get("ChatCategorySales"),
                CategoryOrder = CategoryOrderVendite,
                DisplayName = CrmMessages.Get("ChatAddNoteDisplayName"),
                Description = CrmMessages.Get("ChatAddNoteDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "nota", "annota", "appunta", "segnati", "note", "write down" },
                Examples = new() { "aggiungi una nota al cliente Rossi: vuole il listino nuovo", "annota su Bianchi srl che il referente è cambiato", "add a note to Rossi: call back in January" },
                SummaryTemplate = CrmMessages.Get("ChatAddNoteSummary"),
                Pages = new() { "/crmcustomeredit", "/crmcustomerlist" },
                Slots = BuildSlots(
                    SlotCliente(true),
                    SlotTesto("text", true, "testo della nota come detto dall'utente, senza il nome del cliente e senza 'aggiungi una nota'",
                        CrmMessages.Get("ChatNoteTextQuestion"), CrmMessages.Get("ChatNoteTextLabel")))
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (cliente, errore) = await RisolviCliente(_clienteService, slots.GetValueOrDefault("customerName"));
            if (cliente == null) return CommandResult.Fail(errore!);

            var testo = slots.GetValueOrDefault("text", string.Empty).Trim();
            if (testo.Length == 0) return CommandResult.Fail(CrmMessages.Get("ChatNoteTextQuestion"));

            var res = await _notaService.Crm_NotaPut(new GenericRequest<Crm_NotaPutInModel>(new Crm_NotaPutInModel
            {
                Nota = new Crm_NotaModel { Id = 0, IdAz_Cliente = cliente.Id, Testo = testo }
            }), false);
            if (!res.Success) return Fallito(res);

            return CommandResult.Ok(CrmMessages.Get("ChatAddNoteDone", cliente.Descrizione));
        }
    }
}
