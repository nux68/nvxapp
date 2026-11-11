using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Storage.Models
{
    // valori predefiniti dell'installazione (solo SuperUser)
    public class StorageSettingsGetInModel { }

    public class StorageSettingsModel
    {
        public int WarningPercent { get; set; }
    }

    public class StorageSettingsGetOutModel : ModelResult
    {
        public StorageSettingsModel Settings { get; set; } = new StorageSettingsModel();
        // spazio occupato da tutte le aziende (byte)
        public long TotalUsedBytes { get; set; }
    }

    public class StorageSettingsPutInModel
    {
        public StorageSettingsModel Settings { get; set; } = new StorageSettingsModel();
    }

    public class StorageSettingsPutOutModel : ModelResult
    {
        public StorageSettingsModel Settings { get; set; } = new StorageSettingsModel();
    }

    // spazio di un'azienda
    public class CompanyStorageModel
    {
        public int IdCompany { get; set; }
        public string Azienda { get; set; } = string.Empty;
        public string Studio { get; set; } = string.Empty;
        // piano della licenza Documentale, "illimitato" (licenze spente) o "senza licenza"
        public string Pacchetto { get; set; } = string.Empty;
        public int QuotaMB { get; set; }
        // sistema licenze spento: nessun limite di spazio
        public bool Illimitato { get; set; }
        // sistema licenze acceso e nessuna licenza Documentale: quota zero, caricamenti bloccati
        public bool SenzaLicenza { get; set; }
        public long UsedBytes { get; set; }
        public int FileCount { get; set; }
        public int UsedPercent { get; set; }
        public int WarningPercent { get; set; }
        // soglia propria dell'azienda (null = predefinita)
        public int? WarningPercentAzienda { get; set; }
        public bool OverWarning { get; set; }
        public bool OverQuota { get; set; }
        // l'utente corrente puo' cambiare la soglia di avviso (SuperUser o DealerPowerAdmin del centro)
        public bool CanManage { get; set; }
    }

    public class CompanyStorageListInModel { }

    public class CompanyStorageListOutModel : ModelResult
    {
        public List<CompanyStorageModel> CompanyStorage { get; set; } = new List<CompanyStorageModel>();
    }

    public class CompanyStorageGetInModel
    {
        // 0 = azienda corrente
        public int IdCompany { get; set; }
    }

    public class CompanyStorageGetOutModel : ModelResult
    {
        public CompanyStorageModel CompanyStorage { get; set; } = new CompanyStorageModel();
    }

    public class CompanyStoragePutInModel
    {
        public int IdCompany { get; set; }
        public int? WarningPercent { get; set; }
    }

    public class CompanyStoragePutOutModel : ModelResult
    {
        public CompanyStorageModel CompanyStorage { get; set; } = new CompanyStorageModel();
    }
}
