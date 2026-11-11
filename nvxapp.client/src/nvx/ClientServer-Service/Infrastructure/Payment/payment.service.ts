import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  PaymentDealerConfigGetInModel, PaymentDealerConfigGetOutModel, PaymentDealerConfigPutInModel, PaymentDealerConfigPutOutModel,
  PaymentDealerGetInModel, PaymentDealerGetOutModel, PaymentExportInModel, PaymentExportOutModel,
  PaymentReportGetInModel, PaymentReportGetOutModel, PaymentSettingsGetInModel, PaymentSettingsGetOutModel,
  PaymentSettingsPutInModel, PaymentSettingsPutOutModel, PaymentSettlementCancelInModel, PaymentSettlementCancelOutModel,
  PaymentSettlementConfirmInModel, PaymentSettlementConfirmOutModel, PaymentSettlementGenerateInModel, PaymentSettlementGenerateOutModel,
  PaymentSettlementInvoicePutInModel, PaymentSettlementInvoicePutOutModel, PaymentSettlementPayInModel, PaymentSettlementPayOutModel,
  PaymentSummarySendInModel, PaymentSummarySendOutModel
} from './Models/payment-model';

// Pagamenti delle licenze e ripartizione ai centri (ANALISI_PAGAMENTI_CENTRI.md): SuperUser e utenti del centro
@Injectable({
  providedIn: 'root'
})
export class PaymentService {

  constructor(private http: HttpClient) { }

  private post<I, O>(action: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + 'PaymentReport/' + action, model);
  }

  PaymentReportGet(model: GenericRequest<PaymentReportGetInModel>) { return this.post<PaymentReportGetInModel, PaymentReportGetOutModel>('PaymentReportGet', model); }
  PaymentDealerGet(model: GenericRequest<PaymentDealerGetInModel>) { return this.post<PaymentDealerGetInModel, PaymentDealerGetOutModel>('PaymentDealerGet', model); }
  PaymentSettlementGenerate(model: GenericRequest<PaymentSettlementGenerateInModel>) { return this.post<PaymentSettlementGenerateInModel, PaymentSettlementGenerateOutModel>('PaymentSettlementGenerate', model); }
  PaymentSettlementConfirm(model: GenericRequest<PaymentSettlementConfirmInModel>) { return this.post<PaymentSettlementConfirmInModel, PaymentSettlementConfirmOutModel>('PaymentSettlementConfirm', model); }
  PaymentSettlementCancel(model: GenericRequest<PaymentSettlementCancelInModel>) { return this.post<PaymentSettlementCancelInModel, PaymentSettlementCancelOutModel>('PaymentSettlementCancel', model); }
  PaymentSettlementPay(model: GenericRequest<PaymentSettlementPayInModel>) { return this.post<PaymentSettlementPayInModel, PaymentSettlementPayOutModel>('PaymentSettlementPay', model); }
  PaymentSettlementInvoicePut(model: GenericRequest<PaymentSettlementInvoicePutInModel>) { return this.post<PaymentSettlementInvoicePutInModel, PaymentSettlementInvoicePutOutModel>('PaymentSettlementInvoicePut', model); }
  PaymentSummarySend(model: GenericRequest<PaymentSummarySendInModel>) { return this.post<PaymentSummarySendInModel, PaymentSummarySendOutModel>('PaymentSummarySend', model); }
  PaymentExport(model: GenericRequest<PaymentExportInModel>) { return this.post<PaymentExportInModel, PaymentExportOutModel>('PaymentExport', model); }
  PaymentDealerConfigGet(model: GenericRequest<PaymentDealerConfigGetInModel>) { return this.post<PaymentDealerConfigGetInModel, PaymentDealerConfigGetOutModel>('PaymentDealerConfigGet', model); }
  PaymentDealerConfigPut(model: GenericRequest<PaymentDealerConfigPutInModel>) { return this.post<PaymentDealerConfigPutInModel, PaymentDealerConfigPutOutModel>('PaymentDealerConfigPut', model); }
  PaymentSettingsGet(model: GenericRequest<PaymentSettingsGetInModel>) { return this.post<PaymentSettingsGetInModel, PaymentSettingsGetOutModel>('PaymentSettingsGet', model); }
  PaymentSettingsPut(model: GenericRequest<PaymentSettingsPutInModel>) { return this.post<PaymentSettingsPutInModel, PaymentSettingsPutOutModel>('PaymentSettingsPut', model); }
}
