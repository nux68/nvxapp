import { ModelResult } from '../../../ModelsBase/model-result';

// valori predefiniti dell'installazione (solo SuperUser). La quota e' quella della licenza Documentale: i pacchetti di spazio sono stati eliminati.
export class StorageSettingsModel {
  public warningPercent: number = 80;
}

export class StorageSettingsGetInModel { }
export class StorageSettingsGetOutModel extends ModelResult {
  public settings: StorageSettingsModel = new StorageSettingsModel();
  public totalUsedBytes: number = 0;
}

export class StorageSettingsPutInModel {
  public settings: StorageSettingsModel = new StorageSettingsModel();
}
export class StorageSettingsPutOutModel extends ModelResult {
  public settings: StorageSettingsModel = new StorageSettingsModel();
}

// spazio di un'azienda (CompanyStorageModel del server)
export class CompanyStorageModel {
  public idCompany: number = 0;
  public azienda: string = '';
  public studio: string = '';
  // piano della licenza Documentale, "illimitato" (licenze spente) o "senza licenza"
  public pacchetto: string = '';
  public quotaMB: number = 0;
  // sistema licenze spento: nessun limite di spazio
  public illimitato: boolean = false;
  // sistema licenze acceso e nessuna licenza Documentale: quota zero, caricamenti bloccati
  public senzaLicenza: boolean = false;
  public usedBytes: number = 0;
  public fileCount: number = 0;
  public usedPercent: number = 0;
  public warningPercent: number = 80;
  public warningPercentAzienda: number | null = null;
  public overWarning: boolean = false;
  public overQuota: boolean = false;
  public canManage: boolean = false;
}

export class CompanyStorageListInModel { }
export class CompanyStorageListOutModel extends ModelResult {
  public companyStorage: CompanyStorageModel[] = [];
}

export class CompanyStorageGetInModel {
  // 0 = azienda corrente
  public idCompany: number = 0;
}
export class CompanyStorageGetOutModel extends ModelResult {
  public companyStorage: CompanyStorageModel = new CompanyStorageModel();
}

export class CompanyStoragePutInModel {
  public idCompany: number = 0;
  public warningPercent: number | null = null;
}
export class CompanyStoragePutOutModel extends ModelResult {
  public companyStorage: CompanyStorageModel = new CompanyStorageModel();
}

// byte in MB con un decimale, per le pagine
export function bytesToMB(bytes: number): string {
  return (bytes / (1024 * 1024)).toLocaleString(undefined, { maximumFractionDigits: 1 });
}
