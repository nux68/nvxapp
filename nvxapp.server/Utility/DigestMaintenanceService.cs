using System.Globalization;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Digest;
using Serilog;

namespace nvxapp.server.Utility
{
    /*
     Riepilogo AI settimanale (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-AI.2): una volta per settimana, dal lunedi'
     dopo l'ora Digest:Hour (default 7), genera i riepiloghi di tutte le aziende e di tutti gli studi (IDigestGenerator).
     Stato in AppSetting (Riepilogo_UltimaSettimana = "aaaa-Wss", settimana ISO), come PaymentMaintenanceService:
     se il server era spento il lunedi', la generazione parte al primo controllo dopo l'avvio nella stessa settimana.
     Digest:Weekly = false spegne la generazione automatica ("Genera ora" resta disponibile).
     Sta nel progetto server (non nel service) perche' lo scanner dei servizi registra tutto come Scoped.
    */
    public class DigestMaintenanceService : BackgroundService
    {
        private static readonly TimeSpan Intervallo = TimeSpan.FromMinutes(30);
        private readonly IServiceScopeFactory _scopeFactory;
        private readonly IConfiguration _configuration;

        public DigestMaintenanceService(IServiceScopeFactory scopeFactory, IConfiguration configuration)
        {
            _scopeFactory = scopeFactory;
            _configuration = configuration;
        }

        /// <summary>settimana ISO del giorno ("aaaa-Wss")</summary>
        public static string Settimana(DateTime giorno) => $"{ISOWeek.GetYear(giorno)}-W{ISOWeek.GetWeekOfYear(giorno):00}";

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            if (string.Equals(_configuration["Digest:Weekly"], "false", StringComparison.OrdinalIgnoreCase))
            {
                Log.Information("Riepilogo settimanale automatico spento (Digest:Weekly = false)");
                return;
            }
            int ora = int.TryParse(_configuration["Digest:Hour"], out var h) && h >= 0 && h <= 23 ? h : 7;

            // si lascia finire l'avvio (migration degli schemi)
            try { await Task.Delay(TimeSpan.FromMinutes(5), stoppingToken); } catch (OperationCanceledException) { return; }

            using var timer = new PeriodicTimer(Intervallo);
            do
            {
                var adesso = DateTime.Now;
                // la settimana parte il lunedi' all'ora indicata
                if (adesso.DayOfWeek == DayOfWeek.Monday && adesso.Hour < ora)
                    continue;
                try
                {
                    await EseguiAsync(adesso, stoppingToken);
                }
                catch (OperationCanceledException) { return; }
                catch (Exception ex)
                {
                    Log.Error(ex, "Riepilogo settimanale non riuscito");
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task EseguiAsync(DateTime adesso, CancellationToken stoppingToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var appSetting = scope.ServiceProvider.GetRequiredService<IAppSettingRepository>();
            var settimana = Settimana(adesso);
            if (await appSetting.GetValueAsync(PublicDbContext.Key_RiepilogoUltimaSettimana, stoppingToken) == settimana)
                return;

            int generati = await scope.ServiceProvider.GetRequiredService<IDigestGenerator>().GenerateWeeklyAsync(adesso);
            await appSetting.SetValueAsync(PublicDbContext.Key_RiepilogoUltimaSettimana, settimana, stoppingToken);
            Log.Information("Riepilogo settimanale {Settimana}: {Generati} riepiloghi generati", settimana, generati);
        }
    }
}
