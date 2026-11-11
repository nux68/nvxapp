import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  CompanyStorageGetInModel, CompanyStorageGetOutModel, CompanyStorageListInModel, CompanyStorageListOutModel,
  CompanyStoragePutInModel, CompanyStoragePutOutModel, StorageSettingsGetInModel, StorageSettingsGetOutModel,
  StorageSettingsPutInModel, StorageSettingsPutOutModel
} from './Models/storage-model';

// Spazio a disposizione dei file delle aziende (documentale): valori predefiniti, spazio delle aziende
@Injectable({
  providedIn: 'root'
})
export class StorageService {

  constructor(private http: HttpClient) { }

  private post<I, O>(action: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + 'Storage/' + action, model);
  }

  StorageSettingsGet(model: GenericRequest<StorageSettingsGetInModel>) { return this.post<StorageSettingsGetInModel, StorageSettingsGetOutModel>('StorageSettingsGet', model); }
  StorageSettingsPut(model: GenericRequest<StorageSettingsPutInModel>) { return this.post<StorageSettingsPutInModel, StorageSettingsPutOutModel>('StorageSettingsPut', model); }
  CompanyStorageList(model: GenericRequest<CompanyStorageListInModel>) { return this.post<CompanyStorageListInModel, CompanyStorageListOutModel>('CompanyStorageList', model); }
  CompanyStorageGet(model: GenericRequest<CompanyStorageGetInModel>) { return this.post<CompanyStorageGetInModel, CompanyStorageGetOutModel>('CompanyStorageGet', model); }
  CompanyStoragePut(model: GenericRequest<CompanyStoragePutInModel>) { return this.post<CompanyStoragePutInModel, CompanyStoragePutOutModel>('CompanyStoragePut', model); }
}
