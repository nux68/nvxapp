using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Crm;
using nvxapp.server.service.ClientServer_Service.Crm.Models;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.ClientServer_Service.ModelsBase;
using System.Globalization;
using System.Text;

namespace nvxapp.server.service.ClientServer_Service.ChatAiHandler.Crm
{
    // Parti comuni dei comandi CRM della chat (ANALISI_VOCALI_CRM.md): ruoli, formati, slot ricorrenti,
    // ricerca di clienti e tipi di attività. La visibilità dei dati resta nei servizi del CRM.
    public static class ChatCrmHelper
    {
        // ruoli che usano il CRM (come il menu): i servizi limitano i dati secondo la visibilità
        public static List<string> Ruoli => new() { "CompanyPowerAdmin", "CompanyAdmin", "User" };

        // posizione delle sezioni nell'help: dopo le presenze, prima dei comandi comuni
        public const int CategoryOrderAgenda = 650;
        public const int CategoryOrderVendite = 660;

        public static string Data(DateTime d) => d.ToString("dd/MM/yyyy", CultureInfo.InvariantCulture);
        public static string Ora(DateTime d) => d.ToString("HH:mm", CultureInfo.InvariantCulture);

        // "12.500 EUR" (decimali solo se servono)
        public static string Importo(decimal valore, string? valuta)
        {
            var testo = valore == Math.Round(valore) ? valore.ToString("N0", CultureInfo.CurrentCulture) : valore.ToString("N2", CultureInfo.CurrentCulture);
            return string.IsNullOrWhiteSpace(valuta) ? testo : $"{testo} {valuta}";
        }

        // "il 12/11/2026" oppure "dal 12/11/2026 al 18/11/2026"
        public static string Periodo(DateTime dal, DateTime al) =>
            dal.Date == al.Date ? CrmMessages.Get("ChatPeriodDay", Data(dal)) : CrmMessages.Get("ChatPeriodRange", Data(dal), Data(al));

        // valore di uno slot data (già normalizzato "dd/MM/yyyy" o espressione), altrimenti il default
        public static DateTime ParseData(string? valore, DateTime predefinita) =>
            !string.IsNullOrWhiteSpace(valore) && SlotNormalizers.TryParseDate(valore, DateTime.Today, out var d) ? d.Date : predefinita;

        public static TimeOnly? ParseOra(string? valore) =>
            TimeOnly.TryParseExact(valore, "HH:mm", CultureInfo.InvariantCulture, DateTimeStyles.None, out var t) ? t : null;

        // testo degli errori restituiti da un servizio
        public static string Errore(IEnumerable<Message>? messaggi) =>
            string.Join(" ", (messaggi ?? Enumerable.Empty<Message>()).Where(m => m.MsgType == MessageType.Exception).Select(m => m.Text));

        public static CommandResult Fallito<T>(GenericResult<T> res) =>
            CommandResult.Fail(CrmMessages.Get("ChatServiceError", Errore(res.Messages)));

        // elenco puntato con al massimo max righe
        public static string Elenco(string titolo, IEnumerable<string> righe, int max = 15)
        {
            var lista = righe.ToList();
            var sb = new StringBuilder();
            sb.AppendLine(titolo);
            foreach (var r in lista.Take(max))
                sb.AppendLine("  • " + r);
            if (lista.Count > max)
                sb.AppendLine("  " + CrmMessages.Get("ChatListMore", lista.Count - max));
            return sb.ToString().TrimEnd();
        }

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
                    : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, CrmMessages.Get("ChatInvalidDate", v), name)
            };
        }

        // orario: il LLM copia l'espressione, la converte SlotNormalizers (tipo "HH:mm")
        public static SlotDefinition SlotOrario(string name, bool required, string defaultValue, string question, string label)
        {
            var sb = new StringBuilder();
            BuildSystemPromptUtil.BuildSystemPrompt_Append_Orario(sb);
            return new SlotDefinition
            {
                Name = name,
                Type = SlotNormalizers.TimeFormat,
                Required = required,
                Default = defaultValue,
                PromptDescription = sb.ToString(),
                Question = question,
                Label = label,
                Validator = v => ParseOra(v) != null
                    ? null
                    : SlotValidationResult.Failed(SlotValidationError.InvalidFormat, CrmMessages.Get("ChatInvalidTime", v), name)
            };
        }

        // cliente: il nome si cerca all'esecuzione tra i clienti visibili (RisolviCliente)
        public static SlotDefinition SlotCliente(bool required) => new()
        {
            Name = "customerName",
            Type = "string",
            Required = required,
            PromptDescription = "nome del cliente del CRM (ragione sociale o cognome) come detto dall'utente, senza 'il cliente'",
            Question = CrmMessages.Get("ChatCustomerQuestion"),
            Label = CrmMessages.Get("ChatCustomerLabel")
        };

        // testo libero (oggetto, nota, esito): il LLM lo riporta come detto, anche riformulato
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

        public static SlotDefinition SlotTipoAttivita(string defaultValue) => new()
        {
            Name = "activityType",
            Type = "string",
            Required = false,
            Default = defaultValue,
            PromptDescription = "tipo di attività come detto dall'utente: chiamata, appuntamento, email o attività (es. 'telefonata' -> 'chiamata', 'incontro'/'riunione' -> 'appuntamento')",
            Question = CrmMessages.Get("ChatActivityTypeQuestion"),
            Label = CrmMessages.Get("ChatActivityTypeLabel")
        };

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

        // cliente visibile all'utente dal nome detto; errore se non trovato o ambiguo
        public static async Task<(Crm_ClienteListModel? Cliente, string? Errore)> RisolviCliente(ICrm_ClienteService service, string? nome)
        {
            var res = await service.Crm_ClienteList(new GenericRequest<Crm_ClienteListInModel>(new Crm_ClienteListInModel()), false);
            if (!res.Success || res.Data == null)
                return (null, CrmMessages.Get("ChatServiceError", Errore(res.Messages)));

            var trovati = Trova(res.Data.Clienti, nome, c => c.Descrizione);
            if (trovati.Count == 0)
                return (null, CrmMessages.Get("ChatCustomerNotFound", nome));
            if (trovati.Count > 1)
                return (null, CrmMessages.Get("ChatCustomerAmbiguous", trovati.Count, nome, string.Join(", ", trovati.Take(8).Select(c => c.Descrizione))));
            return (trovati[0], null);
        }

        // tabelle e permessi del CRM (tipi di attività, fasi, valuta)
        public static async Task<Crm_SetupGetOutModel?> Setup(ICrm_ConfigService service)
        {
            var res = await service.Crm_SetupGet(new GenericRequest<Crm_EmptyInModel>(new Crm_EmptyInModel()), false);
            return res.Success ? res.Data : null;
        }

        // sinonimi dei tipi di attività predefiniti, italiano e inglese
        private static readonly string[][] _sinonimiTipo =
        {
            new[] { "chiamata", "telefonata", "chiamare", "telefonare", "call", "phone call", "phone" },
            new[] { "appuntamento", "incontro", "riunione", "visita", "meeting", "appointment", "visit" },
            new[] { "email", "mail", "e mail", "posta" },
            new[] { "attività", "attivita", "compito", "promemoria", "task", "to do", "todo", "reminder" },
        };

        // tipo di attività attivo dal nome detto (anche con un sinonimo)
        public static Crm_ParModel? TrovaTipo(IEnumerable<Crm_ParModel> tipi, string? nome)
        {
            var attivi = tipi.Where(t => t.Attivo).ToList();
            var trovati = Trova(attivi, nome, t => t.Descrizione);
            if (trovati.Count > 0) return trovati[0];

            var n = ChatAIKeywords.Normalize(nome);
            var gruppo = _sinonimiTipo.FirstOrDefault(g => g.Any(s => ChatAIKeywords.Normalize(s) == n));
            return gruppo?.Select(s => Trova(attivi, s, t => t.Descrizione).FirstOrDefault()).FirstOrDefault(t => t != null);
        }
    }
}
