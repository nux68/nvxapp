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

        public async Task<string> CompleteJsonAsync(string systemPrompt, string userMessage, object schema, string schemaName)
        {
            var useLLM = Config("UseLLM");
            var responseFormat = _configuration[$"AI:{useLLM}:ResponseFormat"]
                                 ?? (useLLM is "AI_openrouter" or "AI_deepseek" ? "json_object" : "json_schema");
            var strictSchema = bool.TryParse(_configuration[$"AI:{useLLM}:StrictSchema"], out var strict) && strict;

            var messages = new List<OllamaChatMessage>
            {
                new() { Role = "system", Content = systemPrompt },
                new() { Role = "user", Content = userMessage }
            };

            if (useLLM == "AI_local")
            {
                var result = await _webApiService.Post<OllamaChatRequest, OllamaChatResponse>(
                    serverUrl: Config("AI_local:Url"),
                    authentication: null,
                    action: Config("AI_local:method"),
                    body: new OllamaChatRequest
                    {
                        Model = Config("AI_local:model"),
                        Messages = messages,
                        Stream = false,
                        Format = responseFormat == "json_schema" ? schema : "json"
                    },
                    bodyType: WebApiBodyType.raw,
                    headers: new Dictionary<string, string>());

                if (result?.Data == null)
                    throw new InvalidOperationException($"Ollama ha risposto con status {result?.StatusCode}: {result?.ReasonPhrase}");
                return result.Data.Message?.Content ?? string.Empty;
            }

            if (useLLM is not ("AI_groq" or "AI_openrouter" or "AI_deepseek"))
                throw new InvalidOperationException($"LLM non supportato: {useLLM}");

            var body = new Dictionary<string, object>
            {
                ["model"] = Config($"{useLLM}:model"),
                ["messages"] = messages.Select(m => new { role = m.Role, content = m.Content }).ToList(),
                ["stream"] = false,
                ["temperature"] = 0
            };
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
            var res = await _webApiService.Post<object, GroqChatResponse>(
                serverUrl: Config($"{useLLM}:Url"),
                authentication: null,
                action: Config($"{useLLM}:method"),
                body: body,
                bodyType: WebApiBodyType.raw,
                headers: headers);

            if (res?.StatusCode == HttpStatusCode.TooManyRequests)
                throw new LlmBusyException($"{useLLM}: limite di richieste o di token superato");
            if (res?.Data == null)
                throw new InvalidOperationException($"{useLLM} ha risposto con status {res?.StatusCode}: {res?.ReasonPhrase}");
            return res.Data.Choices?.FirstOrDefault()?.Message?.Content ?? string.Empty;
        }

        private string Config(string key) =>
            _configuration[$"AI:{key}"] ?? throw new InvalidOperationException($"AI:{key} non configurato");
    }

    /// <summary>il provider ha rifiutato la richiesta per il limite di richieste o di token al minuto (HTTP 429): si puo' riprovare</summary>
    public class LlmBusyException : InvalidOperationException
    {
        public LlmBusyException(string message) : base(message) { }
    }

    public interface ILlmClient
    {
        /// <summary>Invia prompt di sistema e testo al LLM e restituisce la risposta (JSON secondo <paramref name="schema"/>).</summary>
        Task<string> CompleteJsonAsync(string systemPrompt, string userMessage, object schema, string schemaName);
    }
}
