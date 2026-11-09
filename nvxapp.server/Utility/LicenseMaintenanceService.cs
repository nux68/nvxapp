using nvxapp.server.service.ClientServer_Service.Infrastructure.License;
using Serilog;

namespace nvxapp.server.Utility
{
    /*
     Manutenzione delle licenze (ANALISI_LICENZE.md §6), una volta al giorno dopo le 3 di notte (e al primo giro dopo l'avvio):
        - rinnova gli abbonamenti con rinnovo automatico scaduti (gratuiti, o a pagamento in modalita' debug: con un provider
          reale i rinnovi li addebita il provider)
        - porta a Scaduto gli abbonamenti oltre i giorni di grace
        - manda gli avvisi di scadenza ai PowerAdmin del soggetto (se l'invio email e' configurato)
     Sta nel progetto server (non nel service) perche' lo scanner dei servizi registra tutto come Scoped.
    */
    public class LicenseMaintenanceService : BackgroundService
    {
        private static readonly TimeSpan Intervallo = TimeSpan.FromMinutes(30);
        private readonly IServiceScopeFactory _scopeFactory;
        private DateTime? _ultimoGiorno;

        public LicenseMaintenanceService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // si lascia finire l'avvio (migration degli schemi)
            try { await Task.Delay(TimeSpan.FromMinutes(3), stoppingToken); } catch (OperationCanceledException) { return; }

            using var timer = new PeriodicTimer(Intervallo);
            do
            {
                var adesso = DateTime.Now;
                bool primoGiro = _ultimoGiorno == null;
                if (primoGiro || (_ultimoGiorno < adesso.Date && adesso.Hour >= 3))
                {
                    try
                    {
                        await EseguiAsync();
                        _ultimoGiorno = adesso.Date;
                    }
                    catch (OperationCanceledException) { return; }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Manutenzione delle licenze non riuscita");
                        _ultimoGiorno ??= DateTime.MinValue;
                    }
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task EseguiAsync()
        {
            using var scope = _scopeFactory.CreateScope();
            var manager = scope.ServiceProvider.GetRequiredService<ILicenseSubscriptionManager>();

            int rinnovati = await manager.AutoRenewAsync();
            int scaduti = await manager.ExpireAsync();
            int avvisi = await manager.SendWarningsAsync();

            Log.Information("Manutenzione delle licenze: {Rinnovati} rinnovi automatici, {Scaduti} scadute, {Avvisi} avvisi di scadenza", rinnovati, scaduti, avvisi);
        }
    }
}
