using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_DocumentoService;
using nvxapp.server.service.ClientServer_Service.DocumentManagement.Doc_DocumentoService.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.DocumentManagement
{
    // Parti comuni dei comandi del Documentale della chat (ANALISI_VOCALI_DOCUMENTALE.md): ruoli, slot dei filtri,
    // ricerca dei documenti. La visibilità la applica Doc_DocumentoList; il LLM non riceve mai i documenti.
    public static class ChatDocHelper
    {
        // ruoli dell'azienda: il servizio restringe i documenti secondo il ruolo (il dipendente vede i suoi)
        public static List<string> Ruoli => new() { "CompanyPowerAdmin", "CompanyAdmin", "User" };

        // posizione della sezione nell'help: dopo presenze e CRM, prima dei comandi comuni
        public const int CategoryOrder = 700;

        public static string Data(DateTime d) => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        public static string Errore(IEnumerable<Message>? messaggi) =>
            string.Join(" ", (messaggi ?? Enumerable.Empty<Message>()).Where(m => m.MsgType == MessageType.Exception).Select(m => m.Text));

        // "Cedolino ottobre — Cedolino — Rossi Mario — 10/2026"
        public static string Descrivi(Doc_DocumentoListModel d)
        {
            var parti = new List<string> { d.Titolo };
            if (!string.IsNullOrWhiteSpace(d.Tipo) && !d.Titolo.Contains(d.Tipo, StringComparison.OrdinalIgnoreCase)) parti.Add(d.Tipo);
            var persona = d.Dipendente ?? d.Cliente;
            if (!string.IsNullOrWhiteSpace(persona)) parti.Add(persona);
            if (d.Anno.HasValue && d.Mese.HasValue) parti.Add($"{d.Mese:D2}/{d.Anno}");
            else if (d.DataDocumento.HasValue) parti.Add(Data(d.DataDocumento.Value));
            else if (d.DataCaricamento.HasValue) parti.Add(Data(d.DataCaricamento.Value));
            return string.Join(" — ", parti);
        }

        public static string Elenco(string titolo, IEnumerable<string> righe, int max = 10)
        {
            var lista = righe.ToList();
            var sb = new StringBuilder();
            sb.AppendLine(titolo);
            foreach (var r in lista.Take(max))
                sb.AppendLine("  • " + r);
            if (lista.Count > max)
                sb.AppendLine("  " + DocumentManagementMessages.Get("ChatListMore", lista.Count - max));
            return sb.ToString().TrimEnd();
        }

        // ---------------------------------------------------------------------------
        // Slot dei filtri
        // ---------------------------------------------------------------------------

        public static SlotDefinition SlotTipo() => new()
        {
            Name = "documentType",
            Type = "string",
            Required = false,
            PromptDescription = "tipo di documento come detto dall'utente (es. 'cedolino', 'busta paga', 'CU', 'contratto', 'certificato medico', 'F24', 'DURC', 'visura'), al singolare",
            Question = DocumentManagementMessages.Get("ChatTypeQuestion"),
            Label = DocumentManagementMessages.Get("ChatTypeLabel")
        };

        public static SlotDefinition SlotPersona() => new()
        {
            Name = "person",
            Type = "string",
            Required = false,
            IsPerson = true,
            PromptDescription = "dipendente o cliente a cui si riferisce il documento (es. 'di Rossi' -> 'Rossi'); se l'utente parla dei propri documenti ('i miei cedolini') restituisci " + ChatSelf.Token,
            Question = DocumentManagementMessages.Get("ChatPersonQuestion"),
            Label = DocumentManagementMessages.Get("ChatPersonLabel")
        };

        public static SlotDefinition SlotTesto() => new()
        {
            Name = "query",
            Type = "string",
            Required = false,
            PromptDescription = "parole da cercare nel titolo o nel testo dei documenti, SOLO se l'utente cerca un testo preciso (es. 'con scritto Enel' -> 'Enel'); non ripetere tipo, persona, mese o anno",
            Question = DocumentManagementMessages.Get("ChatQueryQuestion"),
            Label = DocumentManagementMessages.Get("ChatQueryLabel"),
            HasRelevantContent = msg => !string.IsNullOrWhiteSpace(msg)
        };

        public static SlotDefinition SlotAnno() => new()
        {
            Name = "year",
            Type = "yyyy",
            Required = false,
            PromptDescription = "anno del documento se detto (es. 'del 2025', 'dell'anno scorso'); copia l'espressione",
            Question = DocumentManagementMessages.Get("ChatYearQuestion"),
            Label = DocumentManagementMessages.Get("ChatYearLabel"),
            Normalizer = NormalizzaAnno,
            Validator = v => int.TryParse(v, out var a) && a >= 1990 && a <= 2100
                ? null
                : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, DocumentManagementMessages.Get("ChatYearQuestion"), "year")
        };

        public static SlotDefinition SlotMese() => new()
        {
            Name = "month",
            Type = "MM/yyyy",
            Required = false,
            PromptDescription = "mese di competenza del documento se detto (es. 'di ottobre', 'ottobre 2025', 'del mese scorso'); copia l'espressione",
            Question = DocumentManagementMessages.Get("ChatMonthQuestion"),
            Label = DocumentManagementMessages.Get("ChatMonthLabel"),
            Normalizer = NormalizzaMese,
            Validator = v => ParseMese(v) != null
                ? null
                : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, DocumentManagementMessages.Get("ChatMonthQuestion"), "month")
        };

        private static readonly Regex _anno = new(@"\b(19|20)\d{2}\b", RegexOptions.CultureInvariant);

        public static string? NormalizzaAnno(string valore)
        {
            var t = ChatAIKeywords.Normalize(valore);
            var oggi = DateTime.Today;
            if (t.Contains("anno scorso") || t.Contains("last year")) return (oggi.Year - 1).ToString(CultureInfo.InvariantCulture);
            if (t.Contains("quest anno") || t.Contains("anno corrente") || t.Contains("this year")) return oggi.Year.ToString(CultureInfo.InvariantCulture);
            var m = _anno.Match(valore ?? string.Empty);
            return m.Success ? m.Value : null;
        }

        public static string? NormalizzaMese(string valore)
        {
            var t = ChatAIKeywords.Normalize(valore);
            var oggi = DateTime.Today;
            if (t.Contains("mese scorso") || t.Contains("scorso mese") || t.Contains("last month")) return oggi.AddMonths(-1).ToString("MM/yyyy", CultureInfo.InvariantCulture);
            if (t.Contains("questo mese") || t.Contains("this month")) return oggi.ToString("MM/yyyy", CultureInfo.InvariantCulture);
            if (ParseMese(valore) is { } mm) return mm.ToString("MM/yyyy", CultureInfo.InvariantCulture);
            // il mese senza anno è quello più recente già iniziato ("il cedolino di dicembre" a novembre = dicembre scorso)
            if (!SlotNormalizers.TryParseMonth(valore, oggi, out var inizio, out _)) return null;
            if (inizio > oggi && !_anno.IsMatch(valore ?? string.Empty)) inizio = inizio.AddYears(-1);
            return inizio.ToString("MM/yyyy", CultureInfo.InvariantCulture);
        }

        public static DateTime? ParseMese(string? valore) =>
            DateTime.TryParseExact(valore, "MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out var d) ? d : null;

        // ---------------------------------------------------------------------------
        // Ricerca
        // ---------------------------------------------------------------------------

        // sinonimi dei tipi predefiniti (DefaultType*), italiano e inglese
        private static readonly string[][] _sinonimiTipo =
        {
            new[] { "cedolino", "cedolini", "busta paga", "buste paga", "payslip", "pay slip", "paycheck" },
            new[] { "certificazione unica", "cu", "cud", "tax certificate" },
            new[] { "contratto", "contratti", "contract", "contracts" },
            new[] { "certificato medico", "certificati medici", "malattia", "medical certificate", "sick note" },
            new[] { "attestato di formazione", "formazione", "attestato", "corso", "training", "training certificate" },
            new[] { "comunicazione", "comunicazioni", "circolare", "avviso", "notice", "communication" },
            new[] { "visura", "visura camerale", "company registration", "chamber of commerce" },
        };

        // elementi il cui testo corrisponde al nome: uguale, poi contenuto a parole intere, poi con tutte le parole (anche iniziali)
        public static List<string> Trova(IEnumerable<string> nomi, string? nome)
        {
            var n = ChatAIKeywords.Normalize(nome);
            if (n.Length == 0) return new();
            var lista = nomi.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

            var uguali = lista.Where(x => ChatAIKeywords.Normalize(x) == n).ToList();
            if (uguali.Count > 0) return uguali;
            var contenuti = lista.Where(x => $" {ChatAIKeywords.Normalize(x)} ".Contains($" {n} ")).ToList();
            if (contenuti.Count > 0) return contenuti;
            var parole = n.Split(' ');
            return lista.Where(x =>
            {
                var w = ChatAIKeywords.Normalize(x).Split(' ');
                return parole.All(p => w.Any(parola => parola.StartsWith(p, StringComparison.Ordinal)));
            }).ToList();
        }

        // tipi (nomi presenti nei documenti) che corrispondono a quello detto, anche con un sinonimo
        public static List<string> TrovaTipi(IEnumerable<string> tipi, string? nome)
        {
            var elenco = tipi.ToList();
            var trovati = Trova(elenco, nome);
            if (trovati.Count > 0) return trovati;

            var n = ChatAIKeywords.Normalize(nome);
            var gruppo = _sinonimiTipo.FirstOrDefault(g => g.Any(s => ChatAIKeywords.Normalize(s) == n));
            return gruppo == null ? new() : gruppo.SelectMany(s => Trova(elenco, s)).Distinct().ToList();
        }

        public record Ricerca(List<Doc_DocumentoListModel> Documenti, bool Troncato, string Filtro);

        // Documenti visibili con i filtri degli slot (tipo, persona, testo, anno, mese).
        // perMe: solo i documenti dell'utente (anche senza slot persona). Errore: lettura non riuscita.
        public static async Task<(Ricerca? Esito, string? Errore)> Cerca(IDoc_DocumentoService service, Dictionary<string, string> slots,
            bool perMe = false, int? scadenzaEntroGiorni = null)
        {
            var persona = slots.GetValueOrDefault("person")?.Trim();
            var miei = perMe || string.Equals(persona, ChatSelf.Token, StringComparison.OrdinalIgnoreCase);
            var anno = int.TryParse(slots.GetValueOrDefault("year"), out var a) ? a : (int?)null;
            var mese = ParseMese(slots.GetValueOrDefault("month"));

            var res = await service.Doc_DocumentoList(new GenericRequest<Doc_DocumentoListInModel>(new Doc_DocumentoListInModel
            {
                Miei = miei,
                Testo = string.IsNullOrWhiteSpace(slots.GetValueOrDefault("query")) ? null : slots["query"].Trim(),
                Anno = mese == null ? anno : null,
                ScadenzaEntroGiorni = scadenzaEntroGiorni
            }), false);
            if (!res.Success || res.Data == null)
                return (null, DocumentManagementMessages.Get("ChatServiceError", Errore(res.Messages)));

            var documenti = res.Data.Documenti.Where(d => !d.NelCestino).ToList();
            var filtri = new List<string>();

            if (slots.TryGetValue("documentType", out var tipo) && !string.IsNullOrWhiteSpace(tipo))
            {
                var tipi = TrovaTipi(documenti.Select(d => d.Tipo), tipo);
                documenti = documenti.Where(d => tipi.Contains(d.Tipo, StringComparer.OrdinalIgnoreCase)).ToList();
                filtri.Add(tipo.Trim());
            }

            if (!miei && !string.IsNullOrWhiteSpace(persona))
            {
                var nomi = Trova(documenti.Select(d => d.Dipendente ?? string.Empty).Concat(documenti.Select(d => d.Cliente ?? string.Empty)), persona);
                documenti = documenti.Where(d => nomi.Contains(d.Dipendente ?? string.Empty, StringComparer.OrdinalIgnoreCase) ||
                                                 nomi.Contains(d.Cliente ?? string.Empty, StringComparer.OrdinalIgnoreCase)).ToList();
                filtri.Add(persona);
            }
            else if (miei)
                filtri.Add(DocumentManagementMessages.Get("ChatMine"));

            if (mese != null)
            {
                var m = mese.Value;
                documenti = documenti.Where(d => d.Anno.HasValue && d.Mese.HasValue
                    ? d.Anno == m.Year && d.Mese == m.Month
                    : d.DataDocumento.HasValue && d.DataDocumento.Value.Year == m.Year && d.DataDocumento.Value.Month == m.Month).ToList();
                filtri.Add(m.ToString("MM/yyyy", CultureInfo.InvariantCulture));
            }
            else if (anno != null)
                filtri.Add(anno.Value.ToString(CultureInfo.InvariantCulture));

            if (!string.IsNullOrWhiteSpace(slots.GetValueOrDefault("query")))
                filtri.Add($"\"{slots["query"].Trim()}\"");

            return (new Ricerca(documenti, res.Data.Troncato, filtri.Count > 0 ? $" ({string.Join(", ", filtri)})" : string.Empty), null);
        }
    }
}
