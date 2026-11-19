using System.Security.Cryptography;
using System.Text;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Mcp
{
    /*
     Impostazioni dell'installazione per l'accesso AI tramite MCP (ANALISI_INFRASTRUCTURE_MCP.md, sezione 8.6), sezione "Mcp":
        - Enabled      : endpoint MCP presente (false = nessun endpoint, i token non servono a nulla)
        - MaxLevel     : livello massimo degli strumenti che un'azienda puo' scegliere (1 lettura, 2 modifiche personali,
                         3 modifiche amministrative). Fase 2 dell'analisi: 2.
        - TokenMaxDays : durata massima di un token personale
        - MaxCallsPerMinute / MaxCallsPerDay : chiamate agli strumenti ammesse per token (0 = nessun limite), MCP-21
        - MaxRows      : righe massime di ogni elenco restituito all'AI, MCP-21
    */
    public static class McpOptions
    {
        /// <summary>percorso dell'endpoint MCP sul sito dell'applicazione</summary>
        public const string EndpointPath = "/mcp";

        public static bool Enabled(IConfiguration configuration) =>
            bool.TryParse(configuration["Mcp:Enabled"], out var enabled) && enabled;

        public static McpAccessLevel MaxLevel(IConfiguration configuration)
        {
            int level = int.TryParse(configuration["Mcp:MaxLevel"], out var value) ? value : (int)McpAccessLevel.Read;
            // il livello 3 (modifiche amministrative) e' attivabile dalla fase 4: gli strumenti di quel livello chiedono
            // sempre la conferma in due passi lato server (McpConfirmation, punto MCP-40)
            return (McpAccessLevel)Math.Clamp(level, (int)McpAccessLevel.Read, (int)McpAccessLevel.AdminWrite);
        }

        public static int TokenMaxDays(IConfiguration configuration) =>
            int.TryParse(configuration["Mcp:TokenMaxDays"], out var days) && days > 0 ? days : 365;

        public static int MaxCallsPerMinute(IConfiguration configuration) => NonNegative(configuration["Mcp:MaxCallsPerMinute"], 30);

        public static int MaxCallsPerDay(IConfiguration configuration) => NonNegative(configuration["Mcp:MaxCallsPerDay"], 1000);

        public static int MaxRows(IConfiguration configuration) =>
            int.TryParse(configuration["Mcp:MaxRows"], out var rows) && rows > 0 ? rows : 200;

        /// <summary>minuti di validita' del codice di conferma delle modifiche amministrative (MCP-40)</summary>
        public static int ConfirmMinutes(IConfiguration configuration) => Positive(configuration["Mcp:ConfirmMinutes"], 5);

        /// <summary>giorni massimi del periodo su cui puo' agire una modifica amministrativa in una chiamata (MCP-41)</summary>
        public static int MaxWriteDays(IConfiguration configuration) => Positive(configuration["Mcp:MaxWriteDays"], 31);

        /// <summary>ore dopo le quali un'impersonificazione fatta con "lavora come" torna da sola all'identita' di partenza (fase 6)</summary>
        public static int ImpersonationHours(IConfiguration configuration) => Positive(configuration["Mcp:ImpersonationHours"], 8);

        private static int Positive(string? value, int fallback) =>
            int.TryParse(value, out var number) && number > 0 ? number : fallback;

        // valore assente o non valido = predefinito; 0 = nessun limite
        private static int NonNegative(string? value, int fallback) =>
            int.TryParse(value, out var number) && number >= 0 ? number : fallback;
    }

    /*
     Conferma in due passi delle modifiche amministrative (ANALISI_INFRASTRUCTURE_MCP.md, sezione 8.1, MCP-40).
     La prima chiamata di uno strumento di livello 3 NON esegue: restituisce il riepilogo e un codice. La seconda chiamata,
     con gli STESSI parametri e il codice, esegue. Il server non si fida del client: senza codice valido non scrive nulla.

     Il codice non si salva: e' "<scadenza>.<firma>", dove la firma (HMAC-SHA256 con la chiave dell'installazione) copre
     scadenza, token, utente di lavoro, strumento e parametri. Vale quindi solo per quella operazione, per pochi minuti
     (Mcp:ConfirmMinutes) e funziona anche con piu' istanze del server. L'uso unico lo verifica McpToolService sul
     registro delle chiamate.
    */
    public static class McpConfirmation
    {
        public enum Esito { Valido, NonValido, Scaduto }

        /// <summary>impronta dell'operazione: token, utente di lavoro, strumento e parametri (ordinati, senza il codice)</summary>
        public static string Fingerprint(int idToken, string userId, string tool, IEnumerable<KeyValuePair<string, string?>> arguments)
        {
            var testo = new StringBuilder().Append(idToken).Append('|').Append(userId).Append('|').Append(tool);
            foreach (var a in arguments.Where(a => !string.IsNullOrWhiteSpace(a.Value))
                                       .OrderBy(a => a.Key.ToLowerInvariant(), StringComparer.Ordinal))
                testo.Append('|').Append(a.Key.ToLowerInvariant()).Append('=').Append(a.Value!.Trim());
            return testo.ToString();
        }

        public static string NewCode(string key, string fingerprint, DateTime expiresUtc)
        {
            string scadenza = new DateTimeOffset(expiresUtc, TimeSpan.Zero).ToUnixTimeSeconds().ToString(System.Globalization.CultureInfo.InvariantCulture);
            return scadenza + "." + Firma(key, scadenza, fingerprint);
        }

        public static Esito Check(string key, string fingerprint, string? code, DateTime nowUtc)
        {
            var parti = (code ?? string.Empty).Trim().Split('.');
            if (parti.Length != 2 || !long.TryParse(parti[0], out var secondi))
                return Esito.NonValido;
            var attesa = Encoding.ASCII.GetBytes(Firma(key, parti[0], fingerprint));
            var ricevuta = Encoding.ASCII.GetBytes(parti[1]);
            if (!CryptographicOperations.FixedTimeEquals(attesa, ricevuta))
                return Esito.NonValido;
            return DateTimeOffset.FromUnixTimeSeconds(secondi).UtcDateTime < nowUtc ? Esito.Scaduto : Esito.Valido;
        }

        private static string Firma(string key, string scadenza, string fingerprint)
        {
            using var hmac = new HMACSHA256(Encoding.UTF8.GetBytes("mcp-confirm:" + key));
            var firma = hmac.ComputeHash(Encoding.UTF8.GetBytes(scadenza + "|" + fingerprint));
            return Convert.ToBase64String(firma, 0, 15).Replace('+', '-').Replace('/', '_');
        }
    }

    /*
     Provenienza "AI" delle modifiche (ANALISI_INFRASTRUCTURE_MCP.md, MCP-22, decisione MCP-D11 = nota nelle richieste):
     le richieste da approvare create da un assistente AI hanno nel corpo JSON (Dip_GG_Richiesta.Dati) il campo
     RequestBodyKey = true; le pagine delle richieste aggiungono al dettaglio il testo RequestViaAi.
    */
    public static class McpProvenance
    {
        public const string RequestBodyKey = "viaAi";

        /// <summary>vero se il corpo JSON di una richiesta la segna come creata da un assistente AI</summary>
        public static bool IsViaAi(Newtonsoft.Json.Linq.JObject? body) =>
            body?.GetValue(RequestBodyKey, StringComparison.OrdinalIgnoreCase) is Newtonsoft.Json.Linq.JToken v
            && v.Type == Newtonsoft.Json.Linq.JTokenType.Boolean && (bool)v;
    }

    /*
     Token personale MCP: "nvx_" + 32 byte casuali in base64url (47 caratteri). Nel database resta solo l'impronta
     SHA-256: chi legge la tabella non puo' usare i token. Il prefisso serve a riconoscere il token nell'elenco.
    */
    public static class McpTokenValue
    {
        public const string Prefix = "nvx_";
        private const int PrefixLength = 10;

        public static string New() =>
            Prefix + Convert.ToBase64String(RandomNumberGenerator.GetBytes(32)).TrimEnd('=').Replace('+', '-').Replace('/', '_');

        public static string Hash(string token) =>
            Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(token))).ToLowerInvariant();

        public static string DisplayPrefix(string token) =>
            token.Length <= PrefixLength ? token : token[..PrefixLength];

        /// <summary>vero se il testo ha la forma di un token personale (evita la ricerca nel database per i valori evidentemente estranei)</summary>
        public static bool LooksValid(string? token) =>
            !string.IsNullOrEmpty(token) && token.Length is > 20 and <= 80 && token.StartsWith(Prefix, StringComparison.Ordinal);
    }
}
