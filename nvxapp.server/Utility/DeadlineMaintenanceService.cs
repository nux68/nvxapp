using nvxapp.server.service.ClientServer_Service.Infrastructure.Deadline;
using Serilog;

namespace nvxapp.server.Utility
{
    /*
     Promemoria dello scadenzario (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.5), una volta al giorno dopo le 6
     (e al primo giro dopo l'avvio): avvisa il responsabile, o gli amministratori dell'azienda, delle scadenze che entrano
     nel preavviso e di quelle arrivate alla data (una volta sola per fase, vedi DeadlineReminder).
     Sta nel progetto server (non nel service) perche' lo scanner dei servizi registra tutto come Scoped.
    */
    public class DeadlineMaintenanceService : BackgroundService
    {
        private static readonly TimeSpan Intervallo = TimeSpan.FromMinutes(30);
        private readonly IServiceScopeFactory _scopeFactory;
        private DateTime? _ultimoGiorno;

        public DeadlineMaintenanceService(IServiceScopeFactory scopeFactory)
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
                if (primoGiro || (_ultimoGiorno < adesso.Date && adesso.Hour >= 6))
                {
                    try
                    {
                        using var scope = _scopeFactory.CreateScope();
                        int avvisi = await scope.ServiceProvider.GetRequiredService<IDeadlineReminder>().SendRemindersAsync(adesso.Date);
                        Log.Information("Scadenzario: {Avvisi} promemoria inviati", avvisi);
                        _ultimoGiorno = adesso.Date;
                    }
                    catch (OperationCanceledException) { return; }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Promemoria dello scadenzario non riuscito");
                        _ultimoGiorno ??= DateTime.MinValue;
                    }
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
