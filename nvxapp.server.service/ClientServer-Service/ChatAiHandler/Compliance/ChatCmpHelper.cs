using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Compliance;
using nvxapp.server.service.ClientServer_Service.Compliance.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Compliance
{
    // Parti comuni dei comandi di Compliance della chat (ANALISI_COMPLIANCE_VOCALI.md): ruoli, formati, slot ricorrenti,
    // ricerca di dipendenti, DPI e tipi di corso. Permessi e visibilità restano nei servizi di Compliance (CmpUtility):
    // dalla chat l'utente vede esattamente ciò che vede nelle pagine. Le risposte si compongono qui, mai dal LLM;
    // i dati sanitari (esito, limitazioni, medico, note sanitarie) non compaiono mai nelle risposte.
    public static class ChatCmpHelper
    {
        // amministratori dell'azienda: sicurezza sul lavoro e registro GDPR (come il menu del modulo)
        public static List<string> RuoliAmministratori => new() { "CompanyPowerAdmin", "CompanyAdmin" };

        // "I miei adempimenti": amministratori e dipendenti
        public static List<string> RuoliTutti => new() { "CompanyPowerAdmin", "CompanyAdmin", "User" };

        // posizione delle sezioni nell'help: dopo Risorse Umane (680), prima di documentale e Risorse aziendali (700)
        public const int CategoryOrderSicurezza = 690;
        public const int CategoryOrderGdpr = 695;

        public static string Data(DateTime d) => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        public static string DataOra(DateTime d) => d.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

        public static DateTime ParseData(string? valore, DateTime predefinita) =>
            !string.IsNullOrWhiteSpace(valore) && SlotNormalizers.TryParseDate(valore, DateTime.Today, out var d) ? d.Date : predefinita;

        // testo degli errori restituiti da un servizio
        public static string Errore(IEnumerable<Message>? messaggi) =>
            string.Join(" ", (messaggi ?? Enumerable.Empty<Message>()).Where(m => m.MsgType == MessageType.Exception).Select(m => m.Text));

        public static CommandResult Fallito<T>(GenericResult<T> res) =>
            CommandResult.Fail(ComplianceMessages.Get("ChatServiceError", Errore(res.Messages)));

        // elenco puntato con al massimo max righe
        public static string Elenco(string titolo, IEnumerable<string> righe, int max = 15)
        {
            var lista = righe.ToList();
            var sb = new StringBuilder();
            sb.AppendLine(titolo);
            foreach (var r in lista.Take(max))
                sb.AppendLine("  • " + r);
            if (lista.Count > max)
                sb.AppendLine("  " + ComplianceMessages.Get("ChatListMore", lista.Count - max));
            return sb.ToString().TrimEnd();
        }

        // descrizioni dei codici
        public static string TipoCorso(int tipo) => ComplianceMessages.Get("ChatCourseType_" + Math.Clamp(tipo, 0, 10));
        public static string TipoDocumento(int tipo) => ComplianceMessages.Get("ChatDocType_" + Math.Clamp(tipo, 0, 4));
        public static string TipoNomina(int tipo) => ComplianceMessages.Get("AppointmentType_" + Math.Clamp(tipo, 0, 8));

        // ---------------------------------------------------------------------------
        // Slot ricorrenti
        // ---------------------------------------------------------------------------

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
                    : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, ComplianceMessages.Get("ChatInvalidDate", v), name)
            };
        }

        // dipendente: il nome si cerca all'esecuzione tra i dipendenti visibili (RisolviDipendente)
        public static SlotDefinition SlotDipendente(bool required) => new()
        {
            Name = "person",
            Type = "string",
            Required = required,
            PromptDescription = "cognome e/o nome del dipendente come detto dall'utente (es. 'Rossi', 'Mario Rossi'); ometti se non nominato",
            Question = ComplianceMessages.Get("ChatPersonQuestion"),
            Label = ComplianceMessages.Get("ChatPersonLabel")
        };

        // entro quanti giorni: vuoto = giorni di preavviso del modulo
        public static SlotDefinition SlotGiorni() => new()
        {
            Name = "days",
            Type = "int",
            Required = false,
            PromptDescription = "entro quanti giorni, se l'utente lo dice (es. 'nei prossimi 60 giorni' -> '60', 'entro un mese' -> '30', 'entro due mesi' -> '60'); altrimenti ometti",
            Question = ComplianceMessages.Get("ChatDaysQuestion"),
            Label = ComplianceMessages.Get("ChatDaysLabel"),
            Normalizer = NormalizzaGiorni,
            Validator = v => int.TryParse(v, out var g) && g > 0 && g <= 3650
                ? null
                : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, ComplianceMessages.Get("ChatDaysQuestion"), "days")
        };

        // "60", "60 giorni", "un mese", "2 mesi", "una settimana", "un anno" -> giorni
        public static string? NormalizzaGiorni(string valore)
        {
            var n = ChatAIKeywords.Normalize(valore);
            if (int.TryParse(n, out var g)) return g.ToString(CultureInfo.InvariantCulture);
            var parole = n.Split(' ', StringSplitOptions.RemoveEmptyEntries).ToList();
            int numero = 1;
            var numeri = new Dictionary<string, int>
            {
                ["un"] = 1, ["uno"] = 1, ["una"] = 1, ["a"] = 1, ["one"] = 1, ["due"] = 2, ["two"] = 2, ["tre"] = 3, ["three"] = 3,
                ["quattro"] = 4, ["four"] = 4, ["sei"] = 6, ["six"] = 6, ["dodici"] = 12, ["twelve"] = 12
            };
            foreach (var p in parole)
            {
                if (int.TryParse(p, out var x)) numero = x;
                else if (numeri.TryGetValue(p, out var y)) numero = y;
            }
            int? unita = parole.Any(p => p.StartsWith("giorn") || p.StartsWith("day")) ? 1
                       : parole.Any(p => p.StartsWith("settiman") || p.StartsWith("week")) ? 7
                       : parole.Any(p => p.StartsWith("mes") || p.StartsWith("month")) ? 30
                       : parole.Any(p => p.StartsWith("ann") || p.StartsWith("year")) ? 365
                       : null;
            return unita == null ? null : (numero * unita.Value).ToString(CultureInfo.InvariantCulture);
        }

        // ---------------------------------------------------------------------------
        // Ricerca per nome
        // ---------------------------------------------------------------------------

        // elementi il cui testo corrisponde al nome: uguale, poi contenuto a parole intere, poi con tutte le parole
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

        // tabelle di Compliance visibili all'utente (dipendenti, mansioni, DPI, giorni di preavviso)
        public static async Task<(Cmp_LookupOutModel? Lookup, string? Errore)> Lookup(ICmp_SicurezzaService service)
        {
            var res = await service.Cmp_Lookup(new GenericRequest<Cmp_ListInModel>(new Cmp_ListInModel()), false);
            return res.Success && res.Data != null
                ? (res.Data, null)
                : (null, ComplianceMessages.Get("ChatServiceError", Errore(res.Messages)));
        }

        // dipendente dal nome detto, tra quelli della tabella; errore se non trovato o ambiguo (si preferiscono gli attivi)
        public static (Cmp_VoceModel? Dipendente, string? Errore) RisolviDipendente(Cmp_LookupOutModel lookup, string? nome)
        {
            var trovati = Trova(lookup.Dipendenti, nome, d => d.Nome);
            if (trovati.Count > 1 && trovati.Any(d => d.Attivo))
                trovati = trovati.Where(d => d.Attivo).ToList();
            if (trovati.Count == 0)
                return (null, ComplianceMessages.Get("ChatPersonNotFound", nome));
            if (trovati.Count > 1)
                return (null, ComplianceMessages.Get("ChatPersonAmbiguous", trovati.Count, nome, string.Join(", ", trovati.Take(8).Select(d => d.Nome))));
            return (trovati[0], null);
        }

        // DPI attivo del catalogo dal nome detto
        public static (Cmp_VoceModel? Dpi, string? Errore) RisolviDpi(Cmp_LookupOutModel lookup, string? nome)
        {
            var attivi = lookup.Dpi.Where(d => d.Attivo).ToList();
            var trovati = Trova(attivi, nome, d => d.Nome);
            // "guanti" e "guanto": si riprova senza l'ultima lettera
            if (trovati.Count == 0 && !string.IsNullOrWhiteSpace(nome) && nome.Trim().Length > 4)
                trovati = Trova(attivi, nome.Trim()[..^1], d => d.Nome);
            if (trovati.Count == 0)
                return (null, ComplianceMessages.Get("ChatPpeNotFound", nome, string.Join(", ", attivi.Take(12).Select(d => d.Nome))));
            if (trovati.Count > 1)
                return (null, ComplianceMessages.Get("ChatPpeAmbiguous", trovati.Count, nome, string.Join(", ", trovati.Take(8).Select(d => d.Nome))));
            return (trovati[0], null);
        }

        // sinonimi dei tipi di corso (codici di CmpTesti), italiano e inglese
        private static readonly string[][] _sinonimiCorso =
        {
            new[] { "formazione generale", "generale", "corso generale", "general training", "general" },
            new[] { "formazione specifica", "specifica", "rischio specifico", "specific training", "specific" },
            new[] { "aggiornamento", "aggiornamento quinquennale", "refresher", "refresher course", "update" },
            new[] { "antincendio", "incendio", "prevenzione incendi", "emergenze", "fire prevention", "fire safety", "fire" },
            new[] { "primo soccorso", "pronto soccorso", "first aid" },
            new[] { "preposti", "preposto", "supervisors", "supervisor" },
            new[] { "dirigenti", "dirigente", "executives", "executive" },
            new[] { "rls", "rappresentante dei lavoratori", "rappresentante sicurezza", "workers representative" },
            new[] { "rspp", "aspp", "rspp aspp", "safety manager", "safety officer" },
            new[] { "attrezzature", "carrelli", "carrello elevatore", "muletto", "ple", "piattaforme", "gru", "forklift", "equipment", "mewp" },
            new[] { "altro", "other" },
        };

        // codice del tipo di corso dal nome detto (null se non riconosciuto)
        public static int? TrovaTipoCorso(string? nome)
        {
            var n = ChatAIKeywords.Normalize(nome);
            if (n.Length == 0) return null;
            n = n.Replace("corso ", string.Empty).Replace("course ", string.Empty).Replace("di ", string.Empty).Trim();
            for (int i = 0; i < _sinonimiCorso.Length; i++)
                if (_sinonimiCorso[i].Any(s => ChatAIKeywords.Normalize(s) == n))
                    return i;
            for (int i = 0; i < _sinonimiCorso.Length - 1; i++)
                if (_sinonimiCorso[i].Any(s => $" {n} ".Contains($" {ChatAIKeywords.Normalize(s)} ")))
                    return i;
            return null;
        }

        public static string TipiCorso() => string.Join(", ", Enumerable.Range(0, 11).Select(TipoCorso));
    }
}
