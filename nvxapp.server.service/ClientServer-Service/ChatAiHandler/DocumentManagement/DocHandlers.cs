using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_DocumentoService;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_DocumentoService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Text;
using static nvxapp.server.service.ClientServer_Service.ChatAiHandler.DocumentManagement.ChatDocHelper;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.DocumentManagement
{
    // Comandi del Documentale della chat. ANALISI_DOCUMENTMANAGEMENT_VOCALI.md §2

    // R1 — ricerca per tipo, persona, testo, anno o mese
    public class DocSearchHandler : BaseCommandHandler
    {
        private readonly IDoc_DocumentoService _service;
        private readonly IntentDefinition _intentDefinition;

        public DocSearchHandler(IDoc_DocumentoService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "DocSearch",
                Application = ApplicationType.DocumentManagement,
                Category = DocumentManagementMessages.Get("ChatCategoryDocuments"),
                CategoryOrder = CategoryOrder,
                DisplayName = DocumentManagementMessages.Get("ChatSearchDisplayName"),
                Description = DocumentManagementMessages.Get("ChatSearchDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "trova", "cerca", "documenti", "documento", "cedolini", "contratto", "find", "search", "documents" },
                Examples = new() { "trova il contratto di Rossi", "cedolini del 2025 di Bianchi", "documenti con scritto Enel", "quali sono i miei cedolini?", "find Rossi's contract" },
                RequiresConfirmation = false,
                Pages = new() { "/doclist", "/docmine" },
                Slots = BuildSlots(SlotTipo(), SlotPersona(), SlotTesto(), SlotAnno(), SlotMese())
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (r, errore) = await Cerca(_service, slots);
            if (r == null) return CommandResult.Fail(errore!);
            if (r.Documenti.Count == 0)
                return CommandResult.Ok(DocumentManagementMessages.Get("ChatSearchNone", r.Filtro));

            var testo = Elenco(DocumentManagementMessages.Get("ChatSearchTitle", r.Documenti.Count, r.Filtro), r.Documenti.Select(Descrivi));
            if (r.Troncato) testo += "\n" + DocumentManagementMessages.Get("ChatSearchTruncated");
            return CommandResult.Ok(testo);
        }
    }

    // O1 — apre il documento più recente tra quelli trovati (pagina /docedit, history.state.id)
    public class DocOpenHandler : BaseCommandHandler
    {
        private readonly IDoc_DocumentoService _service;
        private readonly IntentDefinition _intentDefinition;

        public DocOpenHandler(IDoc_DocumentoService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "DocOpen",
                Application = ApplicationType.DocumentManagement,
                Category = DocumentManagementMessages.Get("ChatCategoryDocuments"),
                CategoryOrder = CategoryOrder,
                DisplayName = DocumentManagementMessages.Get("ChatOpenDisplayName"),
                Description = DocumentManagementMessages.Get("ChatOpenDescription"),
                Usable4Role = Ruoli,
                IsNavigation = true,
                Keywords = new() { "apri", "fammi vedere", "mostrami", "ultimo cedolino", "open", "show me" },
                Examples = new() { "apri l'ultimo cedolino", "apri il contratto di Rossi", "fammi vedere la CU del 2025", "open my last payslip" },
                Pages = new() { "/doclist", "/docmine", "/docedit" },
                Slots = BuildSlots(SlotTipo(), SlotPersona(), SlotTesto(), SlotAnno(), SlotMese())
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            if (!new[] { "documentType", "person", "query", "year", "month" }.Any(k => !string.IsNullOrWhiteSpace(slots.GetValueOrDefault(k))))
                return CommandResult.Fail(DocumentManagementMessages.Get("ChatOpenWhich"));

            var (r, errore) = await Cerca(_service, slots);
            if (r == null) return CommandResult.Fail(errore!);
            if (r.Documenti.Count == 0)
                return CommandResult.Fail(DocumentManagementMessages.Get("ChatSearchNone", r.Filtro));

            // l'elenco del servizio è già dal più recente
            var documento = r.Documenti[0];
            var payload = new NavigatePayload
            {
                Route = "/docedit",
                State = new Dictionary<string, object> { ["id"] = documento.Id }
            };
            var messaggio = r.Documenti.Count == 1
                ? DocumentManagementMessages.Get("ChatOpenOpening", Descrivi(documento))
                : DocumentManagementMessages.Get("ChatOpenOpeningLatest", r.Documenti.Count, Descrivi(documento));
            return CommandResult.Ok(messaggio, payload);
        }
    }

    // R2 — documenti scaduti e in scadenza entro N giorni (default 30)
    public class DocExpiringHandler : BaseCommandHandler
    {
        private const int GiorniPredefiniti = 30;

        private readonly IDoc_DocumentoService _service;
        private readonly IntentDefinition _intentDefinition;

        public DocExpiringHandler(IDoc_DocumentoService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "DocExpiring",
                Application = ApplicationType.DocumentManagement,
                Category = DocumentManagementMessages.Get("ChatCategoryDocuments"),
                CategoryOrder = CategoryOrder,
                DisplayName = DocumentManagementMessages.Get("ChatExpiringDisplayName"),
                Description = DocumentManagementMessages.Get("ChatExpiringDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "scadenza", "scadono", "scaduti", "in scadenza", "expiring", "expired", "expire" },
                Examples = new() { "documenti in scadenza", "cosa scade nei prossimi 60 giorni?", "ci sono documenti scaduti?", "which documents are expiring?" },
                RequiresConfirmation = false,
                Pages = new() { "/doclist" },
                Slots = BuildSlots(
                    new SlotDefinition
                    {
                        Name = "days",
                        Type = "int",
                        Required = false,
                        Default = GiorniPredefiniti.ToString(),
                        PromptDescription = "entro quanti giorni (es. 'nei prossimi 60 giorni' -> '60', 'entro un mese' -> 'un mese'); copia l'espressione",
                        Question = DocumentManagementMessages.Get("ChatDaysQuestion"),
                        Label = DocumentManagementMessages.Get("ChatDaysLabel"),
                        Normalizer = NormalizzaGiorni,
                        Validator = v => int.TryParse(v, out var g) && g > 0 && g <= 3650
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, DocumentManagementMessages.Get("ChatDaysQuestion"), "days")
                    },
                    SlotTipo(),
                    SlotPersona())
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        // "60", "60 giorni", "un mese", "due settimane" -> giorni
        public static string? NormalizzaGiorni(string valore)
        {
            var t = ChatAIKeywords.Normalize(valore);
            var numeri = new Dictionary<string, int>
            {
                ["un"] = 1, ["una"] = 1, ["uno"] = 1, ["due"] = 2, ["tre"] = 3, ["quattro"] = 4, ["sei"] = 6, ["dodici"] = 12,
                ["a"] = 1, ["one"] = 1, ["two"] = 2, ["three"] = 3, ["four"] = 4, ["six"] = 6, ["twelve"] = 12
            };
            var parole = t.Split(' ', StringSplitOptions.RemoveEmptyEntries);
            int quanti = 1;
            foreach (var p in parole)
            {
                if (int.TryParse(p, out var n)) quanti = n;
                else if (numeri.TryGetValue(p, out var m)) quanti = m;
            }
            int unita = parole.Any(p => p.StartsWith("mes") || p.StartsWith("month")) ? 30
                      : parole.Any(p => p.StartsWith("settiman") || p.StartsWith("week")) ? 7
                      : parole.Any(p => p.StartsWith("ann") || p.StartsWith("year")) ? 365
                      : 1;
            if (unita == 1 && !parole.Any(p => int.TryParse(p, out _))) return null;
            return (quanti * unita).ToString();
        }

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var giorni = int.TryParse(slots.GetValueOrDefault("days"), out var g) && g > 0 ? g : GiorniPredefiniti;
            var (r, errore) = await Cerca(_service, slots, scadenzaEntroGiorni: giorni);
            if (r == null) return CommandResult.Fail(errore!);

            var oggi = DateTime.Today;
            var conScadenza = r.Documenti.Where(d => d.DataScadenza.HasValue).OrderBy(d => d.DataScadenza).ToList();
            var scaduti = conScadenza.Where(d => d.DataScadenza!.Value.Date < oggi).ToList();
            var inScadenza = conScadenza.Where(d => d.DataScadenza!.Value.Date >= oggi).ToList();

            if (conScadenza.Count == 0)
                return CommandResult.Ok(DocumentManagementMessages.Get("ChatExpiringNone", giorni, r.Filtro));

            var sb = new StringBuilder();
            if (scaduti.Count > 0)
            {
                sb.AppendLine(Elenco(DocumentManagementMessages.Get("ChatExpiredTitle", scaduti.Count),
                    scaduti.Select(d => $"{Descrivi(d)} — {DocumentManagementMessages.Get("ChatExpiredOn", Data(d.DataScadenza!.Value))}")));
                sb.AppendLine();
            }
            if (inScadenza.Count > 0)
                sb.AppendLine(Elenco(DocumentManagementMessages.Get("ChatExpiringTitle", inScadenza.Count, giorni),
                    inScadenza.Select(d => $"{Descrivi(d)} — {DocumentManagementMessages.Get("ChatExpiresOn", Data(d.DataScadenza!.Value))}")));
            return CommandResult.Ok(sb.ToString().TrimEnd());
        }
    }

    // R3 — i miei documenti nuovi (mai aperti) e quelli con presa visione mancante
    public class DocToReadHandler : BaseCommandHandler
    {
        private readonly IDoc_DocumentoService _service;
        private readonly IntentDefinition _intentDefinition;

        public DocToReadHandler(IDoc_DocumentoService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "DocToRead",
                Application = ApplicationType.DocumentManagement,
                Category = DocumentManagementMessages.Get("ChatCategoryDocuments"),
                CategoryOrder = CategoryOrder,
                DisplayName = DocumentManagementMessages.Get("ChatToReadDisplayName"),
                Description = DocumentManagementMessages.Get("ChatToReadDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "da leggere", "nuovi documenti", "presa visione", "non letti", "to read", "new documents", "unread" },
                Examples = new() { "ho documenti da leggere?", "ci sono documenti nuovi per me?", "cosa devo confermare per presa visione?", "do I have new documents?" },
                RequiresConfirmation = false,
                Pages = new() { "/docmine" }
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (r, errore) = await Cerca(_service, slots, perMe: true);
            if (r == null) return CommandResult.Fail(errore!);

            var nuovi = r.Documenti.Where(d => d.Nuovo).ToList();
            var daConfermare = r.Documenti.Where(d => !d.Nuovo && d.PresaVisioneRichiesta && d.DataPresaVisione == null).ToList();
            if (nuovi.Count == 0 && daConfermare.Count == 0)
                return CommandResult.Ok(DocumentManagementMessages.Get("ChatToReadNone"));

            var sb = new StringBuilder();
            if (nuovi.Count > 0)
            {
                sb.AppendLine(Elenco(DocumentManagementMessages.Get("ChatToReadNewTitle", nuovi.Count), nuovi.Select(Descrivi)));
                sb.AppendLine();
            }
            if (daConfermare.Count > 0)
                sb.AppendLine(Elenco(DocumentManagementMessages.Get("ChatToReadConfirmTitle", daConfermare.Count), daConfermare.Select(Descrivi)));
            return CommandResult.Ok(sb.ToString().TrimEnd());
        }
    }

    // W1 — presa visione di un mio documento già aperto (attesta la lettura: un documento mai aperto va aperto prima)
    public class DocConfirmReadHandler : BaseCommandHandler
    {
        private readonly IDoc_DocumentoService _service;
        private readonly IntentDefinition _intentDefinition;

        public DocConfirmReadHandler(IDoc_DocumentoService service)
        {
            _service = service;
            _intentDefinition = new IntentDefinition
            {
                Name = "DocConfirmRead",
                Application = ApplicationType.DocumentManagement,
                Category = DocumentManagementMessages.Get("ChatCategoryDocuments"),
                CategoryOrder = CategoryOrder,
                DisplayName = DocumentManagementMessages.Get("ChatConfirmReadDisplayName"),
                Description = DocumentManagementMessages.Get("ChatConfirmReadDescription"),
                Usable4Role = Ruoli,
                Keywords = new() { "presa visione", "confermo la lettura", "ho letto", "conferma lettura", "acknowledge", "confirm reading" },
                Examples = new() { "conferma la presa visione del cedolino di ottobre", "ho letto la comunicazione, confermo", "confirm I read the October payslip" },
                SummaryTemplate = DocumentManagementMessages.Get("ChatConfirmReadSummary"),
                Pages = new() { "/docmine", "/docedit" },
                Slots = BuildSlots(SlotTipo(), SlotTesto(), SlotAnno(), SlotMese())
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var (r, errore) = await Cerca(_service, slots, perMe: true);
            if (r == null) return CommandResult.Fail(errore!);

            var candidati = r.Documenti.Where(d => d.PresaVisioneRichiesta && d.DataPresaVisione == null).ToList();
            if (candidati.Count == 0)
                return CommandResult.Fail(DocumentManagementMessages.Get("ChatConfirmReadNone", r.Filtro));
            if (candidati.Count > 1)
                return CommandResult.Fail(DocumentManagementMessages.Get("ChatConfirmReadAmbiguous", candidati.Count,
                    string.Join("; ", candidati.Take(6).Select(Descrivi))));

            var documento = candidati[0];
            if (documento.Nuovo)
                return CommandResult.Fail(DocumentManagementMessages.Get("ChatConfirmReadOpenFirst", Descrivi(documento)));

            var res = await _service.Doc_DocumentoPresaVisione(new GenericRequest<Doc_DocumentoIdInModel>(new Doc_DocumentoIdInModel { Id = documento.Id }), false);
            if (!res.Success)
                return CommandResult.Fail(DocumentManagementMessages.Get("ChatServiceError", Errore(res.Messages)));
            return CommandResult.Ok(DocumentManagementMessages.Get("ChatConfirmReadDone", Descrivi(documento)));
        }
    }
}
