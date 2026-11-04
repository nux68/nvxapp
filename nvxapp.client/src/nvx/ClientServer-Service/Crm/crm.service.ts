import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { firstValueFrom, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { NvxHttpInterceptorService } from '../../http/http-interceptor';
import {
  CrmAllegatoUploadModel, CrmAllegatoUploadOutModel, CrmAttivitaCompletaInModel, CrmAttivitaGetOutModel, CrmAttivitaListInModel, CrmAttivitaListOutModel,
  CrmAttivitaPutInModel, CrmAttivitaPutOutModel, CrmCambiaFaseInModel, CrmCfgPutInModel, CrmCfgPutOutModel, CrmClienteGetOutModel, CrmClienteListInModel,
  CrmClienteListOutModel, CrmClientePutInModel, CrmClientePutOutModel, CrmContattoPutInModel, CrmContattoPutOutModel, CrmEmptyInModel, CrmIdInModel,
  CrmIdOutModel, CrmLeadConvertiInModel, CrmLeadConvertiOutModel, CrmLeadGetOutModel, CrmLeadListInModel, CrmLeadListOutModel, CrmLeadPutInModel,
  CrmLeadPutOutModel, CrmNotaPutInModel, CrmNotaPutOutModel, CrmOffertaGetOutModel, CrmOffertaInviaInModel, CrmOffertaListInModel, CrmOffertaListOutModel,
  CrmOffertaPutInModel, CrmOffertaPutOutModel, CrmOpportunitaGetOutModel, CrmOpportunitaListInModel, CrmOpportunitaListOutModel, CrmOpportunitaPutInModel,
  CrmOpportunitaPutOutModel, CrmParDeleteInModel, CrmParPutInModel, CrmParPutOutModel, CrmReportInModel, CrmReportOutModel, CrmRiassegnaInModel,
  CrmRiassegnaOutModel, CrmSetupGetOutModel
} from './Models/crm-model';

/*
 CRM (ANALISI_CRM.md). Allegati: caricamento multipart (dati JSON + file) e download in streaming (blob), come il
 documentale; PDF delle offerte ed export Excel dei clienti scaricati allo stesso modo.
*/
@Injectable({
  providedIn: 'root'
})
export class CrmService {

  constructor(private http: HttpClient, private nvxHttpInterceptorService: NvxHttpInterceptorService) { }

  private post<I, O>(api: string, action: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + api + '/' + action, model);
  }

  // configurazione
  SetupGet(m: GenericRequest<CrmEmptyInModel>) { return this.post<CrmEmptyInModel, CrmSetupGetOutModel>('Crm_Config', 'Crm_SetupGet', m); }
  CfgPut(m: GenericRequest<CrmCfgPutInModel>) { return this.post<CrmCfgPutInModel, CrmCfgPutOutModel>('Crm_Config', 'Crm_CfgPut', m); }
  ParPut(m: GenericRequest<CrmParPutInModel>) { return this.post<CrmParPutInModel, CrmParPutOutModel>('Crm_Config', 'Crm_ParPut', m); }
  ParDelete(m: GenericRequest<CrmParDeleteInModel>) { return this.post<CrmParDeleteInModel, CrmIdOutModel>('Crm_Config', 'Crm_ParDelete', m); }
  Riassegna(m: GenericRequest<CrmRiassegnaInModel>) { return this.post<CrmRiassegnaInModel, CrmRiassegnaOutModel>('Crm_Config', 'Crm_Riassegna', m); }

  // clienti
  ClienteList(m: GenericRequest<CrmClienteListInModel>) { return this.post<CrmClienteListInModel, CrmClienteListOutModel>('Crm_Cliente', 'Crm_ClienteList', m); }
  ClienteGet(m: GenericRequest<CrmIdInModel>) { return this.post<CrmIdInModel, CrmClienteGetOutModel>('Crm_Cliente', 'Crm_ClienteGet', m); }
  ClientePut(m: GenericRequest<CrmClientePutInModel>) { return this.post<CrmClientePutInModel, CrmClientePutOutModel>('Crm_Cliente', 'Crm_ClientePut', m); }
  ContattoPut(m: GenericRequest<CrmContattoPutInModel>) { return this.post<CrmContattoPutInModel, CrmContattoPutOutModel>('Crm_Cliente', 'Crm_ContattoPut', m); }
  ContattoDelete(m: GenericRequest<CrmIdInModel>) { return this.post<CrmIdInModel, CrmIdOutModel>('Crm_Cliente', 'Crm_ContattoDelete', m); }
  ClienteExport(m: GenericRequest<CrmClienteListInModel>): Promise<void> {
    return this.scarica(this.http.post(environment.remoteData.apiUri + 'Crm_Cliente/Crm_ClienteExport', m, { responseType: 'blob', observe: 'response' }), 'clienti.xlsx');
  }

  // lead
  LeadList(m: GenericRequest<CrmLeadListInModel>) { return this.post<CrmLeadListInModel, CrmLeadListOutModel>('Crm_Lead', 'Crm_LeadList', m); }
  LeadGet(m: GenericRequest<CrmIdInModel>) { return this.post<CrmIdInModel, CrmLeadGetOutModel>('Crm_Lead', 'Crm_LeadGet', m); }
  LeadPut(m: GenericRequest<CrmLeadPutInModel>) { return this.post<CrmLeadPutInModel, CrmLeadPutOutModel>('Crm_Lead', 'Crm_LeadPut', m); }
  LeadDelete(m: GenericRequest<CrmIdInModel>) { return this.post<CrmIdInModel, CrmIdOutModel>('Crm_Lead', 'Crm_LeadDelete', m); }
  LeadConverti(m: GenericRequest<CrmLeadConvertiInModel>) { return this.post<CrmLeadConvertiInModel, CrmLeadConvertiOutModel>('Crm_Lead', 'Crm_LeadConverti', m); }

  // opportunita'
  OpportunitaList(m: GenericRequest<CrmOpportunitaListInModel>) { return this.post<CrmOpportunitaListInModel, CrmOpportunitaListOutModel>('Crm_Opportunita', 'Crm_OpportunitaList', m); }
  OpportunitaGet(m: GenericRequest<CrmIdInModel>) { return this.post<CrmIdInModel, CrmOpportunitaGetOutModel>('Crm_Opportunita', 'Crm_OpportunitaGet', m); }
  OpportunitaPut(m: GenericRequest<CrmOpportunitaPutInModel>) { return this.post<CrmOpportunitaPutInModel, CrmOpportunitaPutOutModel>('Crm_Opportunita', 'Crm_OpportunitaPut', m); }
  OpportunitaCambiaFase(m: GenericRequest<CrmCambiaFaseInModel>) { return this.post<CrmCambiaFaseInModel, CrmOpportunitaPutOutModel>('Crm_Opportunita', 'Crm_OpportunitaCambiaFase', m); }
  OpportunitaDelete(m: GenericRequest<CrmIdInModel>) { return this.post<CrmIdInModel, CrmIdOutModel>('Crm_Opportunita', 'Crm_OpportunitaDelete', m); }

  // attivita'
  AttivitaList(m: GenericRequest<CrmAttivitaListInModel>) { return this.post<CrmAttivitaListInModel, CrmAttivitaListOutModel>('Crm_Attivita', 'Crm_AttivitaList', m); }
  AttivitaGet(m: GenericRequest<CrmIdInModel>) { return this.post<CrmIdInModel, CrmAttivitaGetOutModel>('Crm_Attivita', 'Crm_AttivitaGet', m); }
  AttivitaPut(m: GenericRequest<CrmAttivitaPutInModel>) { return this.post<CrmAttivitaPutInModel, CrmAttivitaPutOutModel>('Crm_Attivita', 'Crm_AttivitaPut', m); }
  AttivitaCompleta(m: GenericRequest<CrmAttivitaCompletaInModel>) { return this.post<CrmAttivitaCompletaInModel, CrmAttivitaPutOutModel>('Crm_Attivita', 'Crm_AttivitaCompleta', m); }
  AttivitaDelete(m: GenericRequest<CrmIdInModel>) { return this.post<CrmIdInModel, CrmIdOutModel>('Crm_Attivita', 'Crm_AttivitaDelete', m); }

  // offerte
  OffertaList(m: GenericRequest<CrmOffertaListInModel>) { return this.post<CrmOffertaListInModel, CrmOffertaListOutModel>('Crm_Offerta', 'Crm_OffertaList', m); }
  OffertaGet(m: GenericRequest<CrmIdInModel>) { return this.post<CrmIdInModel, CrmOffertaGetOutModel>('Crm_Offerta', 'Crm_OffertaGet', m); }
  OffertaPut(m: GenericRequest<CrmOffertaPutInModel>) { return this.post<CrmOffertaPutInModel, CrmOffertaPutOutModel>('Crm_Offerta', 'Crm_OffertaPut', m); }
  OffertaDelete(m: GenericRequest<CrmIdInModel>) { return this.post<CrmIdInModel, CrmIdOutModel>('Crm_Offerta', 'Crm_OffertaDelete', m); }
  OffertaInvia(m: GenericRequest<CrmOffertaInviaInModel>) { return this.post<CrmOffertaInviaInModel, CrmOffertaPutOutModel>('Crm_Offerta', 'Crm_OffertaInvia', m); }
  OffertaPdf(id: number): Promise<void> {
    return this.scarica(this.http.get(environment.remoteData.apiUri + 'Crm_Offerta/Crm_OffertaPdf?id=' + id, { responseType: 'blob', observe: 'response' }), 'offerta.pdf');
  }

  // note e allegati
  NotaPut(m: GenericRequest<CrmNotaPutInModel>) { return this.post<CrmNotaPutInModel, CrmNotaPutOutModel>('Crm_NotaAllegato', 'Crm_NotaPut', m); }
  NotaDelete(m: GenericRequest<CrmIdInModel>) { return this.post<CrmIdInModel, CrmIdOutModel>('Crm_NotaAllegato', 'Crm_NotaDelete', m); }
  AllegatoDelete(m: GenericRequest<CrmIdInModel>) { return this.post<CrmIdInModel, CrmIdOutModel>('Crm_NotaAllegato', 'Crm_AllegatoDelete', m); }
  AllegatoUpload(data: CrmAllegatoUploadModel, file: File): Observable<GenericResult<CrmAllegatoUploadOutModel>> {
    const form = new FormData();
    form.append('data', JSON.stringify(data));
    form.append('file', file, file.name);
    return this.http.post<GenericResult<CrmAllegatoUploadOutModel>>(environment.remoteData.apiUri + 'Crm_NotaAllegato/Crm_AllegatoUpload', form);
  }
  AllegatoDownload(id: number): Promise<void> {
    return this.scarica(this.http.get(environment.remoteData.apiUri + 'Crm_NotaAllegato/Crm_AllegatoDownload?id=' + id, { responseType: 'blob', observe: 'response' }), 'allegato');
  }

  // report
  Report(m: GenericRequest<CrmReportInModel>) { return this.post<CrmReportInModel, CrmReportOutModel>('Crm_Report', 'Crm_Report', m); }

  // salva il file ricevuto con il suo nome; gli errori (messaggio nel corpo) si mostrano come gli altri
  private async scarica(richiesta: Observable<HttpResponse<Blob>>, nomePredefinito: string): Promise<void> {
    try {
      const res = await firstValueFrom(richiesta);
      const nome = this.fileName(res.headers.get('Content-Disposition')) ?? nomePredefinito;
      const link = document.createElement('a');
      link.href = URL.createObjectURL(res.body!);
      link.download = nome;
      link.click();
      setTimeout(() => URL.revokeObjectURL(link.href), 10000);
    }
    catch (error: any) {
      const testo = error?.error instanceof Blob ? await error.error.text() : String(error?.message ?? error);
      this.nvxHttpInterceptorService.HttpError_Add({ success: false, data: null, token: null, messages: [{ text: testo, msgType: 0 }] } as any);
    }
  }

  private fileName(disposition: string | null): string | null {
    if (!disposition) return null;
    const utf = /filename\*=UTF-8''([^;]+)/i.exec(disposition);
    if (utf) return decodeURIComponent(utf[1]);
    const plain = /filename="?([^";]+)"?/i.exec(disposition);
    return plain ? plain[1] : null;
  }
}
