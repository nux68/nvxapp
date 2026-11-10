using nvxapp.server.data.Entities.Public;
using nvxapp.server.data.Infrastructure;
using nvxapp.server.data.Repositories.Public;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.License
{
    /*
     Impostazioni delle licenze (ANALISI_LICENZE.md §5.1, §8.1.1), nella tabella AppSetting:
        - Attive        : false = ILicenseGuard risponde in modo neutro (come Pro, utenti illimitati), nessuno viene bloccato
        - MaxUtenti*    : utenti predefiniti dei piani dei nuovi listini (Presenze; null = illimitati)
        - MaxClienti*   : clienti predefiniti (CRM), MaxSpazioMB*: spazio predefinito in MB (Documentale)
        - GraceGiorni   : giorni dopo la scadenza in cui la licenza vale ancora
        - AvvisoGiorni  : giorni prima della scadenza in cui parte l'avviso
     Letti una volta per richiesta (servizio Scoped).
    */
    public class LicenseSettings
    {
        public bool Attive { get; set; }
        public int? MaxUtentiFree { get; set; } = 10;
        public int? MaxUtentiStandard { get; set; } = 50;
        public int? MaxUtentiPro { get; set; }
        public int? MaxClientiFree { get; set; } = 100;
        public int? MaxClientiStandard { get; set; } = 1000;
        public int? MaxClientiPro { get; set; }
        public int? MaxSpazioMBFree { get; set; } = 1024;
        public int? MaxSpazioMBStandard { get; set; } = 5120;
        public int? MaxSpazioMBPro { get; set; } = 20480;
        public int GraceGiorni { get; set; } = 7;
        public int AvvisoGiorni { get; set; } = 10;

        public int? MaxUtenti(LivelloLicenza livello) => livello switch
        {
            LivelloLicenza.Free => MaxUtentiFree,
            LivelloLicenza.Standard => MaxUtentiStandard,
            _ => MaxUtentiPro
        };

        public int? MaxClienti(LivelloLicenza livello) => livello switch
        {
            LivelloLicenza.Free => MaxClientiFree,
            LivelloLicenza.Standard => MaxClientiStandard,
            _ => MaxClientiPro
        };

        public int? MaxSpazioMB(LivelloLicenza livello) => livello switch
        {
            LivelloLicenza.Free => MaxSpazioMBFree,
            LivelloLicenza.Standard => MaxSpazioMBStandard,
            _ => MaxSpazioMBPro
        };
    }

    public interface ILicenseSettingsProvider
    {
        Task<LicenseSettings> GetAsync();
        Task SaveAsync(LicenseSettings settings);
    }

    public class LicenseSettingsProvider : ILicenseSettingsProvider
    {
        private readonly IAppSettingRepository _appSettingRepository;
        private LicenseSettings? _cache;

        public LicenseSettingsProvider(IAppSettingRepository appSettingRepository)
        {
            _appSettingRepository = appSettingRepository;
        }

        public async Task<LicenseSettings> GetAsync()
        {
            if (_cache != null)
                return _cache;

            async Task<int?> Utenti(string key, int? def)
            {
                var v = await _appSettingRepository.GetValueAsync(key);
                if (v == null)
                    return def;
                return int.TryParse(v, out var n) && n > 0 ? n : null; // vuoto = illimitati
            }
            async Task<int> Giorni(string key, int def)
            {
                var v = await _appSettingRepository.GetValueAsync(key);
                return int.TryParse(v, out var n) && n >= 0 ? n : def;
            }

            _cache = new LicenseSettings
            {
                Attive = string.Equals(await _appSettingRepository.GetValueAsync(PublicDbContext.Key_LicenzeAttive), "true", StringComparison.OrdinalIgnoreCase),
                MaxUtentiFree = await Utenti(PublicDbContext.Key_LicenzeMaxUtentiFree, 10),
                MaxUtentiStandard = await Utenti(PublicDbContext.Key_LicenzeMaxUtentiStandard, 50),
                MaxUtentiPro = await Utenti(PublicDbContext.Key_LicenzeMaxUtentiPro, null),
                MaxClientiFree = await Utenti(PublicDbContext.Key_LicenzeMaxClientiFree, 100),
                MaxClientiStandard = await Utenti(PublicDbContext.Key_LicenzeMaxClientiStandard, 1000),
                MaxClientiPro = await Utenti(PublicDbContext.Key_LicenzeMaxClientiPro, null),
                MaxSpazioMBFree = await Utenti(PublicDbContext.Key_LicenzeMaxSpazioMBFree, 1024),
                MaxSpazioMBStandard = await Utenti(PublicDbContext.Key_LicenzeMaxSpazioMBStandard, 5120),
                MaxSpazioMBPro = await Utenti(PublicDbContext.Key_LicenzeMaxSpazioMBPro, 20480),
                GraceGiorni = await Giorni(PublicDbContext.Key_LicenzeGraceGiorni, 7),
                AvvisoGiorni = await Giorni(PublicDbContext.Key_LicenzeAvvisoGiorni, 10)
            };
            return _cache;
        }

        public async Task SaveAsync(LicenseSettings settings)
        {
            static string Utenti(int? n) => n.HasValue && n > 0 ? n.Value.ToString() : string.Empty;

            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_LicenzeAttive, settings.Attive ? "true" : "false");
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_LicenzeMaxUtentiFree, Utenti(settings.MaxUtentiFree));
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_LicenzeMaxUtentiStandard, Utenti(settings.MaxUtentiStandard));
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_LicenzeMaxUtentiPro, Utenti(settings.MaxUtentiPro));
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_LicenzeMaxClientiFree, Utenti(settings.MaxClientiFree));
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_LicenzeMaxClientiStandard, Utenti(settings.MaxClientiStandard));
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_LicenzeMaxClientiPro, Utenti(settings.MaxClientiPro));
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_LicenzeMaxSpazioMBFree, Utenti(settings.MaxSpazioMBFree));
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_LicenzeMaxSpazioMBStandard, Utenti(settings.MaxSpazioMBStandard));
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_LicenzeMaxSpazioMBPro, Utenti(settings.MaxSpazioMBPro));
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_LicenzeGraceGiorni, settings.GraceGiorni.ToString());
            await _appSettingRepository.SetValueAsync(PublicDbContext.Key_LicenzeAvvisoGiorni, settings.AvvisoGiorni.ToString());
            _cache = null;
        }
    }
}
