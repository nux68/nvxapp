using System.Globalization;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Payments
{
    /*
     Impostazioni della ripartizione degli incassi ai centri (ANALISI_INFRASTRUCTURE_PAGAMENTI_CENTRI.md §3), nella tabella AppSetting:
        - PercentualePiattaforma : trattenuta della piattaforma (0-100) per i centri senza una propria
        - RiepilogoAutomatico    : la manutenzione genera le bozze dei rendiconti e invia i riepiloghi annuali
        - GiornoInvioMensile     : dal giorno del mese in cui si generano le bozze del mese precedente (1-28)
        - MeseInvioAnnuale       : mese in cui si invia il riepilogo dell'anno precedente (1-12)
    */
    public class PaymentSettings
    {
        public decimal PercentualePiattaforma { get; set; }
        public bool RiepilogoAutomatico { get; set; }
        public int GiornoInvioMensile { get; set; } = 5;
        public int MeseInvioAnnuale { get; set; } = 1;
    }

    public interface IPaymentSettingsProvider
    {
        Task<PaymentSettings> GetAsync();
        Task SaveAsync(PaymentSettings settings);
    }

    public class PaymentSettingsProvider : IPaymentSettingsProvider
    {
        private readonly IAppSettingRepository _appSettingRepository;

        public PaymentSettingsProvider(IAppSettingRepository appSettingRepository)
        {
            _appSettingRepository = appSettingRepository;
        }

        public async Task<PaymentSettings> GetAsync()
        {
            async Task<int> Intero(string key, int def, int min, int max)
            {
                var v = await _appSettingRepository.GetValueAsync(key);
                return int.TryParse(v, out var n) && n >= min && n <= max ? n : def;
            }

            var perc = await _appSettingRepository.GetValueAsync(PublicDbContext.Key_PagamentiPercentualePiattaforma);
            return new PaymentSettings
            {
                PercentualePiattaforma = decimal.TryParse(perc, NumberStyles.Number, CultureInfo.InvariantCulture, out var p) && p >= 0 && p <= 100 ? p : 0,
                RiepilogoAutomatico = string.Equals(await _appSettingRepository.GetValueAsync(PublicDbContext.Key_PagamentiRiepilogoAutomatico), "true", StringComparison.OrdinalIgnoreCase),
                GiornoInvioMensile = await Intero(PublicDbContext.Key_PagamentiGiornoInvioMensile, 5, 1, 28),
                MeseInvioAnnuale = await Intero(PublicDbContext.Key_PagamentiMeseInvioAnnuale, 1, 1, 12)
            };
        }

        public async Task SaveAsync(PaymentSettings settings)
        {
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_PagamentiPercentualePiattaforma, settings.PercentualePiattaforma.ToString(CultureInfo.InvariantCulture));
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_PagamentiRiepilogoAutomatico, settings.RiepilogoAutomatico ? "true" : "false");
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_PagamentiGiornoInvioMensile, settings.GiornoInvioMensile.ToString(CultureInfo.InvariantCulture));
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_PagamentiMeseInvioAnnuale, settings.MeseInvioAnnuale.ToString(CultureInfo.InvariantCulture));
        }
    }
}
