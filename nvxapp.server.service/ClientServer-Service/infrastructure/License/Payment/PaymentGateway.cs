using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License.Payment
{
    /*
     Pagamenti delle licenze (ANALISI_LICENZE.md §7).
     Il servizio licenze parla solo con IPaymentGateway: non conosce i dettagli del provider.
        - DebugPaymentGateway: giro completo senza transazione reale (§7.2), attivo con Payments:Debug = true
          (variabile d'ambiente NVXAPP_Payments__Debug) e MAI in ambiente Production (li' il flag e' ignorato)
        - carta di credito e PayPal: implementazioni di IPaymentGateway da aggiungere nella fase dei pagamenti reali
          (provider da scegliere, §13); i secret stanno in configurazione, mai nel codice.
     Ogni implementazione registrata (scanner dei servizi) e' scelta da IPaymentGatewayResolver in base al metodo.
    */
    public class PaymentRequest
    {
        public decimal Importo { get; set; }
        public string Valuta { get; set; } = "EUR";
        public MetodoPagamento Metodo { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public int IdAbbonamento { get; set; }
        /// <summary>pagina a cui il provider riporta l'utente (gateway con redirect)</summary>
        public string? ReturnUrl { get; set; }
    }

    public class PaymentResult
    {
        public StatoPagamento Stato { get; set; }
        public string? RiferimentoGateway { get; set; }
        /// <summary>pagina del provider su cui portare l'utente (pagamento ospitato dal provider); null = esito gia' noto</summary>
        public string? RedirectUrl { get; set; }
        public string? Errore { get; set; }
        public bool Debug { get; set; }
    }

    public interface IPaymentGateway
    {
        /// <summary>true = gateway di debug (nessuna transazione reale)</summary>
        bool Debug { get; }
        bool Supports(MetodoPagamento metodo);
        Task<PaymentResult> StartAsync(PaymentRequest request);
        /// <summary>esito comunicato dal provider (callback/webhook verificato), idempotente sul riferimento</summary>
        Task<PaymentResult> ConfirmAsync(string riferimentoGateway);
    }

    public class DebugPaymentGateway : IPaymentGateway
    {
        public bool Debug => true;

        public bool Supports(MetodoPagamento metodo) => metodo is MetodoPagamento.CartaCredito or MetodoPagamento.PayPal;

        public Task<PaymentResult> StartAsync(PaymentRequest request)
            => Task.FromResult(new PaymentResult
            {
                Stato = StatoPagamento.Debug,
                RiferimentoGateway = "DEBUG-" + Guid.NewGuid().ToString("N"),
                Debug = true
            });

        public Task<PaymentResult> ConfirmAsync(string riferimentoGateway)
            => Task.FromResult(new PaymentResult { Stato = StatoPagamento.Debug, RiferimentoGateway = riferimentoGateway, Debug = true });
    }

    public interface IPaymentGatewayResolver
    {
        /// <summary>modalita' debug attiva (Payments:Debug = true fuori da Production)</summary>
        bool DebugMode { get; }
        /// <summary>metodi di pagamento utilizzabili adesso</summary>
        List<MetodoPagamento> MetodiDisponibili();
        /// <summary>gateway per il metodo; null = metodo non configurato</summary>
        IPaymentGateway? Resolve(MetodoPagamento metodo);
    }

    public class PaymentGatewayResolver : IPaymentGatewayResolver
    {
        private readonly IEnumerable<IPaymentGateway> _gateways;

        public PaymentGatewayResolver(IEnumerable<IPaymentGateway> gateways, IConfiguration configuration, IHostEnvironment environment)
        {
            _gateways = gateways;
            DebugMode = !environment.IsProduction()
                        && string.Equals(configuration["Payments:Debug"], "true", StringComparison.OrdinalIgnoreCase);
        }

        public bool DebugMode { get; }

        public List<MetodoPagamento> MetodiDisponibili()
            => new[] { MetodoPagamento.CartaCredito, MetodoPagamento.PayPal }.Where(m => Resolve(m) != null).ToList();

        public IPaymentGateway? Resolve(MetodoPagamento metodo)
            => _gateways.FirstOrDefault(g => g.Debug == DebugMode && g.Supports(metodo));
    }
}
