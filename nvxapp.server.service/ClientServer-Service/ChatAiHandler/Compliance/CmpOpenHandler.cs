using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Compliance;
using nvxapp.server.service.ClientServer_Service.Compliance.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Commands;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using static nvxapp.server.service.ClientServer_Service.ChatAiHandler.Compliance.ChatCmpHelper;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Compliance
{
    // O1 — apre una sezione di Compliance ("apri le visite mediche") o un elemento ("apri il DVR", "apri la nomina dell'RSPP").
    // ANALISI_COMPLIANCE_VOCALI.md §3.3
    //
    // Non è un IOpenableEntity: le pagine di Compliance non si aprono con history.state = { id } (quello che passa
    // OpenEntity), ma l'elenco unico (/cmp81, /cmpgdpr) legge { sezione } e la modifica unica (/cmpedit) legge
    // { sezione, item } con l'elemento completo, come fa la pagina elenco. Il client non cambia.
    // Elementi apribili solo dalle tabelle senza dati personali dei dipendenti (documenti, nomine, mansioni, catalogo DPI,
    // trattamenti, responsabili esterni); corsi, visite, consegne e violazioni si aprono sull'elenco della sezione.
    public class CmpOpenHandler : BaseCommandHandler
    {
        private readonly ICmp_SicurezzaService _sicurezza;
        private readonly ICmp_GdprService _gdpr;
        private readonly IntentDefinition _intentDefinition;

        // sezione (chiave di cmp-sezioni.ts nel client) -> pagina elenco
        private static readonly Dictionary<string, string> _pagine = new()
        {
            ["formazione"] = "/cmp81", ["stato"] = "/cmp81", ["visite"] = "/cmp81", ["consegne"] = "/cmp81", ["nomine"] = "/cmp81",
            ["documenti"] = "/cmp81", ["mansioni"] = "/cmp81", ["dpi"] = "/cmp81",
            ["trattamenti"] = "/cmpgdpr", ["responsabili"] = "/cmpgdpr", ["violazioni"] = "/cmpgdpr",
        };

        // parole della sezione (si prova in ordine: le più specifiche prima)
        private static readonly (string Sezione, string[] Parole)[] _parole =
        {
            ("consegne", new[] { "consegne dpi", "consegne dei dpi", "consegne", "consegna", "ppe deliveries", "deliveries" }),
            ("stato", new[] { "situazione formazione", "situazione della formazione", "formazione obbligatoria", "corsi mancanti", "training status" }),
            ("formazione", new[] { "corsi sicurezza", "corsi di sicurezza", "corsi svolti", "formazione sicurezza", "corsi", "corso", "safety courses", "courses", "training" }),
            ("visite", new[] { "visite mediche", "visita medica", "visite", "sorveglianza sanitaria", "medical examinations", "medical" }),
            ("nomine", new[] { "nomine", "nomina", "rspp", "aspp", "rls", "preposti", "preposto", "addetti", "medico competente", "appointments", "appointment" }),
            ("documenti", new[] { "documenti sicurezza", "documenti della sicurezza", "documento", "documenti", "dvr", "duvri", "piano di emergenza", "procedure", "procedura", "risk assessment" }),
            ("mansioni", new[] { "mansioni", "mansione", "job roles", "job role" }),
            ("dpi", new[] { "catalogo dpi", "dpi", "dispositivi di protezione", "ppe catalogue", "ppe" }),
            ("trattamenti", new[] { "registro trattamenti", "registro dei trattamenti", "trattamenti", "trattamento", "processing activities", "processing" }),
            ("responsabili", new[] { "responsabili esterni", "responsabili del trattamento", "responsabili", "responsabile", "fornitori gdpr", "processors", "processor" }),
            ("violazioni", new[] { "data breach", "violazioni", "violazione", "breaches", "breach" }),
        };

        public CmpOpenHandler(ICmp_SicurezzaService sicurezza, ICmp_GdprService gdpr)
        {
            _sicurezza = sicurezza;
            _gdpr = gdpr;
            _intentDefinition = new IntentDefinition
            {
                Name = "CmpOpen",
                Application = ApplicationType.Compliance,
                Category = ComplianceMessages.Get("ChatCategorySafety"),
                CategoryOrder = CategoryOrderSicurezza,
                DisplayName = ComplianceMessages.Get("ChatOpenDisplayName"),
                Description = ComplianceMessages.Get("ChatOpenDescription"),
                Usable4Role = RuoliAmministratori,
                IsNavigation = true,
                Keywords = new() { "apri", "mostrami", "fammi vedere", "vai alle", "open", "show me" },
                Examples = new() { "apri le visite mediche", "apri il DVR", "apri la nomina dell'RSPP", "mostrami le consegne dei DPI", "apri il trattamento gestione del personale", "open the data breaches" },
                Pages = new() { "/cmp81", "/cmpgdpr" },
                Slots = BuildSlots(
                    new SlotDefinition
                    {
                        Name = "section",
                        Type = "string",
                        Required = true,
                        PromptDescription = "sezione di Compliance da aprire: corsi sicurezza, situazione formazione, visite mediche, consegne DPI, nomine (RSPP, RLS, preposti...), documenti (DVR, piani di emergenza, procedure), mansioni, catalogo DPI, trattamenti (registro GDPR), responsabili esterni, violazioni (data breach)",
                        Question = ComplianceMessages.Get("ChatOpenSectionQuestion"),
                        Label = ComplianceMessages.Get("ChatOpenSectionLabel"),
                        Normalizer = NormalizzaSezione,
                        Validator = v => _pagine.ContainsKey(v)
                            ? null
                            : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, ComplianceMessages.Get("ChatOpenSectionQuestion"), "section")
                    },
                    new SlotDefinition
                    {
                        Name = "name",
                        Type = "string",
                        Required = false,
                        PromptDescription = "nome dell'elemento da aprire se l'utente lo indica (titolo del documento, tipo di nomina o persona nominata, nome della mansione, del DPI, del trattamento o del responsabile; es. 'apri il DVR' -> 'DVR', 'la nomina dell'RSPP' -> 'RSPP'); ometti se l'utente vuole solo la sezione",
                        Question = ComplianceMessages.Get("ChatOpenNameQuestion"),
                        Label = ComplianceMessages.Get("ChatOpenNameLabel")
                    })
            };
        }

        public override IntentDefinition IntentDefinition => _intentDefinition;

        private static string? NormalizzaSezione(string valore)
        {
            var n = ChatAIKeywords.Normalize(valore);
            if (_pagine.ContainsKey(n)) return n;
            foreach (var (sezione, parole) in _parole)
                if (parole.Any(p => $" {n} ".Contains($" {ChatAIKeywords.Normalize(p)} ")))
                    return sezione;
            return null;
        }

        public override async Task<CommandResult> ExecuteAsync(Dictionary<string, string> slots)
        {
            var sezione = NormalizzaSezione(slots.GetValueOrDefault("section") ?? string.Empty);
            if (sezione == null) return CommandResult.Fail(ComplianceMessages.Get("ChatOpenSectionQuestion"));
            var nomeSezione = ComplianceMessages.Get("ChatSection_" + sezione);

            var nome = slots.GetValueOrDefault("name")?.Trim();
            if (!string.IsNullOrEmpty(nome) && ChatAIKeywords.Normalize(nome) != sezione)
            {
                var (item, testo, avviso) = await CercaElemento(sezione, nome);
                if (item != null)
                    return CommandResult.Ok(ComplianceMessages.Get("ChatOpenOpeningItem", nomeSezione, testo), new NavigatePayload
                    {
                        Route = "/cmpedit",
                        State = new Dictionary<string, object> { ["sezione"] = sezione, ["item"] = item }
                    });
                if (avviso != null)
                    return CommandResult.Ok(avviso + " " + ComplianceMessages.Get("ChatOpenOpeningSection", nomeSezione), Elenco(sezione));
            }
            return CommandResult.Ok(ComplianceMessages.Get("ChatOpenOpeningSection", nomeSezione), Elenco(sezione));
        }

        private static NavigatePayload Elenco(string sezione) => new()
        {
            Route = _pagine[sezione],
            State = new Dictionary<string, object> { ["sezione"] = sezione }
        };

        // elemento della sezione dal nome detto: (elemento, descrizione) oppure un avviso (non trovato, ambiguo) e si apre l'elenco
        private async Task<(object? Item, string? Testo, string? Avviso)> CercaElemento(string sezione, string nome)
        {
            var richiesta = new GenericRequest<Cmp_ListInModel>(new Cmp_ListInModel());
            switch (sezione)
            {
                case "documenti":
                {
                    var res = await _sicurezza.Cmp_DocumentoList(richiesta, false);
                    // più revisioni dello stesso documento: la più recente
                    return Scegli(res.Success ? res.Data?.Items : null, nome, x => $"{TipoDocumento(x.Tipo)} {x.Titolo}", x => (x.Tipo, x.Titolo.Trim().ToLowerInvariant()),
                                  x => x.DataRevisione, x => x.Titolo);
                }
                case "nomine":
                {
                    var res = await _sicurezza.Cmp_NominaList(richiesta, false);
                    return Scegli(res.Success ? res.Data?.Items : null, nome, x => $"{TipoNomina(x.Tipo)} {x.Persona}", x => (x.Tipo, x.Persona),
                                  x => x.DataInizio, x => $"{TipoNomina(x.Tipo)} — {x.Persona}");
                }
                case "mansioni":
                {
                    var res = await _sicurezza.Cmp_MansioneList(richiesta, false);
                    return Scegli(res.Success ? res.Data?.Items : null, nome, x => x.Descrizione, x => x.Id, x => DateTime.MinValue, x => x.Descrizione);
                }
                case "dpi":
                {
                    var res = await _sicurezza.Cmp_DpiList(richiesta, false);
                    return Scegli(res.Success ? res.Data?.Items : null, nome, x => x.Descrizione, x => x.Id, x => DateTime.MinValue, x => x.Descrizione);
                }
                case "trattamenti":
                {
                    var res = await _gdpr.Cmp_TrattamentoList(richiesta, false);
                    return Scegli(res.Success ? res.Data?.Items : null, nome, x => x.Denominazione, x => x.Id, x => DateTime.MinValue, x => x.Denominazione);
                }
                case "responsabili":
                {
                    var res = await _gdpr.Cmp_ResponsabileList(richiesta, false);
                    return Scegli(res.Success ? res.Data?.Items : null, nome, x => x.Denominazione, x => x.Id, x => DateTime.MinValue, x => x.Denominazione);
                }
                default:
                    return (null, null, null);
            }
        }

        // un solo gruppo trovato: il più recente del gruppo; nessuno o più gruppi: avviso
        private static (object? Item, string? Testo, string? Avviso) Scegli<T, K>(List<T>? elementi, string nome, Func<T, string?> testo,
            Func<T, K> gruppo, Func<T, DateTime> data, Func<T, string> descrizione)
        {
            if (elementi == null) return (null, null, null);
            var trovati = Trova(elementi, nome, testo);
            if (trovati.Count == 0)
                return (null, null, ComplianceMessages.Get("ChatOpenNotFound", nome));
            var gruppi = trovati.GroupBy(gruppo).ToList();
            if (gruppi.Count > 1)
                return (null, null, ComplianceMessages.Get("ChatOpenAmbiguous", gruppi.Count, nome,
                    string.Join(", ", gruppi.Take(6).Select(g => descrizione(g.First())))));
            var scelto = gruppi[0].OrderByDescending(data).First()!;
            return (scelto, descrizione(scelto), null);
        }
    }
}
