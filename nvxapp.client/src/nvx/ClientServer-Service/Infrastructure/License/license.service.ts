import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  LicenseAutoRenewPutInModel, LicenseAutoRenewPutOutModel, LicenseCancelInModel, LicenseCancelOutModel, LicenseCatalogDeleteInModel, LicenseCatalogDeleteOutModel,
  LicenseCatalogGetInModel, LicenseCatalogGetOutModel, LicenseCatalogListInModel, LicenseCatalogListOutModel,
  LicenseCatalogPutInModel, LicenseCatalogPutOutModel, LicensePublicPriceListInModel, LicensePublicPriceListOutModel, LicenseRenewInModel, LicenseRenewOutModel,
  LicenseSettingsGetInModel, LicenseSettingsGetOutModel, LicenseSettingsPutInModel, LicenseSettingsPutOutModel,
  LicenseStatusGetInModel, LicenseStatusGetOutModel, LicenseSubscribeInModel, LicenseSubscribeOutModel
} from './Models/license-model';

// Licenze d'uso (ANALISI_INFRASTRUCTURE_LICENZE.md): impostazioni e listini (SuperUser, centro abilitato), licenze dell'azienda o dello studio
@Injectable({
  providedIn: 'root'
})
export class LicenseService {

  constructor(private http: HttpClient) { }

  private post<I, O>(action: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + 'License/' + action, model);
  }

  LicenseSettingsGet(model: GenericRequest<LicenseSettingsGetInModel>) { return this.post<LicenseSettingsGetInModel, LicenseSettingsGetOutModel>('LicenseSettingsGet', model); }
  LicenseSettingsPut(model: GenericRequest<LicenseSettingsPutInModel>) { return this.post<LicenseSettingsPutInModel, LicenseSettingsPutOutModel>('LicenseSettingsPut', model); }
  LicenseCatalogList(model: GenericRequest<LicenseCatalogListInModel>) { return this.post<LicenseCatalogListInModel, LicenseCatalogListOutModel>('LicenseCatalogList', model); }
  LicenseCatalogGet(model: GenericRequest<LicenseCatalogGetInModel>) { return this.post<LicenseCatalogGetInModel, LicenseCatalogGetOutModel>('LicenseCatalogGet', model); }
  LicenseCatalogPut(model: GenericRequest<LicenseCatalogPutInModel>) { return this.post<LicenseCatalogPutInModel, LicenseCatalogPutOutModel>('LicenseCatalogPut', model); }
  LicenseCatalogDelete(model: GenericRequest<LicenseCatalogDeleteInModel>) { return this.post<LicenseCatalogDeleteInModel, LicenseCatalogDeleteOutModel>('LicenseCatalogDelete', model); }
  // pagina prezzi dello showcase, senza login
  LicensePublicPriceList(model: GenericRequest<LicensePublicPriceListInModel>) { return this.post<LicensePublicPriceListInModel, LicensePublicPriceListOutModel>('LicensePublicPriceList', model); }
  LicenseStatusGet(model: GenericRequest<LicenseStatusGetInModel>) { return this.post<LicenseStatusGetInModel, LicenseStatusGetOutModel>('LicenseStatusGet', model); }
  LicenseSubscribe(model: GenericRequest<LicenseSubscribeInModel>) { return this.post<LicenseSubscribeInModel, LicenseSubscribeOutModel>('LicenseSubscribe', model); }
  LicenseRenew(model: GenericRequest<LicenseRenewInModel>) { return this.post<LicenseRenewInModel, LicenseRenewOutModel>('LicenseRenew', model); }
  LicenseAutoRenewPut(model: GenericRequest<LicenseAutoRenewPutInModel>) { return this.post<LicenseAutoRenewPutInModel, LicenseAutoRenewPutOutModel>('LicenseAutoRenewPut', model); }
  LicenseCancel(model: GenericRequest<LicenseCancelInModel>) { return this.post<LicenseCancelInModel, LicenseCancelOutModel>('LicenseCancel', model); }
}
