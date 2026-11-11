using nvxapp.server.service.ClientServer_Service.Assets._utility;
using Serilog;

namespace nvxapp.server.Utility
{
    /*
     Risorse aziendali (ANALISI_ASSETS_APPLICATIVO.md, AST-BENI.3): una volta al giorno dopo le 6 (e al primo giro dopo l'avvio)
     controlla le cessazioni dei dipendenti (data in Common) e avvisa gli amministratori dei beni e dei veicoli da restituire
     (scadenzario + notifica, vedi AstRestituzioni). File del ramo Assets, registrato in Installers4Assets.
     Sta nel progetto server (non nel service) perche' lo scanner dei servizi registra tutto come Scoped.
    */
    public class AstRestituzioniMaintenanceService : BackgroundService
    {
        private static readonly TimeSpan Intervallo = TimeSpan.FromMinutes(30);
        private readonly IServiceScopeFactory _scopeFactory;
        private DateTime? _ultimoGiorno;

        public AstRestituzioniMaintenanceService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // si lascia finire l'avvio (migration degli schemi)
            try { await Task.Delay(TimeSpan.FromMinutes(4), stoppingToken); } catch (OperationCanceledException) { return; }

            using var timer = new PeriodicTimer(Intervallo);
            do
            {
                var adesso = DateTime.Now;
                if (_ultimoGiorno == null || (_ultimoGiorno < adesso.Date && adesso.Hour >= 6))
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        int avvisi = await scope.ServiceProvider.GetRequiredService<IAstRestituzioni>().VerificaTutteAsync(stoppingToken);
                        Log.Information("Risorse aziendali: {Avvisi} restituzioni alla cessazione avvisate", avvisi);
                        _ultimoGiorno = adesso.Date;
                    }
                    catch (OperationCanceledException) { return; }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Risorse aziendali: verifica delle restituzioni non riuscita");
                        _ultimoGiorno ??= DateTime.MinValue;
                    }
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
