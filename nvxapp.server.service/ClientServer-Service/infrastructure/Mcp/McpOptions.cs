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
            // il livello 3 (modifiche amministrative) non si puo' ancora attivare: richiede la conferma in due passi
            // lato server (analisi, punto MCP-40). Fino ad allora il massimo e' il livello 2.
            return (McpAccessLevel)Math.Clamp(level, (int)McpAccessLevel.Read, (int)McpAccessLevel.PersonalWrite);
        }

        public static int TokenMaxDays(IConfiguration configuration) =>
            int.TryParse(configuration["Mcp:TokenMaxDays"], out var days) && days > 0 ? days : 365;

        public static int MaxCallsPerMinute(IConfiguration configuration) => NonNegative(configuration["Mcp:MaxCallsPerMinute"], 30);

        public static int MaxCallsPerDay(IConfiguration configuration) => NonNegative(configuration["Mcp:MaxCallsPerDay"], 1000);

        public static int MaxRows(IConfiguration configuration) =>
            int.TryParse(configuration["Mcp:MaxRows"], out var rows) && rows > 0 ? rows : 200;

        // valore assente o non valido = predefinito; 0 = nessun limite
        private static int NonNegative(string? value, int fallback) =>
            int.TryParse(value, out var number) && number >= 0 ? number : fallback;
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
