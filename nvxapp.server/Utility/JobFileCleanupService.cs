using nvxapp.server.service.Helpers;
using Serilog;

namespace nvxapp.server.Utility
{
    /*
     Cancella i file dei job (export, cartellini, report) dalla cartella NVXSystem.ExportFolder dopo
     Export:RetentionHours ore (default 24): contengono dati dei dipendenti e prima restavano per sempre.
     Gira all'avvio e poi ogni ora. Sta nel progetto server (non nel service) perche' lo scanner dei
     servizi registra tutte le classi come Scoped, che per un hosted service non va bene.
    */
    public class JobFileCleanupService : BackgroundService
    {
        private static readonly TimeSpan Intervallo = TimeSpan.FromHours(1);
        private readonly TimeSpan _retention;

        public JobFileCleanupService(IConfiguration configuration)
        {
            var ore = int.TryParse(configuration["Export:RetentionHours"], out var n) && n > 0 ? n : 24;
            _retention = TimeSpan.FromHours(ore);
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            using var timer = new PeriodicTimer(Intervallo);
            do
            {
                try
                {
                    var cancellati = NVXSystem.PulisciFileJob(_retention);
                    if (cancellati > 0)
                        Log.Information("Pulizia file dei job: cancellati {Count} file piu' vecchi di {Hours} ore", cancellati, _retention.TotalHours);
                }
                catch (Exception ex)
                {
                    Log.Error(ex, "Pulizia file dei job non riuscita");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }
    }
}
