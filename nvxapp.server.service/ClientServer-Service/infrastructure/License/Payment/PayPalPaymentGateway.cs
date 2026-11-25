using System.Collections.Concurrent;
using System.Globalization;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;
using System.Text.Json.Nodes;
using Microsoft.Extensions.Configuration;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.service.Helpers;
using Serilog;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License.Payment
{
    /*
     PayPal (ANALISI_INFRASTRUCTURE_LICENZE.md §7.4): API REST Orders v2 con pagamento ospitato da PayPal.
        - configurazione Payments:PayPal (Mode = sandbox | live, ClientId, Secret, BrandName); senza ClientId o Secret il
          gateway non supporta nessun metodo e il resolver ripiega sul debug (se attivo)
        - StartAsync: crea l'ordine (intent CAPTURE, importo calcolato dal server) e restituisce il link di approvazione
          (RedirectUrl); il riferimento e' l'id dell'ordine. PayPal riporta l'utente a ReturnUrl con
          "pagamento=ok&token=<id ordine>&PayerID=..." oppure, se annulla, con "pagamento=annullato&token=<id ordine>"
        - ConfirmAsync: legge l'ordine; se approvato lo incassa (capture). Idempotente: l'ordine gia' incassato risponde
          Completato e la capture ha la PayPal-Request-Id fissa sull'ordine (una seconda chiamata non addebita di nuovo)
        - nessun rinnovo automatico: si rinnova dalla pagina (le subscription PayPal non sono in questa fase)
     Il token OAuth si tiene in memoria fino alla scadenza (per ambiente e ClientId).
    */
    public class PayPalPaymentGateway : IPaymentGateway
    {
        private const string HttpClientName = "PayPal";
        private static readonly ConcurrentDictionary<string, (string Token, DateTime Scadenza)> _token = new();

        private readonly IHttpClientFactory _httpClientFactory;
        private readonly string? _clientId;
        private readonly string? _secret;
        private readonly string _brandName;
        private readonly bool _live;

        public PayPalPaymentGateway(IHttpClientFactory httpClientFactory, IConfiguration configuration)
        {
            _httpClientFactory = httpClientFactory;
            var section = configuration.GetSection("Payments:PayPal");
            _clientId = section["ClientId"];
            _secret = section["Secret"];
            _brandName = string.IsNullOrWhiteSpace(section["BrandName"]) ? "nvxapp" : section["BrandName"]!;
            _live = string.Equals(section["Mode"], "live", StringComparison.OrdinalIgnoreCase);
        }

        private bool Configurato => !string.IsNullOrWhiteSpace(_clientId) && !string.IsNullOrWhiteSpace(_secret);

        private string BaseUrl => _live ? "https://api-m.paypal.com" : "https://api-m.sandbox.paypal.com";

        public bool Debug => false;

        public bool Sandbox => !_live;

        public bool Supports(MetodoPagamento metodo) => metodo == MetodoPagamento.PayPal && Configurato;

        public async Task<PaymentResult> StartAsync(PaymentRequest request)
        {
            if (string.IsNullOrWhiteSpace(request.ReturnUrl))
                return new PaymentResult { Stato = StatoPagamento.Fallito, Errore = "Indirizzo di ritorno mancante." };

            var descrizione = request.Descrizione.Length > 127 ? request.Descrizione[..127] : request.Descrizione;
            var body = new JsonObject
            {
                ["intent"] = "CAPTURE",
                ["purchase_units"] = new JsonArray
                {
                    new JsonObject
                    {
                        ["reference_id"] = "LIC-" + request.IdAbbonamento,
                        ["custom_id"] = request.IdAbbonamento.ToString(CultureInfo.InvariantCulture),
                        ["description"] = descrizione,
                        ["amount"] = new JsonObject
                        {
                            ["currency_code"] = request.Valuta,
                            ["value"] = request.Importo.ToString("0.00", CultureInfo.InvariantCulture)
                        }
                    }
                },
                ["payment_source"] = new JsonObject
                {
                    ["paypal"] = new JsonObject
                    {
                        ["experience_context"] = new JsonObject
                        {
                            ["brand_name"] = _brandName,
                            ["user_action"] = "PAY_NOW",
                            ["shipping_preference"] = "NO_SHIPPING",
                            ["return_url"] = ConParametro(request.ReturnUrl, "pagamento=ok"),
                            ["cancel_url"] = ConParametro(request.ReturnUrl, "pagamento=annullato")
                        }
                    }
                }
            };

            var (status, json) = await SendAsync(HttpMethod.Post, "/v2/checkout/orders", body, Guid.NewGuid().ToString("N"));
            if (status is not (HttpStatusCode.OK or HttpStatusCode.Created) || json == null)
                return Errore("creazione dell'ordine", status, json);

            var id = json["id"]?.GetValue<string>();
            var link = (json["links"] as JsonArray)?
                .FirstOrDefault(l => l?["rel"]?.GetValue<string>() is "payer-action" or "approve")?["href"]?.GetValue<string>();
            if (id == null || link == null)
                return Errore("creazione dell'ordine (risposta senza id o link)", status, json);

            return new PaymentResult { Stato = StatoPagamento.InAttesa, RiferimentoGateway = id, RedirectUrl = link };
        }

        public async Task<PaymentResult> ConfirmAsync(string riferimentoGateway)
        {
            var id = Uri.EscapeDataString(riferimentoGateway);
            var (status, ordine) = await SendAsync(HttpMethod.Get, $"/v2/checkout/orders/{id}", null, null);
            if (status != HttpStatusCode.OK || ordine == null)
                return Errore("lettura dell'ordine", status, ordine);

            switch (ordine["status"]?.GetValue<string>())
            {
                case "COMPLETED":
                    return EsitoCapture(riferimentoGateway, ordine);
                case "APPROVED":
                    // PayPal-Request-Id fissa sull'ordine: una capture ripetuta restituisce lo stesso esito senza addebitare di nuovo
                    var (cs, capture) = await SendAsync(HttpMethod.Post, $"/v2/checkout/orders/{id}/capture", new JsonObject(), "capture-" + riferimentoGateway);
                    if ((cs is HttpStatusCode.OK or HttpStatusCode.Created) && capture != null)
                        return EsitoCapture(riferimentoGateway, capture);
                    if (Issue(capture) == "ORDER_ALREADY_CAPTURED")
                        return await ConfirmAsync(riferimentoGateway);
                    if (Issue(capture) == "INSTRUMENT_DECLINED")
                        return new PaymentResult { Stato = StatoPagamento.Fallito, RiferimentoGateway = riferimentoGateway, Errore = "Metodo di pagamento rifiutato da PayPal." };
                    return Errore("incasso dell'ordine", cs, capture, riferimentoGateway);
                case "VOIDED":
                    return new PaymentResult { Stato = StatoPagamento.Fallito, RiferimentoGateway = riferimentoGateway, Errore = "Ordine PayPal annullato." };
                default:
                    // CREATED, SAVED, PAYER_ACTION_REQUIRED: l'utente non ha ancora approvato
                    return new PaymentResult { Stato = StatoPagamento.InAttesa, RiferimentoGateway = riferimentoGateway };
            }
        }

        // ---------------------------------------------------------------------------

        // esito della capture (dentro l'ordine o nella risposta della capture): COMPLETED, PENDING (es. verifica), DECLINED/FAILED
        private static PaymentResult EsitoCapture(string riferimento, JsonNode ordine)
        {
            var capture = (ordine["purchase_units"]?[0]?["payments"]?["captures"] as JsonArray)?.FirstOrDefault();
            var stato = capture?["status"]?.GetValue<string>() ?? ordine["status"]?.GetValue<string>();
            return stato switch
            {
                "COMPLETED" => new PaymentResult { Stato = StatoPagamento.Completato, RiferimentoGateway = riferimento },
                "PENDING" => new PaymentResult { Stato = StatoPagamento.InAttesa, RiferimentoGateway = riferimento,
                                                 Errore = "Incasso PayPal in verifica: " + capture?["status_details"]?["reason"]?.GetValue<string>() },
                _ => new PaymentResult { Stato = StatoPagamento.Fallito, RiferimentoGateway = riferimento, Errore = "Incasso PayPal non riuscito (" + stato + ")." }
            };
        }

        private static string ConParametro(string url, string parametro)
            => url + (url.Contains('?') ? "&" : "?") + parametro;

        private static string? Issue(JsonNode? errore)
            => (errore?["details"] as JsonArray)?.FirstOrDefault()?["issue"]?.GetValue<string>();

        private static PaymentResult Errore(string operazione, HttpStatusCode status, JsonNode? risposta, string? riferimento = null)
        {
            var dettaglio = Issue(risposta) ?? risposta?["message"]?.GetValue<string>() ?? risposta?["error_description"]?.GetValue<string>();
            Log.Error("PayPal: {Operazione} non riuscita ({Status}): {Risposta}", operazione, (int)status, LogSanitizer.MaskText(risposta?.ToJsonString()));
            return new PaymentResult
            {
                Stato = StatoPagamento.Fallito,
                RiferimentoGateway = riferimento,
                Errore = $"PayPal: {operazione} non riuscita ({(int)status}{(dettaglio == null ? "" : ", " + dettaglio)})."
            };
        }

        private async Task<(HttpStatusCode Status, JsonNode? Json)> SendAsync(HttpMethod method, string path, JsonNode? body, string? requestId)
        {
            var client = _httpClientFactory.CreateClient(HttpClientName);
            using var request = new HttpRequestMessage(method, BaseUrl + path);
            request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", await TokenAsync(client));
            request.Headers.Add("Prefer", "return=representation");
            if (requestId != null)
                request.Headers.Add("PayPal-Request-Id", requestId);
            if (body != null)
                request.Content = new StringContent(body.ToJsonString(), Encoding.UTF8, "application/json");

            using var response = await client.SendAsync(request);
            var testo = await response.Content.ReadAsStringAsync();
            JsonNode? json = null;
            if (!string.IsNullOrWhiteSpace(testo))
            {
                try { json = JsonNode.Parse(testo); }
                catch (JsonException) { json = new JsonObject { ["message"] = testo.Length > 300 ? testo[..300] : testo }; }
            }
            return (response.StatusCode, json);
        }

        private async Task<string> TokenAsync(HttpClient client)
        {
            var chiave = BaseUrl + "|" + _clientId;
            if (_token.TryGetValue(chiave, out var t) && t.Scadenza > DateTime.UtcNow)
                return t.Token;

            using var request = new HttpRequestMessage(HttpMethod.Post, BaseUrl + "/v1/oauth2/token");
            request.Headers.Authorization = new AuthenticationHeaderValue("Basic", Convert.ToBase64String(Encoding.UTF8.GetBytes(_clientId + ":" + _secret)));
            request.Content = new FormUrlEncodedContent(new Dictionary<string, string> { ["grant_type"] = "client_credentials" });
            using var response = await client.SendAsync(request);
            var testo = await response.Content.ReadAsStringAsync();
            if (!response.IsSuccessStatusCode)
            {
                Log.Error("PayPal: autenticazione non riuscita ({Status}): {Risposta}", (int)response.StatusCode, LogSanitizer.MaskText(testo));
                throw new Exception($"PayPal: autenticazione non riuscita ({(int)response.StatusCode}). Controllare Payments:PayPal (ClientId, Secret, Mode).");
            }

            var json = JsonNode.Parse(testo)!;
            var token = json["access_token"]!.GetValue<string>();
            var durata = json["expires_in"]?.GetValue<int>() ?? 300;
            // margine di un minuto sulla scadenza
            _token[chiave] = (token, DateTime.UtcNow.AddSeconds(Math.Max(60, durata - 60)));
            return token;
        }
    }
}
