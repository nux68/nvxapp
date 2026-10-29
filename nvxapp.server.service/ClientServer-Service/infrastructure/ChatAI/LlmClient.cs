using Microsoft.Extensions.Configuration;
using nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI.Models;
using nvxapp.server.service.Helpers;
using System.Net;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.ChatAI
{
    /*
     Richiesta singola al LLM configurato (AI:UseLLM), senza dialogo: prompt di sistema, testo
     dell'utente e schema JSON della risposta. Lo usano le funzioni che chiedono al LLM di estrarre
     dati strutturati da un testo (es. l'importazione dei dipendenti da file).
     Stessa configurazione della chat (sezione AI di appsettings): provider, modello, ApiKey,
     ResponseFormat ("json_schema", "json_object" o "none") e StrictSchema.
     Con un'immagine (es. foto di un documento) si usa il provider AI:VisionLLM (di default quello di
     AI:UseLLM: serve perche' non tutti leggono immagini, es. DeepSeek) con il suo modello con visione,
     AI:<provider>:VisionModel, e si chiede solo JSON valido (json_object): lo schema resta nel prompt.
     Senza VisionModel le immagini non si possono leggere (LlmVisionUnavailableException).
     Facoltativi: provider diverso da AI:UseLLM per una funzione (es. l'export usa AI:ExportLLM, se impostato),
     maxTokens (lunghezza massima della risposta; senza, decide il provider) e AI:TimeoutSeconds (attesa
     massima di ogni richiesta; senza, 100 secondi): oltre si ha LlmTimeoutException.
    */
    public class LlmClient : ILlmClient
    {
        private readonly IWebApiService _webApiService;
        private readonly IConfiguration _configuration;

        public LlmClient(IWebApiService webApiService, IConfiguration configuration)
        {
            _webApiService = webApiService;
            _configuration = configuration;
        }

        public async Task<string> CompleteJsonAsync(string systemPrompt, string userMessage, object schema, string schemaName, LlmImage? image = null,
                                                    string? provider = null, int? maxTokens = null)
        {
            // provider indicato da chi chiama > VisionLLM (solo con immagine) > UseLLM
            var useLLM = !string.IsNullOrWhiteSpace(provider) ? provider.Trim() : Config("UseLLM");
            if (string.IsNullOrWhiteSpace(provider) && image != null && !string.IsNullOrWhiteSpace(_configuration["AI:VisionLLM"]))
                useLLM = _configuration["AI:VisionLLM"]!.Trim();
            var timeout = int.TryParse(_configuration["AI:TimeoutSeconds"], out var secondi) && secondi > 0
                ? TimeSpan.FromSeconds(secondi)
                : (TimeSpan?)null;
            var responseFormat = _configuration[$"AI:{useLLM}:ResponseFormat"]
                                 ?? (useLLM is "AI_openrouter" or "AI_deepseek" ? "json_object" : "json_schema");
            var strictSchema = bool.TryParse(_configuration[$"AI:{useLLM}:StrictSchema"], out var strict) && strict;

            var model = Config($"{useLLM}:model");
            if (image != null)
            {
                model = _configuration[$"AI:{useLLM}:VisionModel"]?.Trim() ?? string.Empty;
                if (model.Length == 0)
                    throw new LlmVisionUnavailableException($"{useLLM}: nessun modello con visione configurato (AI:{useLLM}:VisionModel)");
                if (responseFormat == "json_schema")
                    responseFormat = "json_object";
            }

            if (useLLM == "AI_local")
            {
                var userOllama = new Dictionary<string, object> { ["role"] = "user", ["content"] = userMessage };
                if (image != null)
                    userOllama["images"] = new[] { Convert.ToBase64String(image.Data) };

                var bodyOllama = new Dictionary<string, object>
                {
                    ["model"] = model,
                    ["messages"] = new object[] { new Dictionary<string, object> { ["role"] = "system", ["content"] = systemPrompt }, userOllama },
                    ["stream"] = false,
                    ["format"] = responseFormat == "json_schema" ? schema : "json"
                };
                if (maxTokens != null)
                    bodyOllama["options"] = new Dictionary<string, object> { ["num_predict"] = maxTokens.Value };

                var result = await ConTimeout(useLLM, timeout, () => _webApiService.Post<object, OllamaChatResponse>(
                    serverUrl: Config("AI_local:Url"),
                    authentication: null,
                    action: Config("AI_local:method"),
                    body: bodyOllama,
                    bodyType: WebApiBodyType.raw,
                    headers: new Dictionary<string, string>(),
                    timeout: timeout));

                if (result?.Data == null)
                    throw new InvalidOperationException($"Ollama ha risposto con status {result?.StatusCode}: {result?.ReasonPhrase}");
                return result.Data.Message?.Content ?? string.Empty;
            }

            if (useLLM is not ("AI_groq" or "AI_openrouter" or "AI_deepseek"))
                throw new InvalidOperationException($"LLM non supportato: {useLLM}");

            // con l'immagine il messaggio dell'utente e' una lista di parti: testo e immagine (data URL)
            object userContent = image == null
                ? userMessage
                : new object[]
                {
                    new Dictionary<string, object> { ["type"] = "text", ["text"] = userMessage },
                    new Dictionary<string, object>
                    {
                        ["type"] = "image_url",
                        ["image_url"] = new Dictionary<string, object> { ["url"] = $"data:{image.MimeType};base64,{Convert.ToBase64String(image.Data)}" }
                    }
                };

            var body = new Dictionary<string, object>
            {
                ["model"] = model,
                ["messages"] = new object[]
                {
                    new Dictionary<string, object> { ["role"] = "system", ["content"] = systemPrompt },
                    new Dictionary<string, object> { ["role"] = "user", ["content"] = userContent }
                },
                ["stream"] = false,
                ["temperature"] = 0
            };
            if (maxTokens != null)
                body["max_tokens"] = maxTokens.Value;
            object? format = responseFormat switch
            {
                "json_schema" => new Dictionary<string, object>
                {
                    ["type"] = "json_schema",
                    ["json_schema"] = new Dictionary<string, object>
                    {
                        ["name"] = schemaName,
                        ["strict"] = strictSchema,
                        ["schema"] = schema
                    }
                },
                "json_object" => new Dictionary<string, object> { ["type"] = "json_object" },
                _ => null
            };
            if (format != null)
                body["response_format"] = format;

            var apiKey = _configuration[$"AI:{useLLM}:ApiKey"]?.Trim() ?? string.Empty;
            if (apiKey.Length == 0)
                throw new InvalidOperationException(
                    $"API key di {useLLM} non configurata: impostare la variabile d'ambiente NVXAPP_AI__{useLLM}__ApiKey.");

            var headers = new Dictionary<string, string> { ["Authorization"] = $"Bearer {apiKey}" };
            if (useLLM == "AI_openrouter")
            {
                headers["HTTP-Referer"] = "http://localhost";   // richiesto da OpenRouter
                headers["X-Title"] = "NvxApp-Dev";
            }

            // Groq, OpenRouter e DeepSeek rispondono nello stesso formato (API compatibili OpenAI)
            var res = await ConTimeout(useLLM, timeout, () => _webApiService.Post<object, GroqChatResponse>(
                serverUrl: Config($"{useLLM}:Url"),
                authentication: null,
                action: Config($"{useLLM}:method"),
                body: body,
                bodyType: WebApiBodyType.raw,
                headers: headers,
                timeout: timeout));

            if (res?.StatusCode == HttpStatusCode.TooManyRequests)
                throw new LlmBusyException($"{useLLM}: limite di richieste o di token superato");
            // modello con visione non piu' offerto dal provider (i provider ritirano spesso i modelli)
            if (image != null && res?.StatusCode == HttpStatusCode.NotFound)
                throw new LlmVisionUnavailableException($"{useLLM}: modello con visione {model} non trovato");
            if (res?.Data == null)
                throw new InvalidOperationException($"{useLLM} ha risposto con status {res?.StatusCode}: {res?.ReasonPhrase}");
            return res.Data.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        }

        // il provider non ha risposto entro il timeout: HttpClient annulla la richiesta (TaskCanceledException,
        // dentro un'AggregateException perche' WebApiService usa .Result)
        private static async Task<T> ConTimeout<T>(string useLLM, TimeSpan? timeout, Func<Task<T>> richiesta)
        {
            try
            {
                return await richiesta();
            }
            catch (Exception ex) when (ex is TaskCanceledException || ex is AggregateException { InnerException: TaskCanceledException })
            {
                var secondi = (int)(timeout ?? TimeSpan.FromSeconds(100)).TotalSeconds;
                throw new LlmTimeoutException($"{useLLM}: nessuna risposta entro {secondi} secondi (AI:TimeoutSeconds)", ex);
            }
        }

        private string Config(string key) =>
            _configuration[$"AI:{key}"] ?? throw new InvalidOperationException($"AI:{key} non configurato");
    }

    /// <summary>immagine da far leggere al LLM (es. "image/jpeg")</summary>
    public record LlmImage(byte[] Data, string MimeType);

    /// <summary>il provider ha rifiutato la richiesta per il limite di richieste o di token al minuto (HTTP 429): si puo' riprovare</summary>
    public class LlmBusyException : InvalidOperationException
    {
        public LlmBusyException(string message) : base(message) { }
    }

    /// <summary>immagine non leggibile: per il provider in uso non e' configurato un modello con visione</summary>
    public class LlmVisionUnavailableException : InvalidOperationException
    {
        public LlmVisionUnavailableException(string message) : base(message) { }
    }

    /// <summary>il provider non ha risposto entro AI:TimeoutSeconds: si puo' riprovare</summary>
    public class LlmTimeoutException : InvalidOperationException
    {
        public LlmTimeoutException(string message, Exception inner) : base(message, inner) { }
    }

    public interface ILlmClient
    {
        /// <summary>
        /// Invia prompt di sistema e testo (ed eventualmente un'immagine) al LLM e restituisce la risposta
        /// (JSON secondo <paramref name="schema"/>).
        /// <paramref name="provider"/>: provider da usare al posto di AI:UseLLM (es. AI_groq); <paramref name="maxTokens"/>:
        /// lunghezza massima della risposta.
        /// </summary>
        Task<string> CompleteJsonAsync(string systemPrompt, string userMessage, object schema, string schemaName, LlmImage? image = null,
                                       string? provider = null, int? maxTokens = null);
    }
}
