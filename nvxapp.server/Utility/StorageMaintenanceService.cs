using Microsoft.EntityFrameworkCore;
using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Storage;
using Serilog;

namespace nvxapp.server.Utility
{
    /*
     Manutenzione dello spazio a disposizione (ANALISI_DOCUMENTALE.md [branch DocumentManagement] §5.3), una volta al giorno dopo le 2 di notte
     (e al primo giro dopo l'avvio):
        - riallinea lo spazio occupato di ogni azienda con quello reale degli applicativi (IStorageUsageProvider)
          e azzera le prenotazioni rimaste appese (caricamenti interrotti)
        - salva la fotografia mensile (CompanyStorageHistory) se manca quella del mese corrente
     Sta nel progetto server (non nel service) perche' lo scanner dei servizi registra tutto come Scoped.
    */
    public class StorageMaintenanceService : BackgroundService
    {
        private static readonly TimeSpan Intervallo = TimeSpan.FromMinutes(30);
        private readonly IServiceScopeFactory _scopeFactory;
        private DateTime? _ultimoGiorno;

        public StorageMaintenanceService(IServiceScopeFactory scopeFactory)
        {
            _scopeFactory = scopeFactory;
        }

        protected override async Task ExecuteAsync(CancellationToken stoppingToken)
        {
            // si lascia finire l'avvio (migration degli schemi)
            try { await Task.Delay(TimeSpan.FromMinutes(2), stoppingToken); } catch (OperationCanceledException) { return; }

            using var timer = new PeriodicTimer(Intervallo);
            do
            {
                var adesso = DateTime.Now;
                bool primoGiro = _ultimoGiorno == null;
                if (primoGiro || (_ultimoGiorno < adesso.Date && adesso.Hour >= 2))
                {
                    try
                    {
                        await EseguiAsync(stoppingToken);
                        _ultimoGiorno = adesso.Date;
                    }
                    catch (OperationCanceledException) { return; }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Manutenzione dello spazio dei file non riuscita");
                        _ultimoGiorno ??= DateTime.MinValue;
                    }
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task EseguiAsync(CancellationToken cancellationToken)
        {
            using var scope = _scopeFactory.CreateScope();
            var companyStorageRepository = scope.ServiceProvider.GetRequiredService<ICompanyStorageRepository>();
            var companyApplicationRepository = scope.ServiceProvider.GetRequiredService<ICompanyApplicationRepository>();
            var historyRepository = scope.ServiceProvider.GetRequiredService<ICompanyStorageHistoryRepository>();
            var quota = scope.ServiceProvider.GetRequiredService<IStorageQuotaService>();
            var providers = scope.ServiceProvider.GetServices<IStorageUsageProvider>().ToList();

            var righe = await companyStorageRepository.FindAll(x => true).Select(x => x.IdCompany).ToListAsync(cancellationToken);
            if (providers.Count > 0)
            {
                var usati = await companyApplicationRepository.FindAll(x => true).Select(x => new { x.IdCompany, x.ApplicationType }).ToListAsync(cancellationToken);
                foreach (var idCompany in righe)
                {
                    long bytes = 0; int files = 0;
                    foreach (var provider in providers.Where(p => usati.Any(u => u.IdCompany == idCompany && u.ApplicationType == p.Application)))
                    {
                        var (b, f) = await provider.UsageAsync(idCompany, cancellationToken);
                        bytes += b; files += f;
                    }
                    await quota.RecalculateAsync(idCompany, bytes, files, cancellationToken);
                }
            }

            // fotografia mensile
            var oggi = DateTime.Today;
            var gia = await historyRepository.FindAll(x => x.Anno == oggi.Year && x.Mese == oggi.Month).Select(x => x.IdCompany).ToListAsync(cancellationToken);
            var daFotografare = await companyStorageRepository.FindAll(x => !gia.Contains(x.IdCompany))
                                        .Select(x => new { x.IdCompany, x.UsedBytes, x.FileCount, Pacchetto = x.StoragePlanNavigation!.Descrizione, x.StoragePlanNavigation.QuotaMB })
                                        .ToListAsync(cancellationToken);
            foreach (var r in daFotografare)
                await historyRepository.CreateAsync(new CompanyStorageHistory
                {
                    IdCompany = r.IdCompany, Anno = oggi.Year, Mese = oggi.Month, Pacchetto = r.Pacchetto, QuotaMB = r.QuotaMB,
                    UsedBytes = r.UsedBytes, FileCount = r.FileCount
                }, SaveChanges: false);
            if (daFotografare.Count > 0)
                await historyRepository.SaveChange();

            Log.Information("Manutenzione dello spazio dei file: {Count} aziende riallineate, {Snapshot} fotografie mensili", righe.Count, daFotografare.Count);
        }
    }
}
