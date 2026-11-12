using nvxapp.server.data.Entities.Tenant.Assets;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Assets.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Assets
{
    // Parti comuni dei comandi di Risorse aziendali della chat (ANALISI_ASSETS_VOCALI.md): ruoli, formati, slot
    // ricorrenti, ricerca delle risorse prenotabili. Visibilità e regole restano nei servizi del modulo.
    public static class ChatAstHelper
    {
        // prenotazioni e "le mie risorse": tutti gli utenti dell'azienda (come il menu)
        public static List<string> Ruoli => new() { "CompanyPowerAdmin", "CompanyAdmin", "User" };

        // flotta e beni: amministratori dell'azienda (come il menu "Veicoli" e "Beni")
        public static List<string> RuoliAmministratori => new() { "CompanyPowerAdmin", "CompanyAdmin" };

        // posizione delle sezioni nell'help (coordinamento: CRM 650-660, Risorse Umane 680, Compliance 690, Documentale 700, Risorse aziendali 720)
        public const int CategoryOrderPrenotazioni = 720;
        public const int CategoryOrderRisorse = 721;

        // durata di una prenotazione quando l'utente dice solo l'inizio
        public const int DurataPredefinitaMinuti = 60;

        public static string Data(DateTime d) => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        public static string Ora(DateTime d) => d.ToString("HH:mm", CultureInfo.InvariantCulture);

        // "il 12/11/2026" oppure "dal 12/11/2026 al 18/11/2026"
        public static string Periodo(DateTime dal, DateTime al) =>
            dal.Date == al.Date ? AssetsMessages.Get("ChatPeriodDay", Data(dal)) : AssetsMessages.Get("ChatPeriodRange", Data(dal), Data(al));

        // "13/11/2026 10:00-11:00"
        public static string Fascia(DateTime inizio, DateTime fine) =>
            inizio.Date == fine.Date ? $"{Data(inizio)} {Ora(inizio)}-{Ora(fine)}" : $"{Data(inizio)} {Ora(inizio)} - {Data(fine)} {Ora(fine)}";

        public static DateTime ParseData(string? valore, DateTime predefinita) =>
            !string.IsNullOrWhiteSpace(valore) && SlotNormalizers.TryParseDate(valore, DateTime.Today, out var d) ? d.Date : predefinita;

        public static TimeOnly? ParseOra(string? valore) =>
            TimeOnly.TryParseExact(valore, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t) ? t : null;

        public static string Errore(IEnumerable<Message>? messaggi) =>
            string.Join(" ", (messaggi ?? Enumerable.Empty<Message>()).Where(m => m.MsgType == MessageType.Exception).Select(m => m.Text));

        public static CommandResult Fallito<T>(GenericResult<T> res) =>
            CommandResult.Fail(AssetsMessages.Get("ChatServiceError", Errore(res.Messages)));

        // elenco puntato con al massimo max righe
        public static string Elenco(string titolo, IEnumerable<string> righe, int max = 15)
        {
            var lista = righe.ToList();
            var sb = new StringBuilder();
            sb.AppendLine(titolo);
            foreach (var r in lista.Take(max))
                sb.AppendLine("  • " + r);
            if (lista.Count > max)
                sb.AppendLine("  " + AssetsMessages.Get("ChatListMore", lista.Count - max));
            return sb.ToString().TrimEnd();
        }

        // ---------------------------------------------------------------------------
        // Slot ricorrenti
        // ---------------------------------------------------------------------------

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
                    : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, AssetsMessages.Get("ChatInvalidDate", v), name)
            };
        }

        public static SlotDefinition SlotOrario(string name, bool required, string question, string label, string? significato = null)
        {
            var sb = new StringBuilder();
            if (!string.IsNullOrEmpty(significato)) sb.Append(significato).Append(". ");
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Orario(sb);
            return new SlotDefinition
            {
                Name = name,
                Type = SlotNormalizers.TimeFormat,
                Required = required,
                PromptDescription = sb.ToString(),
                Question = question,
                Label = label,
                Validator = v => ParseOra(v) != null
                    ? null
                    : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, AssetsMessages.Get("ChatInvalidTime", v), name)
            };
        }

        // risorsa prenotabile: nome ("sala riunioni", "Fiat Panda") o solo il tipo ("una sala", "un'auto");
        // si cerca all'esecuzione tra le risorse attive (RisolviRisorse)
        public static SlotDefinition SlotRisorsa(bool required, string? significato = null) => new()
        {
            Name = "resource",
            Type = "string",
            Required = required,
            PromptDescription = (significato ?? "risorsa da prenotare") +
                ": nome della sala, postazione o auto come detto dall'utente (es. 'la sala riunioni' -> 'sala riunioni', 'la Panda' -> 'Panda'), " +
                "oppure solo il tipo se l'utente non indica quale ('una sala' -> 'sala', 'una postazione' -> 'postazione', 'un'auto' -> 'auto')",
            Question = AssetsMessages.Get("ChatResourceQuestion"),
            Label = AssetsMessages.Get("ChatResourceLabel")
        };

        // tipo di risorsa per l'interrogazione delle risorse libere
        public static SlotDefinition SlotTipoRisorsa() => new()
        {
            Name = "resourceType",
            Type = "string",
            Required = false,
            PromptDescription = "tipo di risorsa se l'utente lo dice: sala, postazione o auto (es. 'sale riunioni' -> 'sala', 'scrivanie' -> 'postazione', 'macchine' -> 'auto'), altrimenti ometti",
            Question = AssetsMessages.Get("ChatResourceTypeQuestion"),
            Label = AssetsMessages.Get("ChatResourceTypeLabel"),
            Normalizer = v => TipoDaParola(v) is { } t ? NomeTipo(t) : null,
            Validator = v => TipoDaParola(v) != null
                ? null
                : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, AssetsMessages.Get("ChatResourceTypeQuestion"), "resourceType")
        };

        // ---------------------------------------------------------------------------
        // Tipi di risorsa e ricerca per nome
        // ---------------------------------------------------------------------------

        private static readonly (AstTipoRisorsa Tipo, string[] Parole)[] _paroleTipo =
        {
            (AstTipoRisorsa.Sala, new[] { "sala", "sale", "sala riunioni", "sale riunioni", "saletta", "salette", "aula", "meeting room", "meeting rooms", "room", "rooms" }),
            (AstTipoRisorsa.Postazione, new[] { "postazione", "postazioni", "scrivania", "scrivanie", "desk", "desks", "workstation", "workstations" }),
            (AstTipoRisorsa.AutoPool, new[] { "auto", "macchina", "macchine", "automobile", "auto aziendale", "auto aziendali", "auto in pool", "vettura", "car", "cars", "pool car", "company car" }),
        };

        // nome canonico del tipo (valore dello slot resourceType)
        public static string NomeTipo(AstTipoRisorsa tipo) => tipo switch
        {
            AstTipoRisorsa.Sala => "sala",
            AstTipoRisorsa.AutoPool => "auto",
            _ => "postazione",
        };

        // tipo di risorsa se il testo è (o finisce con) una parola del tipo: "una sala", "le auto"
        public static AstTipoRisorsa? TipoDaParola(string? testo)
        {
            var n = ChatAIKeywords.Normalize(testo);
            if (n.Length == 0) return null;
            foreach (var (tipo, parole) in _paroleTipo)
                if (parole.Any(p => n == p || n.EndsWith(" " + p, StringComparison.Ordinal)))
                    return tipo;
            return null;
        }

        public static string EtichettaTipo(int tipo) => AssetsMessages.Get("ChatResourceType_" + tipo);

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

        // risorse che corrispondono al nome detto: per nome (descrizione o veicolo), altrimenti tutte quelle del tipo
        // ("una sala"). Generico = true se si è scelto per tipo: tra queste va bene una qualsiasi libera.
        public static (List<Ast_RisorsaModel> Risorse, bool Generico) RisolviRisorse(IEnumerable<Ast_RisorsaModel> risorse, string? nome)
        {
            var attive = risorse.Where(r => r.Attiva).ToList();
            var tipo = TipoDaParola(nome);

            // senza articolo: "una sala" -> "sala", "l'auto" -> "auto"
            var n = ChatAIKeywords.Normalize(nome);
            foreach (var articolo in new[] { "una ", "un ", "uno ", "la ", "le ", "il ", "l ", "a ", "an ", "the " })
                if (n.StartsWith(articolo, StringComparison.Ordinal)) { n = n[articolo.Length..]; break; }

            // solo la parola del tipo ("sala", "auto aziendale"): la risorsa che si chiama così, altrimenti tutte quelle del tipo
            var soloTipo = tipo != null && _paroleTipo.First(x => x.Tipo == tipo).Parole.Contains(n);
            if (soloTipo)
            {
                var stessoNome = attive.Where(r => ChatAIKeywords.Normalize(r.Descrizione) == n).ToList();
                if (stessoNome.Count > 0) return (stessoNome, false);
                // "sala riunioni" con "Sala riunioni 1" e "Sala riunioni 2": una qualsiasi delle due, altrimenti del tipo
                var delTipo = attive.Where(r => r.Tipo == (int)tipo!).ToList();
                var simili = Trova(delTipo, n, r => r.Descrizione);
                return (simili.Count > 0 ? simili : delTipo, true);
            }

            var perNome = Trova(attive, n, r => string.IsNullOrEmpty(r.Veicolo) ? r.Descrizione : $"{r.Descrizione} {r.Veicolo}");
            return (perNome, false);
        }

        // "Sala riunioni (sala, Milano)"
        public static string DescriviRisorsa(Ast_RisorsaModel r)
        {
            var dettagli = new List<string> { EtichettaTipo(r.Tipo) };
            if (!string.IsNullOrWhiteSpace(r.Sede)) dettagli.Add(r.Sede!);
            if (r.Capienza is > 0) dettagli.Add(AssetsMessages.Get("ChatResourceSeats", r.Capienza));
            return $"{r.Descrizione} ({string.Join(", ", dettagli)})";
        }

        // giorni detti dall'utente: "60", "60 giorni", "un mese", "due settimane", "un anno"
        public static string? NormalizzaGiorni(string? valore)
        {
            var n = ChatAIKeywords.Normalize(valore);
            if (n.Length == 0) return null;
            if (int.TryParse(n, out var g)) return g.ToString(CultureInfo.InvariantCulture);

            var parole = n.Split(' ').Where(p => p is not ("entro" or "nei" or "prossimi" or "prossime" or "i" or "le" or "next" or "within" or "the")).ToArray();
            if (parole.Length == 0) return null;
            int quanti = 1;
            int idx = 0;
            if (int.TryParse(parole[0], out var num)) { quanti = num; idx = 1; }
            else if (_numeri.TryGetValue(parole[0], out var num2)) { quanti = num2; idx = 1; }
            var unita = parole.Length > idx ? parole[idx] : string.Empty;
            int? giorni = unita switch
            {
                "giorno" or "giorni" or "day" or "days" => quanti,
                "settimana" or "settimane" or "week" or "weeks" => quanti * 7,
                "mese" or "mesi" or "month" or "months" => quanti * 30,
                "anno" or "anni" or "year" or "years" => quanti * 365,
                _ => null
            };
            return giorni?.ToString(CultureInfo.InvariantCulture);
        }

        private static readonly Dictionary<string, int> _numeri = new()
        {
            ["un"] = 1, ["uno"] = 1, ["una"] = 1, ["a"] = 1, ["one"] = 1, ["due"] = 2, ["two"] = 2, ["tre"] = 3, ["three"] = 3,
            ["quattro"] = 4, ["four"] = 4, ["cinque"] = 5, ["five"] = 5, ["sei"] = 6, ["six"] = 6, ["dieci"] = 10, ["ten"] = 10,
            ["quindici"] = 15, ["fifteen"] = 15, ["venti"] = 20, ["twenty"] = 20, ["trenta"] = 30, ["thirty"] = 30,
            ["sessanta"] = 60, ["sixty"] = 60, ["novanta"] = 90, ["ninety"] = 90,
        };
    }
}
