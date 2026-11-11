using System.Globalization;
using System.Text;
using System.Text.Encodings.Web;
using System.Text.Json;
using System.Text.Json.Serialization;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Localization;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.GestionePresenze._utility
{
    /*
     Spiegazione delle anomalie (ANALISI_ATTENDANCETRACKING_ANOMALIE_AI.md §3.1).
        - i fatti li calcolano le regole deterministiche (GestionePresenzeAnomalie): l'AI li riassume SOLO a partire dal JSON,
          con un prompt che vieta di aggiungere numeri, nomi, date, cause o consigli che non ci sono (come DigestWriter)
        - AI:AnomalieLLM: provider dedicato (vuoto = AI:UseLLM; "none" = sempre senza AI); AI:AnomalieMaxTokens (vuoto = 800)
        - ripiego senza AI (non configurata, errore, risposta vuota): testo semplice generato dai fatti
    */
    public class GestionePresenzeAnomalieWriter : IGestionePresenzeAnomalieWriter
    {
        public const string SenzaAI = "none";
        private const int MaxGiornate = 40;

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

        public GestionePresenzeAnomalieWriter(ILlmClient llmClient, IConfiguration configuration)
        {
            _llmClient = llmClient;
            _configuration = configuration;
        }

        public async Task<AnomalieTesto> WriteAsync(AnomalieRisultato risultato)
        {
            var semplice = new AnomalieTesto { Testo = TestoSemplice(risultato) };
            if (risultato.Giornate.Count == 0 && risultato.RitardiRicorrenti.Count == 0)
                return semplice;

            var provider = _configuration["AI:AnomalieLLM"]?.Trim();
            if (string.Equals(provider, SenzaAI, StringComparison.OrdinalIgnoreCase))
                return semplice;
            var usato = string.IsNullOrEmpty(provider) ? _configuration["AI:UseLLM"]?.Trim() : provider;
            if (string.IsNullOrEmpty(usato))
                return semplice;

            try
            {
                int maxTokens = int.TryParse(_configuration["AI:AnomalieMaxTokens"], out var t) && t > 0 ? t : 800;
                var risposta = await _llmClient.CompleteJsonAsync(SystemPrompt(risultato), JsonSerializer.Serialize(Fatti(risultato), JsonFatti),
                                                                  Schema, "anomalie",
                                                                  provider: string.IsNullOrEmpty(provider) ? null : provider,
                                                                  maxTokens: maxTokens);
                var testo = LeggiTesto(risposta);
                if (string.IsNullOrWhiteSpace(testo))
                {
                    Log.Warning("Anomalie presenze: risposta vuota dell'AI ({Provider}), si usa il testo senza AI", usato);
                    return semplice;
                }
                return new AnomalieTesto { Testo = testo.Trim(), GeneratoConAI = true, ProviderAI = usato };
            }
            catch (Exception ex)
            {
                Log.Warning(ex, "Anomalie presenze: AI non disponibile ({Provider}), si usa il testo senza AI", usato);
                return semplice;
            }
        }

        /// <summary>fatti passati all'AI: conteggi, giornate (al massimo 40) e ritardi ricorrenti</summary>
        private static object Fatti(AnomalieRisultato r) => new
        {
            periodo = $"{r.Dal:dd/MM/yyyy} - {r.Al:dd/MM/yyyy}",
            dipendentiAnalizzati = r.DipendentiAnalizzati,
            giornateAnalizzate = r.GiornateAnalizzate,
            giornateDaSistemare = r.GiornateDaSistemare,
            giornateDaVerificare = r.GiornateDaVerificare,
            conteggi = r.Conteggi.Select(c => new { tipo = c.Descrizione, gravita = c.Gravita.ToString(), numero = c.Numero }),
            giornate = r.Giornate.OrderByDescending(g => g.DaSistemare).ThenBy(g => g.Data).Take(MaxGiornate).Select(g => new
            {
                data = g.Data.ToString("dd/MM/yyyy"),
                dipendente = g.NomeDipendente,
                daSistemare = g.DaSistemare,
                timbrature = g.Timbrature,
                anomalie = g.Anomalie.Select(a => a.Descrizione)
            }),
            giornateNonElencate = Math.Max(0, r.Giornate.Count - MaxGiornate),
            ritardiRicorrenti = r.RitardiRicorrenti.Select(x => new { dipendente = x.NomeDipendente, ritardi = x.Ritardi, ritardoMedioMinuti = x.RitardoMedioMin }),
        };

        private static string SystemPrompt(AnomalieRisultato r)
        {
            string lingua = CultureInfo.CurrentUICulture.TwoLetterISOLanguageName == "en" ? "inglese" : "italiano";
            return $"""
                Sei l'assistente dell'ufficio del personale di un'azienda. Ricevi un JSON con le anomalie delle timbrature del periodo
                {r.Dal:dd/MM/yyyy} - {r.Al:dd/MM/yyyy}, gia' calcolate da regole fisse: conteggi per tipo, giornate (daSistemare = true
                blocca l'export verso le paghe, false = solo da verificare) e ritardi ricorrenti.
                Scrivi una spiegazione breve per chi deve sistemare le giornate prima dell'export paghe.
                Regole obbligatorie:
                - usa SOLO i fatti del JSON: non inventare numeri, nomi, date, orari, cause o conseguenze che non vi compaiono;
                - non cambiare i numeri e non fare calcoli nuovi;
                - prima le giornate da sistemare (raggruppate per tipo o per dipendente), poi gli avvisi e i ritardi ricorrenti;
                - puoi suggerire solo azioni ovvie legate al tipo di anomalia (aggiungere la timbratura mancante, inserire il
                  giustificativo, approvare le richieste in attesa, ricalcolare la giornata), senza attribuire colpe;
                - niente saluti, titoli, markdown o simboli di elenco: testo semplice in paragrafi brevi;
                - al massimo 160 parole, in {lingua}.
                Rispondi solo con un oggetto JSON: {"{"}"testo": "<spiegazione>"{"}"}
                """;
        }

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

        /// <summary>testo senza AI: totali, conteggi per tipo, ritardi ricorrenti</summary>
        public static string TestoSemplice(AnomalieRisultato r)
        {
            if (r.Giornate.Count == 0 && r.RitardiRicorrenti.Count == 0)
                return AttendanceTrackingMessages.Get("Anomalie_Nessuna", r.Dal.ToString("dd/MM/yyyy"), r.Al.ToString("dd/MM/yyyy"));

            var sb = new StringBuilder();
            sb.AppendLine(AttendanceTrackingMessages.Get("Anomalie_Riepilogo", r.Dal.ToString("dd/MM/yyyy"), r.Al.ToString("dd/MM/yyyy"),
                                                         r.GiornateDaSistemare, r.GiornateDaVerificare, r.DipendentiCoinvolti));
            foreach (var c in r.Conteggi)
                sb.AppendLine("- " + AttendanceTrackingMessages.Get(c.Gravita == AnomaliaGravita.Errore ? "Anomalie_ConteggioErrore" : "Anomalie_ConteggioAvviso", c.Descrizione, c.Numero));
            foreach (var x in r.RitardiRicorrenti)
                sb.AppendLine("- " + AttendanceTrackingMessages.Get("Anomalie_RitardiRicorrenti", x.NomeDipendente, x.Ritardi, x.RitardoMedioMin));
            return sb.ToString().TrimEnd();
        }
    }

    public class AnomalieTesto
    {
        public string Testo { get; set; } = string.Empty;
        public bool GeneratoConAI { get; set; }
        public string? ProviderAI { get; set; }
    }

    public interface IGestionePresenzeAnomalieWriter
    {
        Task<AnomalieTesto> WriteAsync(AnomalieRisultato risultato);
    }
}
