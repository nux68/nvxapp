import { ModelResult } from '../../../ModelsBase/model-result';

// pacchetto di spazio (StoragePlanModel del server)
export class StoragePlanModel {
  public id: number = 0;
  public descrizione: string = '';
  public quotaMB: number = 1024;
  public maxFileMB: number | null = null;
  public ordine: number = 0;
  public active: boolean = true;
  public predefinito: boolean = false;
  // aziende che lo usano (un pacchetto usato non si elimina, si disattiva)
  public aziende: number = 0;
}

export class StorageSettingsModel {
  public maxFileMB: number = 50;
  public warningPercent: number = 80;
}

export class StorageSettingsGetInModel { }
export class StorageSettingsGetOutModel extends ModelResult {
  public settings: StorageSettingsModel = new StorageSettingsModel();
  public plans: StoragePlanModel[] = [];
  public totalUsedBytes: number = 0;
}

export class StorageSettingsPutInModel {
  public settings: StorageSettingsModel = new StorageSettingsModel();
}
export class StorageSettingsPutOutModel extends ModelResult {
  public settings: StorageSettingsModel = new StorageSettingsModel();
}

export class StoragePlanPutInModel {
  public plan: StoragePlanModel = new StoragePlanModel();
}
export class StoragePlanPutOutModel extends ModelResult {
  public plan: StoragePlanModel = new StoragePlanModel();
}

export class StoragePlanDeleteInModel {
  public id: number = 0;
}
export class StoragePlanDeleteOutModel extends ModelResult {
  public deleted: boolean = false;
}

// spazio di un'azienda (CompanyStorageModel del server)
export class CompanyStorageModel {
  public idCompany: number = 0;
  public azienda: string = '';
  public studio: string = '';
  public idStoragePlan: number = 0;
  public pacchetto: string = '';
  public quotaMB: number = 0;
  public usedBytes: number = 0;
  public fileCount: number = 0;
  public usedPercent: number = 0;
  public warningPercent: number = 80;
  public maxFileMB: number = 50;
  public maxFileMBAzienda: number | null = null;
  public warningPercentAzienda: number | null = null;
  public overWarning: boolean = false;
  public overQuota: boolean = false;
  public canManage: boolean = false;
}

export class CompanyStorageListInModel { }
export class CompanyStorageListOutModel extends ModelResult {
  public companyStorage: CompanyStorageModel[] = [];
  public plans: StoragePlanModel[] = [];
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
  public idStoragePlan: number = 0;
  public maxFileMB: number | null = null;
  public warningPercent: number | null = null;
}
export class CompanyStoragePutOutModel extends ModelResult {
  public companyStorage: CompanyStorageModel = new CompanyStorageModel();
}

// byte in MB con un decimale, per le pagine
export function bytesToMB(bytes: number): string {
  return (bytes / (1024 * 1024)).toLocaleString(undefined, { maximumFractionDigits: 1 });
}
