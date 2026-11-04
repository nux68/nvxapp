using nvxapp.server.service.ClientServer_Service.ModelsBase;

namespace nvxapp.server.service.ClientServer_Service.Infrastructure.Storage.Models
{
    // pacchetto di spazio (StoragePlan)
    public class StoragePlanModel
    {
        public int Id { get; set; }
        public string Descrizione { get; set; } = string.Empty;
        public int QuotaMB { get; set; }
        public int? MaxFileMB { get; set; }
        public int Ordine { get; set; }
        public bool Active { get; set; } = true;
        public bool Predefinito { get; set; }
        // aziende che lo usano (un pacchetto usato non si elimina, si disattiva)
        public int Aziende { get; set; }
    }

    // valori predefiniti dell'installazione e pacchetti (solo SuperUser)
    public class StorageSettingsGetInModel { }

    public class StorageSettingsModel
    {
        public int MaxFileMB { get; set; }
        public int WarningPercent { get; set; }
    }

    public class StorageSettingsGetOutModel : ModelResult
    {
        public StorageSettingsModel Settings { get; set; } = new StorageSettingsModel();
        public List<StoragePlanModel> Plans { get; set; } = new List<StoragePlanModel>();
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

    public class StoragePlanPutInModel
    {
        public StoragePlanModel Plan { get; set; } = new StoragePlanModel();
    }

    public class StoragePlanPutOutModel : ModelResult
    {
        public StoragePlanModel Plan { get; set; } = new StoragePlanModel();
    }

    public class StoragePlanDeleteInModel
    {
        public int Id { get; set; }
    }

    public class StoragePlanDeleteOutModel : ModelResult
    {
        // true = eliminato; false = usato da aziende, quindi solo disattivato
        public bool Deleted { get; set; }
    }

    // spazio di un'azienda
    public class CompanyStorageModel
    {
        public int IdCompany { get; set; }
        public string Azienda { get; set; } = string.Empty;
        public string Studio { get; set; } = string.Empty;
        public int IdStoragePlan { get; set; }
        public string Pacchetto { get; set; } = string.Empty;
        public int QuotaMB { get; set; }
        public long UsedBytes { get; set; }
        public int FileCount { get; set; }
        public int UsedPercent { get; set; }
        public int WarningPercent { get; set; }
        public int MaxFileMB { get; set; }
        // valori propri dell'azienda (null = del pacchetto / predefiniti)
        public int? MaxFileMBAzienda { get; set; }
        public int? WarningPercentAzienda { get; set; }
        public bool OverWarning { get; set; }
        public bool OverQuota { get; set; }
        // l'utente corrente puo' cambiare pacchetto e limiti (SuperUser o DealerPowerAdmin del centro)
        public bool CanManage { get; set; }
    }

    public class CompanyStorageListInModel { }

    public class CompanyStorageListOutModel : ModelResult
    {
        public List<CompanyStorageModel> CompanyStorage { get; set; } = new List<CompanyStorageModel>();
        public List<StoragePlanModel> Plans { get; set; } = new List<StoragePlanModel>();
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
        public int IdStoragePlan { get; set; }
        public int? MaxFileMB { get; set; }
        public int? WarningPercent { get; set; }
    }

    public class CompanyStoragePutOutModel : ModelResult
    {
        public CompanyStorageModel CompanyStorage { get; set; } = new CompanyStorageModel();
    }
}
