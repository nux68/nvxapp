using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.HumanResources;
using nvxapp.server.service.ClientServer_Service.HumanResources.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.HumanResources
{
    // Parti comuni dei comandi di Risorse Umane della chat (ANALISI_HUMANRESOURCES_VOCALI.md): ruoli, formati, slot ricorrenti,
    // ricerca di dipendenti e corsi. La visibilità dei dati resta nei servizi del modulo (HrUtility: amministratore o dipendente);
    // il LLM non riceve mai i dati del modulo, le risposte si compongono qui.
    public static class ChatHrHelper
    {
        // ufficio del personale: come il menu di Risorse Umane (Pages4CompanyAdmin)
        public static List<string> RuoliUfficio => new() { "CompanyPowerAdmin", "CompanyAdmin" };

        // area del dipendente ("I miei cedolini", Pages4User): qualsiasi utente dell'azienda
        public static List<string> RuoliDipendente => new() { "CompanyPowerAdmin", "CompanyAdmin", "User" };

        // posizione della sezione nell'help: dopo presenze e CRM, prima del Documentale
        public const int CategoryOrder = 680;

        public static string Data(DateTime d) => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);

        // "10/2026" oppure "2026" (CU)
        public static string Periodo(int anno, int? mese) =>
            mese.HasValue ? $"{mese:D2}/{anno}" : anno.ToString(CultureInfo.InvariantCulture);

        public static DateTime ParseData(string? valore, DateTime predefinita) =>
            !string.IsNullOrWhiteSpace(valore) && SlotNormalizers.TryParseDate(valore, DateTime.Today, out var d) ? d.Date : predefinita;

        public static string Errore(IEnumerable<Message>? messaggi) =>
            string.Join(" ", (messaggi ?? Enumerable.Empty<Message>()).Where(m => m.MsgType == MessageType.Exception).Select(m => m.Text));

        public static CommandResult Fallito<T>(GenericResult<T> res) =>
            CommandResult.Fail(HumanResourcesMessages.Get("ChatServiceError", Errore(res.Messages)));

        // elenco puntato con al massimo max righe
        public static string Elenco(string titolo, IEnumerable<string> righe, int max = 15)
        {
            var lista = righe.ToList();
            var sb = new StringBuilder();
            sb.AppendLine(titolo);
            foreach (var r in lista.Take(max))
                sb.AppendLine("  • " + r);
            if (lista.Count > max)
                sb.AppendLine("  " + HumanResourcesMessages.Get("ChatListMore", lista.Count - max));
            return sb.ToString().TrimEnd();
        }

        // " (Rossi, corso sicurezza)" oppure ""
        public static string Filtro(IEnumerable<string?> filtri)
        {
            var f = filtri.Where(x => !string.IsNullOrWhiteSpace(x)).ToList();
            return f.Count > 0 ? $" ({string.Join(", ", f)})" : string.Empty;
        }

        // ---------------------------------------------------------------------------
        // Slot ricorrenti
        // ---------------------------------------------------------------------------

        // dipendente: il nome si cerca all'esecuzione tra i dipendenti dell'azienda (Trova)
        public static SlotDefinition SlotDipendente(bool required) => new()
        {
            Name = "employeeName",
            Type = "string",
            Required = required,
            PromptDescription = "cognome (ed eventualmente nome) del dipendente come detto dall'utente, senza 'il dipendente'",
            Question = HumanResourcesMessages.Get("ChatEmployeeQuestion"),
            Label = HumanResourcesMessages.Get("ChatEmployeeLabel")
        };

        public static SlotDefinition SlotCorso(bool required) => new()
        {
            Name = "course",
            Type = "string",
            Required = required,
            PromptDescription = "titolo o parola del corso come detto dall'utente (es. 'il corso sicurezza' -> 'sicurezza', 'primo soccorso')",
            Question = HumanResourcesMessages.Get("ChatCourseQuestion"),
            Label = HumanResourcesMessages.Get("ChatCourseLabel")
        };

        public static SlotDefinition SlotTesto(string name, bool required, string promptDescription, string question, string label) => new()
        {
            Name = name,
            Type = "string",
            Required = required,
            PromptDescription = promptDescription,
            Question = question,
            Label = label,
            HasRelevantContent = msg => !string.IsNullOrWhiteSpace(msg)
        };

        // data: il LLM copia l'espressione, la converte SlotNormalizers (tipo "dd/MM/yyyy")
        public static SlotDefinition SlotData(string name, bool required, string defaultValue, string question, string label, string? significato = null)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(significato)) sb.Append(significato).Append(". ");
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Data_Oggi(sb);
            return new SlotDefinition
            {
                Name = name,
                Type = SlotNormalizers.DateFormat,
                Required = required,
                Default = defaultValue,
                PromptDescription = sb.ToString(),
                Question = question,
                Label = label,
                Validator = v => DateOnly.TryParseExact(v, "dd/MM/yyyy", CultureInfo.InvariantCulture, DateTimeStyles.None, out _)
                    ? null
                    : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, HumanResourcesMessages.Get("ChatInvalidDate", v), name)
            };
        }

        public static SlotDefinition SlotAnno() => new()
        {
            Name = "year",
            Type = "yyyy",
            Required = false,
            PromptDescription = "anno se detto (es. 'del 2025', 'dell'anno scorso'); copia l'espressione, altrimenti ometti",
            Question = HumanResourcesMessages.Get("ChatYearQuestion"),
            Label = HumanResourcesMessages.Get("ChatYearLabel"),
            Normalizer = NormalizzaAnno,
            Validator = v => int.TryParse(v, out var a) && a >= 2000 && a <= 2100
                ? null
                : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, HumanResourcesMessages.Get("ChatYearQuestion"), "year")
        };

        public static string? NormalizzaAnno(string valore)
        {
            var t = ChatAIKeywords.Normalize(valore);
            var oggi = DateTime.Today;
            if (t.Contains("anno scorso") || t.Contains("last year")) return (oggi.Year - 1).ToString(CultureInfo.InvariantCulture);
            if (t.Contains("anno prossimo") || t.Contains("prossimo anno") || t.Contains("next year")) return (oggi.Year + 1).ToString(CultureInfo.InvariantCulture);
            if (t.Contains("quest anno") || t.Contains("anno corrente") || t.Contains("this year")) return oggi.Year.ToString(CultureInfo.InvariantCulture);
            var m = System.Text.RegularExpressions.Regex.Match(valore ?? string.Empty, @"\b(19|20)\d{2}\b");
            return m.Success ? m.Value : null;
        }

        // "nei prossimi 60 giorni" -> 60, "entro due mesi" -> 60 (null se non c'è un numero o un'unità)
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
            return (quanti * unita).ToString(CultureInfo.InvariantCulture);
        }

        // ---------------------------------------------------------------------------
        // Ricerca per nome
        // ---------------------------------------------------------------------------

        // elementi il cui testo corrisponde al nome: uguale, poi contenuto a parole intere, poi con tutte le parole (anche iniziali)
        public static List<T> Trova<T>(IEnumerable<T> elementi, string? nome, Func<T, string?> testo)
        {
            var n = ChatAIKeywords.Normalize(nome);
            if (n.Length == 0) return new();
            var lista = elementi.ToList();

            var uguali = lista.Where(x => ChatAIKeywords.Normalize(testo(x)) == n).ToList();
            if (uguali.Count > 0) return uguali;

            var contenuti = lista.Where(x => $" {ChatAIKeywords.Normalize(testo(x))} ".Contains($" {n} ")).ToList();
            if (contenuti.Count > 0) return contenuti;

            var parole = n.Split(' ');
            return lista.Where(x =>
            {
                var w = ChatAIKeywords.Normalize(testo(x)).Split(' ');
                return parole.All(p => w.Any(parola => parola.StartsWith(p, StringComparison.Ordinal)));
            }).ToList();
        }

        // un solo nome tra quelli (distinti) che corrispondono a quello detto; errore se nessuno o più d'uno
        public static (string? Nome, string? Errore) UnNome(IEnumerable<string> nomi, string? detto, string chiaveNonTrovato)
        {
            var trovati = Trova(nomi.Where(x => !string.IsNullOrWhiteSpace(x)).Distinct(StringComparer.OrdinalIgnoreCase), detto, x => x);
            if (trovati.Count == 0)
                return (null, HumanResourcesMessages.Get(chiaveNonTrovato, detto));
            if (trovati.Count > 1)
                return (null, HumanResourcesMessages.Get("ChatAmbiguous", trovati.Count, detto, string.Join(", ", trovati.Take(8))));
            return (trovati[0], null);
        }

        public static string NomeDipendente(string? cognome, string? nome) => $"{cognome} {nome}".Trim();

        // dipendente dell'azienda dal nome detto (anche i cessati: prima si cercano gli attivi)
        public static async Task<(Hr_DipendenteModel? Dipendente, string? Errore)> RisolviDipendente(IHr_DipendenteService service, string? nome)
        {
            var res = await service.Hr_DipendenteList(new GenericRequest<Hr_DipendenteListInModel>(new Hr_DipendenteListInModel { SoloAttivi = false }), false);
            if (!res.Success || res.Data == null)
                return (null, HumanResourcesMessages.Get("ChatServiceError", Errore(res.Messages)));

            var tutti = res.Data.Dipendenti;
            var trovati = Trova(tutti.Where(d => d.Attivo), nome, d => NomeDipendente(d.Cognome, d.Nome));
            if (trovati.Count == 0)
                trovati = Trova(tutti, nome, d => NomeDipendente(d.Cognome, d.Nome));
            if (trovati.Count == 0)
                return (null, HumanResourcesMessages.Get("ChatEmployeeNotFound", nome));
            if (trovati.Count > 1)
                return (null, HumanResourcesMessages.Get("ChatAmbiguous", trovati.Count, nome,
                    string.Join(", ", trovati.Take(8).Select(d => NomeDipendente(d.Cognome, d.Nome)))));
            return (trovati[0], null);
        }

        // "Assunzione" / "Cessazione"
        public static string TipoChecklist(int tipo) =>
            HumanResourcesMessages.Get(tipo == 2 ? "ChatChecklistTypeEnd" : "ChatChecklistTypeHire");
    }
}
