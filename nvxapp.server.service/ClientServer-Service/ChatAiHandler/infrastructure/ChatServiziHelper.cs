using nvxapp.server.data.Infrastructure.Tenancy;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using System.Text;
using System.Text.RegularExpressions;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.infrastructure
{
    // Funzioni comuni ai comandi di lettura dei servizi comuni (ticket, notifiche, scadenzario, richieste da approvare):
    // formati di data, elenchi troncati, slot con scelta fra valori fissi e slot numerici.
    // ANALISI_INFRASTRUCTURE_MCP.md, MCP-30.
    public static class ChatServiziHelper
    {
        // ruoli di chi lavora in un'azienda (come il menu dello scadenzario e delle richieste)
        public static List<string> RuoliAzienda => new() { "CompanyPowerAdmin", "CompanyAdmin", "User" };
        public static readonly string[] RuoliAmministratoriAzienda = { "CompanyPowerAdmin", "CompanyAdmin" };

        // righe al massimo nel testo (chat e voce); i dati strutturati ne portano di piu'
        public const int MaxRigheTesto = 15;

        // testo lungo nei dati strutturati (messaggi, risposte): oltre si tronca
        public const int MaxTestoDati = 1000;

        public static string Data(DateTime d) => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        public static string DataOra(DateTime d) => d.ToString("dd/MM/yyyy HH:mm", CultureInfo.InvariantCulture);

        // date nei dati per l'AI: ISO
        public static string? Iso(DateTime? d) => d?.ToString("yyyy-MM-dd", CultureInfo.InvariantCulture);
        public static string? IsoOra(DateTime? d) => d?.ToString("HH:mm", CultureInfo.InvariantCulture);

        public static string? Tronca(string? testo, int max)
        {
            if (string.IsNullOrEmpty(testo)) return testo;
            testo = testo.Trim();
            return testo.Length <= max ? testo : testo[..max].TrimEnd() + "…";
        }

        // testo su una riga sola (per l'elenco della chat)
        public static string UnaRiga(string? testo, int max) =>
            Tronca(Regex.Replace(testo ?? string.Empty, @"\s+", " "), max) ?? string.Empty;

        public static string NomeApplicativo(int application) => ChatAIMessages.Get("ChatApplication_" + application);
        public static string NomeApplicativo(ApplicationType application) => NomeApplicativo((int)application);

        public static string Errore(IEnumerable<Message>? messaggi) =>
            string.Join(" ", (messaggi ?? Enumerable.Empty<Message>()).Where(m => m.MsgType == MessageType.Exception).Select(m => m.Text));

        public static CommandResult Fallito<T>(GenericResult<T> res) =>
            CommandResult.Fail(ChatAIMessages.Get("ChatServiceError", Errore(res.Messages)));

        // elenco puntato con al massimo max righe
        public static string Elenco(string titolo, IEnumerable<string> righe, int max = MaxRigheTesto)
        {
            var lista = righe.ToList();
            var sb = new StringBuilder();
            sb.AppendLine(titolo);
            foreach (var r in lista.Take(max))
                sb.AppendLine("  • " + r);
            if (lista.Count > max)
                sb.AppendLine("  " + ChatAIMessages.Get("ChatListMore", lista.Count - max));
            return sb.ToString().TrimEnd();
        }

        // ---------------------------------------------------------------------------
        // Slot
        // ---------------------------------------------------------------------------

        /// <summary>
        /// slot con un valore fra pochi codici fissi (es. "mine" / "handled"): il valore canonico o un'espressione
        /// italiana o inglese che lo contiene ("aperti da me" -> mine). parole: codice -> espressioni riconosciute,
        /// nell'ordine in cui si cercano.
        /// </summary>
        public static SlotDefinition SlotScelta(string nome, string predefinito, string promptDescription, string question, string label,
                                                params (string Codice, string[] Parole)[] parole)
        {
            var codici = parole.Select(p => p.Codice).ToList();
            return new SlotDefinition
            {
                Name = nome,
                Type = string.Join("/", codici),
                Required = false,
                Default = predefinito,
                PromptDescription = promptDescription,
                Question = question,
                Label = label,
                Normalizer = v => Scelta(v, parole),
                Validator = v => codici.Contains(v)
                    ? null
                    : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, question, nome)
            };
        }

        private static string? Scelta(string valore, (string Codice, string[] Parole)[] parole)
        {
            var esatto = parole.FirstOrDefault(p => p.Codice.Equals(valore.Trim(), StringComparison.OrdinalIgnoreCase));
            if (esatto.Codice != null)
                return esatto.Codice;

            var testo = " " + ChatAIKeywords.Normalize(valore) + " ";
            foreach (var (codice, espressioni) in parole)
                if (espressioni.Any(e => testo.Contains(" " + ChatAIKeywords.Normalize(e) + " ")))
                    return codice;
            return null;
        }

        /// <summary>slot numerico intero fra minimo e massimo (il primo numero del testo: "le ultime 20" -> 20)</summary>
        public static SlotDefinition SlotNumero(string nome, int predefinito, int minimo, int massimo, string promptDescription, string question, string label)
        {
            var errore = ChatAIMessages.Get("ChatNumberInvalid", minimo, massimo);
            return new SlotDefinition
            {
                Name = nome,
                Type = "number",
                Required = false,
                Default = predefinito.ToString(CultureInfo.InvariantCulture),
                PromptDescription = promptDescription,
                Question = question,
                Label = label,
                HasRelevantContent = msg => msg.Any(char.IsDigit),
                Normalizer = v =>
                {
                    var m = Regex.Match(v, @"\d+");
                    return m.Success ? m.Value : null;
                },
                Validator = v => int.TryParse(v, NumberStyles.None, CultureInfo.InvariantCulture, out var n) && n >= minimo && n <= massimo
                    ? null
                    : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, errore, nome)
            };
        }

        public static int Numero(Dictionary<string, string> slots, string nome, int predefinito, int minimo, int massimo) =>
            Math.Clamp(int.TryParse(slots.GetValueOrDefault(nome), NumberStyles.None, CultureInfo.InvariantCulture, out var n) ? n : predefinito, minimo, massimo);
    }
}
