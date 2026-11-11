using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories.Public;
using nvxapp.server.service.ClientServer_Service.Infrastructure.Payments;
using Serilog;

namespace nvxapp.server.Utility
{
    /*
     Ripartizione degli incassi ai centri (ANALISI_PAGAMENTI_CENTRI.md §5), una volta al giorno dopo le 4 di notte, solo con
     l'impostazione Pagamenti_RiepilogoAutomatico accesa:
        - dal giorno Pagamenti_GiornoInvioMensile genera le bozze dei rendiconti del mese precedente, una volta per mese
          (Pagamenti_UltimaGenerazione = "aaaa-mm"); il SuperUser le controlla e le conferma, e la conferma invia l'email al centro
        - dal mese Pagamenti_MeseInvioAnnuale invia a ogni centro con movimenti il riepilogo dell'anno precedente, una volta sola
          (controllo sul registro Pag_InvioEmail)
     Sta nel progetto server (non nel service) perche' lo scanner dei servizi registra tutto come Scoped.
    */
    public class PaymentMaintenanceService : BackgroundService
    {
        private static readonly TimeSpan Intervallo = TimeSpan.FromMinutes(30);
        private readonly IServiceScopeFactory _scopeFactory;
        private DateTime? _ultimoGiorno;

        public PaymentMaintenanceService(IServiceScopeFactory scopeFactory)
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
                bool primoGiro = _ultimoGiorno == null;
                if (primoGiro || (_ultimoGiorno < adesso.Date && adesso.Hour >= 4))
                {
                    try
                    {
                        await EseguiAsync(adesso.Date);
                        _ultimoGiorno = adesso.Date;
                    }
                    catch (OperationCanceledException) { return; }
                    catch (Exception ex)
                    {
                        Log.Error(ex, "Manutenzione dei pagamenti ai centri non riuscita");
                        _ultimoGiorno ??= DateTime.MinValue;
                    }
                }
            }
            while (await timer.WaitForNextTickAsync(stoppingToken));
        }

        private async Task EseguiAsync(DateTime oggi)
        {
            using var scope = _scopeFactory.CreateScope();
            var settings = await scope.ServiceProvider.GetRequiredService<IPaymentSettingsProvider>().GetAsync();
            if (!settings.RiepilogoAutomatico)
                return;

            var manager = scope.ServiceProvider.GetRequiredService<IPaymentSettlementManager>();
            var appSetting = scope.ServiceProvider.GetRequiredService<IAppSettingRepository>();

            int bozze = 0, annuali = 0;
            var precedente = oggi.AddMonths(-1);
            var chiave = precedente.ToString("yyyy-MM");
            if (oggi.Day >= settings.GiornoInvioMensile && await appSetting.GetValueAsync(PublicDbContext.Key_PagamentiUltimaGenerazione) != chiave)
            {
                bozze = await manager.GeneraTuttiAsync(precedente.Year, precedente.Month);
                await appSetting.SetValueAsync(PublicDbContext.Key_PagamentiUltimaGenerazione, chiave);
            }

            if (oggi.Month >= settings.MeseInvioAnnuale)
                annuali = await manager.InviaRiepiloghiAnnualiAsync(oggi.Year - 1);

            Log.Information("Manutenzione dei pagamenti ai centri: {Bozze} bozze di rendiconto, {Annuali} riepiloghi annuali inviati", bozze, annuali);
        }
    }
}
