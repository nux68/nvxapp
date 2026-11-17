using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Hosting;
using nvxapp.server.data.Entities.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License.Payment
{
    /*
     Pagamenti delle licenze (ANALISI_INFRASTRUCTURE_LICENZE.md §7).
     Il servizio licenze parla solo con IPaymentGateway: non conosce i dettagli del provider.
        - DebugPaymentGateway: giro completo senza transazione reale (§7.2), attivo con Payments:Debug = true
          (appsettings.json, o variabile d'ambiente NVXAPP_Payments__Debug). Per ora e' acceso ovunque, anche in
          Production (deploy Docker), finche' non ci sono pagamenti reali: in Production lo si segnala nel log
        - PayPalPaymentGateway: PayPal reale (REST Orders v2, sandbox o live), attivo quando Payments:PayPal ha ClientId e
          Secret (§7.4); i secret stanno in configurazione (user-secrets o NVXAPP_Payments__PayPal__*), mai nel codice
        - carta di credito: implementazione reale da aggiungere (provider da scegliere, §13).
     Ogni implementazione registrata (scanner dei servizi) e' scelta da IPaymentGatewayResolver in base al metodo:
     un gateway reale configurato per il metodo vince sempre; altrimenti, con Payments:Debug = true, quello di debug.
     Cosi' PayPal sandbox convive con la carta ancora simulata.
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
        /// <summary>true = ambiente di prova del provider (es. PayPal sandbox): giro reale, denaro finto</summary>
        bool Sandbox { get; }
        bool Supports(MetodoPagamento metodo);
        Task<PaymentResult> StartAsync(PaymentRequest request);
        /// <summary>esito comunicato dal provider (callback/webhook verificato), idempotente sul riferimento</summary>
        Task<PaymentResult> ConfirmAsync(string riferimentoGateway);
    }

    public class DebugPaymentGateway : IPaymentGateway
    {
        public bool Debug => true;
        public bool Sandbox => false;

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
        /// <summary>modalita' debug attiva (Payments:Debug = true)</summary>
        bool DebugMode { get; }
        /// <summary>metodi che adesso si completano senza provider (gateway di debug)</summary>
        List<MetodoPagamento> MetodiSimulati();
        /// <summary>metodi che adesso passano dall'ambiente di prova del provider (sandbox)</summary>
        List<MetodoPagamento> MetodiSandbox();
        /// <summary>metodi di pagamento utilizzabili adesso</summary>
        List<MetodoPagamento> MetodiDisponibili();
        /// <summary>gateway per il metodo; null = metodo non configurato</summary>
        IPaymentGateway? Resolve(MetodoPagamento metodo);
    }

    public class PaymentGatewayResolver : IPaymentGatewayResolver
    {
        private static int _avvisoProduction;
        private readonly IEnumerable<IPaymentGateway> _gateways;

        public PaymentGatewayResolver(IEnumerable<IPaymentGateway> gateways, IConfiguration configuration, IHostEnvironment environment)
        {
            _gateways = gateways;
            DebugMode = string.Equals(configuration["Payments:Debug"], "true", StringComparison.OrdinalIgnoreCase);

            // in Production il debug e' consentito (per ora), ma si segnala una volta nel log: nessun pagamento e' reale
            if (DebugMode && environment.IsProduction() && Interlocked.Exchange(ref _avvisoProduction, 1) == 0)
                Serilog.Log.Warning("Pagamenti delle licenze in modalita' debug anche in Production (Payments:Debug = true): nessuna transazione reale viene eseguita");
        }

        public bool DebugMode { get; }

        private static readonly MetodoPagamento[] Metodi = { MetodoPagamento.CartaCredito, MetodoPagamento.PayPal };

        public List<MetodoPagamento> MetodiDisponibili() => Metodi.Where(m => Resolve(m) != null).ToList();

        public List<MetodoPagamento> MetodiSimulati() => Metodi.Where(m => Resolve(m)?.Debug == true).ToList();

        public List<MetodoPagamento> MetodiSandbox() => Metodi.Where(m => Resolve(m)?.Sandbox == true).ToList();

        // gateway reale configurato per il metodo, altrimenti quello di debug se la modalita' debug e' attiva
        public IPaymentGateway? Resolve(MetodoPagamento metodo)
            => _gateways.FirstOrDefault(g => !g.Debug && g.Supports(metodo))
               ?? (DebugMode ? _gateways.FirstOrDefault(g => g.Debug && g.Supports(metodo)) : null);
    }
}
