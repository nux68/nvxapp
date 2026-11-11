using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Digest
{
    /*
     Testo del riepilogo (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-AI.2).
        - lo scrive l'AI con il provider gia' configurato (ILlmClient, sezione AI di appsettings) a partire SOLO dai fatti
          strutturati (JSON delle sezioni): il prompt vieta di aggiungere numeri, nomi, date, cause o consigli che non ci sono
        - AI:DigestLLM: provider dedicato (vuoto = quello di AI:UseLLM; "none" = sempre senza AI); AI:DigestMaxTokens:
          lunghezza massima della risposta (vuoto = 1000)
        - se l'AI non e' configurata, non risponde o risponde vuoto: testo semplice generato dai fatti, senza AI
    */
    public class DigestWriter : IDigestWriter
    {
        public const string SenzaAI = "none";

        private static readonly JsonSerializerOptions JsonFatti = new()
        {
            PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
            DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
            Encoder = JavaScriptEncoder.UnsafeRelaxedJsonEscaping,
            Converters = { new JsonStringEnumConverter() },
        };

        private static readonly object Schema = new Dictionary<string, object>
        {
            ["type"] = "object",
            ["properties"] = new Dictionary<string, object>
            {
                ["testo"] = new Dictionary<string, object> { ["type"] = "string" }
            },
            ["required"] = new[] { "testo" },
            ["additionalProperties"] = false
        };

        private readonly ILlmClient _llmClient;
        private readonly IConfiguration _configuration;

        public DigestWriter(ILlmClient llmClient, IConfiguration configuration)
        {
            _llmClient = llmClient;
            _configuration = configuration;
        }

        /// <summary>JSON dei fatti (si salva nel riepilogo e si passa all'AI)</summary>
        public static string Serialize(DigestFacts fatti) => JsonSerializer.Serialize(fatti, JsonFatti);

        public static DigestFacts? Deserialize(string? json)
        {
            if (string.IsNullOrWhiteSpace(json))
                return null;
            try { return JsonSerializer.Deserialize<DigestFacts>(json, JsonFatti); }
            catch { return null; }
        }

        public async Task<DigestText> WriteAsync(DigestFacts fatti)
        {
            var semplice = new DigestText { Testo = TestoSemplice(fatti) };

            var provider = _configuration["AI:DigestLLM"]?.Trim();
            if (string.Equals(provider, SenzaAI, StringComparison.OrdinalIgnoreCase))
                return semplice;
            var usato = string.IsNullOrEmpty(provider) ? _configuration["AI:UseLLM"]?.Trim() : provider;
            if (string.IsNullOrEmpty(usato))
                return semplice;

            try
            {
                int maxTokens = int.TryParse(_configuration["AI:DigestMaxTokens"], out var t) && t > 0 ? t : 1000;
                var risposta = await _llmClient.CompleteJsonAsync(SystemPrompt(fatti), Serialize(fatti), Schema, "riepilogo",
                                                                  provider: string.IsNullOrEmpty(provider) ? null : provider,
                                                                  maxTokens: maxTokens);
                var testo = LeggiTesto(risposta);
                if (string.IsNullOrWhiteSpace(testo))
                {
                    Log.Warning("Riepilogo: risposta vuota dell'AI ({Provider}), si usa il testo senza AI", usato);
                    return semplice;
                }
                return new DigestText { Testo = testo.Trim(), GeneratoConAI = true, ProviderAI = usato };
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Riepilogo: AI non disponibile ({Provider}), si usa il testo senza AI", usato);
                return semplice;
            }
        }

        // ---------------------------------------------------------------------------

        private static string SystemPrompt(DigestFacts fatti)
        {
            string lingua = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en" ? "inglese" : "italiano";
            string destinatario = fatti.Aziende != null
                ? "lo studio professionale che segue le aziende elencate (sono riportate solo le aziende con criticita')"
                : "un amministratore dell'azienda";
            return $"""
                Scrivi il riepilogo settimanale di un gestionale aziendale per {destinatario}.
                Ricevi un JSON con i fatti del periodo {fatti.Dal:dd/MM/yyyy} - {fatti.Al:dd/MM/yyyy}, divisi in sezioni
                (sezioni.righe: testo, numero, critico, dettagli){(fatti.Aziende != null ? ", raggruppate per azienda" : "")}.
                Regole obbligatorie:
                - usa SOLO i fatti del JSON: non inventare numeri, nomi, date, cause, conseguenze, scadenze, persone o consigli che non vi compaiono;
                - non cambiare i numeri e non fare calcoli nuovi;
                - metti per prime le criticita' (critica o critico = true), poi il resto;
                - puoi citare gli oggetti dei dettagli cosi' come sono scritti;
                - niente saluti, firme, titoli, markdown o simboli di elenco: testo semplice in paragrafi brevi;
                - al massimo 180 parole, in {lingua}.
                Rispondi solo con un oggetto JSON: {"{"}"testo": "<riepilogo>"{"}"}
                """;
        }

        // la risposta dovrebbe essere {"testo": "..."}; alcuni modelli aggiungono testo o ``` intorno al JSON
        private static string? LeggiTesto(string? risposta)
        {
            if (string.IsNullOrWhiteSpace(risposta))
                return null;
            int inizio = risposta.IndexOf('{');
            int fine = risposta.LastIndexOf('}');
            if (inizio < 0 || fine <= inizio)
                return null;
            try
            {
                using var doc = JsonDocument.Parse(risposta[inizio..(fine + 1)]);
                return doc.RootElement.ValueKind == JsonValueKind.Object
                       && doc.RootElement.TryGetProperty("testo", out var testo)
                       && testo.ValueKind == JsonValueKind.String
                    ? testo.GetString()
                    : null;
            }
            catch (JsonException)
            {
                return null;
            }
        }

        /// <summary>Testo senza AI: le sezioni con i loro fatti, cosi' come li forniscono i moduli.</summary>
        public static string TestoSemplice(DigestFacts fatti)
        {
            var sb = new StringBuilder();
            if (fatti.Aziende != null)
            {
                sb.AppendLine(InfrastructureMessages.Get("DigestStudioCompanies", fatti.Aziende.Count));
                foreach (var azienda in fatti.Aziende)
                {
                    sb.AppendLine();
                    sb.AppendLine(azienda.Azienda);
                    Sezioni(sb, azienda.Sezioni);
                }
            }
            else
                Sezioni(sb, fatti.Sezioni ?? new List<DigestSection>());
            return sb.ToString().TrimEnd();
        }

        private static void Sezioni(StringBuilder sb, List<DigestSection> sezioni)
        {
            bool prima = true;
            foreach (var s in sezioni)
            {
                if (!prima)
                    sb.AppendLine();
                prima = false;
                sb.AppendLine(s.Critica ? InfrastructureMessages.Get("DigestCriticalSection", s.Titolo) : s.Titolo);
                foreach (var r in s.Righe)
                {
                    sb.AppendLine("- " + r.Testo);
                    foreach (var d in r.Dettagli)
                        sb.AppendLine("    " + d);
                }
            }
        }
    }

    /// <summary>Fatti del riepilogo: sezioni dell'azienda oppure, per lo studio, aziende con criticita' e le loro sezioni.</summary>
    public class DigestFacts
    {
        public DateTime Dal { get; set; }
        public DateTime Al { get; set; }

        /// <summary>azienda (riepilogo dell'azienda) o studio</summary>
        public string Soggetto { get; set; } = string.Empty;

        /// <summary>riepilogo dell'azienda</summary>
        public List<DigestSection>? Sezioni { get; set; }

        /// <summary>riepilogo dello studio</summary>
        public List<DigestCompanyFacts>? Aziende { get; set; }
    }

    public class DigestCompanyFacts
    {
        public int IdCompany { get; set; }
        public string Azienda { get; set; } = string.Empty;
        public List<DigestSection> Sezioni { get; set; } = new List<DigestSection>();
    }

    public class DigestText
    {
        public string Testo { get; set; } = string.Empty;
        public bool GeneratoConAI { get; set; }
        public string? ProviderAI { get; set; }
    }

    public interface IDigestWriter
    {
        /// <summary>testo del riepilogo: dall'AI se configurata e disponibile, altrimenti generato dai fatti</summary>
        Task<DigestText> WriteAsync(DigestFacts fatti);
    }
}
