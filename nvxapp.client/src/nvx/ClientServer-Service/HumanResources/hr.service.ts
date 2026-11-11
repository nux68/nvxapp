import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { firstValueFrom, Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { NvxHttpInterceptorService } from '../../http/http-interceptor';
import * as M from './Models/hr-model';

/*
 Risorse Umane (ANALISI_HUMANRESOURCES_APPLICATIVO.md): configurazione, dipendenti, cedolini e CU (anche lato studio con idCompany),
 formazione, onboarding e offboarding. I file (PDF dei cedolini, attestati, documenti delle checklist) si caricano in multipart
 e si scaricano con GET, come gli allegati del CRM; le elaborazioni lunghe (divisione e pubblicazione) sono job del server.
*/
@Injectable({
  providedIn: 'root'
})
export class HrService {

  constructor(private http: HttpClient, private nvxHttpInterceptorService: NvxHttpInterceptorService) { }

  private post<I, O>(api: string, action: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + api + '/' + action, model);
  }

  private upload<O>(api: string, action: string, data: any, file: File): Observable<GenericResult<O>> {
    const form = new FormData();
    form.append('data', JSON.stringify(data));
    form.append('file', file, file.name);
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + api + '/' + action, form);
  }

  private download(api: string, action: string, query: string, nomePredefinito: string): Promise<void> {
    return this.scarica(this.http.get(environment.remoteData.apiUri + api + '/' + action + '?' + query, { responseType: 'blob', observe: 'response' }), nomePredefinito);
  }

  // configurazione
  CfgGet(m: GenericRequest<M.HrEmptyInModel>) { return this.post<M.HrEmptyInModel, M.HrCfgGetOutModel>('Hr_Config', 'Hr_CfgGet', m); }
  CfgPut(m: GenericRequest<M.HrCfgPutInModel>) { return this.post<M.HrCfgPutInModel, M.HrCfgPutOutModel>('Hr_Config', 'Hr_CfgPut', m); }

  // dipendenti e account
  DipendenteList(m: GenericRequest<M.HrDipendenteListInModel>) { return this.post<M.HrDipendenteListInModel, M.HrDipendenteListOutModel>('Hr_Dipendente', 'Hr_DipendenteList', m); }
  DipendenteGet(m: GenericRequest<M.HrIdInModel>) { return this.post<M.HrIdInModel, M.HrDipendenteGetOutModel>('Hr_Dipendente', 'Hr_DipendenteGet', m); }
  DipendentePut(m: GenericRequest<M.HrDipendentePutInModel>) { return this.post<M.HrDipendentePutInModel, M.HrDipendentePutOutModel>('Hr_Dipendente', 'Hr_DipendentePut', m); }
  AccountCrea(m: GenericRequest<M.HrAccountCreaInModel>) { return this.post<M.HrAccountCreaInModel, M.HrAccountOutModel>('Hr_Dipendente', 'Hr_AccountCrea', m); }
  AccountDisattiva(m: GenericRequest<M.HrAccountDisattivaInModel>) { return this.post<M.HrAccountDisattivaInModel, M.HrAccountOutModel>('Hr_Dipendente', 'Hr_AccountDisattiva', m); }

  // cedolini e CU
  StudioAziende(m: GenericRequest<M.HrEmptyInModel>) { return this.post<M.HrEmptyInModel, M.HrStudioAziendeOutModel>('Hr_Cedolino', 'Hr_StudioAziende', m); }
  CaricamentoList(m: GenericRequest<M.HrCaricamentoListInModel>) { return this.post<M.HrCaricamentoListInModel, M.HrCaricamentoListOutModel>('Hr_Cedolino', 'Hr_CaricamentoList', m); }
  CaricamentoGet(m: GenericRequest<M.HrCedIdInModel>) { return this.post<M.HrCedIdInModel, M.HrCaricamentoGetOutModel>('Hr_Cedolino', 'Hr_CaricamentoGet', m); }
  CaricamentoUpload(data: M.HrCaricamentoUploadModel, file: File) { return this.upload<M.HrCaricamentoUploadOutModel>('Hr_Cedolino', 'Hr_CaricamentoUpload', data, file); }
  ScartoAssegna(m: GenericRequest<M.HrScartoAssegnaInModel>) { return this.post<M.HrScartoAssegnaInModel, M.HrCaricamentoGetOutModel>('Hr_Cedolino', 'Hr_ScartoAssegna', m); }
  ScartoIgnora(m: GenericRequest<M.HrCedIdInModel>) { return this.post<M.HrCedIdInModel, M.HrCaricamentoGetOutModel>('Hr_Cedolino', 'Hr_ScartoIgnora', m); }
  CaricamentoPubblica(m: GenericRequest<M.HrCedIdInModel>) { return this.post<M.HrCedIdInModel, M.HrJobOutModel>('Hr_Cedolino', 'Hr_CaricamentoPubblica', m); }
  CaricamentoDelete(m: GenericRequest<M.HrCedIdInModel>) { return this.post<M.HrCedIdInModel, M.HrIdOutModel>('Hr_Cedolino', 'Hr_CaricamentoDelete', m); }
  DocDownload(id: number, idCompany: number | null): Promise<void> {
    return this.download('Hr_Cedolino', 'Hr_DocDownload', 'id=' + id + (idCompany ? '&idCompany=' + idCompany : ''), 'documento.pdf');
  }
  MieiDocumenti(m: GenericRequest<M.HrEmptyInModel>) { return this.post<M.HrEmptyInModel, M.HrMieiDocumentiOutModel>('Hr_Cedolino', 'Hr_MieiDocumenti', m); }
  MioDocDownload(id: number): Promise<void> { return this.download('Hr_Cedolino', 'Hr_MioDocDownload', 'id=' + id, 'documento.pdf'); }

  // formazione
  CorsoList(m: GenericRequest<M.HrEmptyInModel>) { return this.post<M.HrEmptyInModel, M.HrCorsoListOutModel>('Hr_Formazione', 'Hr_CorsoList', m); }
  CorsoGet(m: GenericRequest<M.HrIdInModel>) { return this.post<M.HrIdInModel, M.HrCorsoGetOutModel>('Hr_Formazione', 'Hr_CorsoGet', m); }
  CorsoPut(m: GenericRequest<M.HrCorsoPutInModel>) { return this.post<M.HrCorsoPutInModel, M.HrCorsoPutOutModel>('Hr_Formazione', 'Hr_CorsoPut', m); }
  CorsoDelete(m: GenericRequest<M.HrIdInModel>) { return this.post<M.HrIdInModel, M.HrIdOutModel>('Hr_Formazione', 'Hr_CorsoDelete', m); }
  EdizioneGet(m: GenericRequest<M.HrEdizioneGetInModel>) { return this.post<M.HrEdizioneGetInModel, M.HrEdizioneGetOutModel>('Hr_Formazione', 'Hr_EdizioneGet', m); }
  EdizionePut(m: GenericRequest<M.HrEdizionePutInModel>) { return this.post<M.HrEdizionePutInModel, M.HrEdizionePutOutModel>('Hr_Formazione', 'Hr_EdizionePut', m); }
  EdizioneDelete(m: GenericRequest<M.HrIdInModel>) { return this.post<M.HrIdInModel, M.HrIdOutModel>('Hr_Formazione', 'Hr_EdizioneDelete', m); }
  AttestatoUpload(data: M.HrAttestatoUploadModel, file: File) { return this.upload<M.HrAttestatoUploadOutModel>('Hr_Formazione', 'Hr_AttestatoUpload', data, file); }
  AttestatoDownload(id: number): Promise<void> { return this.download('Hr_Formazione', 'Hr_AttestatoDownload', 'id=' + id, 'attestato'); }
  PianoList(m: GenericRequest<M.HrPianoListInModel>) { return this.post<M.HrPianoListInModel, M.HrPianoListOutModel>('Hr_Formazione', 'Hr_PianoList', m); }
  PianoGet(m: GenericRequest<M.HrIdInModel>) { return this.post<M.HrIdInModel, M.HrPianoGetOutModel>('Hr_Formazione', 'Hr_PianoGet', m); }
  PianoPut(m: GenericRequest<M.HrPianoPutInModel>) { return this.post<M.HrPianoPutInModel, M.HrPianoPutOutModel>('Hr_Formazione', 'Hr_PianoPut', m); }
  PianoDelete(m: GenericRequest<M.HrIdInModel>) { return this.post<M.HrIdInModel, M.HrIdOutModel>('Hr_Formazione', 'Hr_PianoDelete', m); }

  // onboarding e offboarding
  ModelloList(m: GenericRequest<M.HrEmptyInModel>) { return this.post<M.HrEmptyInModel, M.HrModelloListOutModel>('Hr_Onboarding', 'Hr_ModelloList', m); }
  ModelloGet(m: GenericRequest<M.HrIdInModel>) { return this.post<M.HrIdInModel, M.HrModelloGetOutModel>('Hr_Onboarding', 'Hr_ModelloGet', m); }
  ModelloPut(m: GenericRequest<M.HrModelloPutInModel>) { return this.post<M.HrModelloPutInModel, M.HrModelloPutOutModel>('Hr_Onboarding', 'Hr_ModelloPut', m); }
  ModelloDelete(m: GenericRequest<M.HrIdInModel>) { return this.post<M.HrIdInModel, M.HrIdOutModel>('Hr_Onboarding', 'Hr_ModelloDelete', m); }
  ChecklistList(m: GenericRequest<M.HrChecklistListInModel>) { return this.post<M.HrChecklistListInModel, M.HrChecklistListOutModel>('Hr_Onboarding', 'Hr_ChecklistList', m); }
  ChecklistGet(m: GenericRequest<M.HrIdInModel>) { return this.post<M.HrIdInModel, M.HrChecklistGetOutModel>('Hr_Onboarding', 'Hr_ChecklistGet', m); }
  ChecklistCrea(m: GenericRequest<M.HrChecklistCreaInModel>) { return this.post<M.HrChecklistCreaInModel, M.HrChecklistGetOutModel>('Hr_Onboarding', 'Hr_ChecklistCrea', m); }
  ChecklistPut(m: GenericRequest<M.HrChecklistPutInModel>) { return this.post<M.HrChecklistPutInModel, M.HrChecklistGetOutModel>('Hr_Onboarding', 'Hr_ChecklistPut', m); }
  ChecklistDelete(m: GenericRequest<M.HrIdInModel>) { return this.post<M.HrIdInModel, M.HrIdOutModel>('Hr_Onboarding', 'Hr_ChecklistDelete', m); }
  ChecklistVocePut(m: GenericRequest<M.HrChecklistVocePutInModel>) { return this.post<M.HrChecklistVocePutInModel, M.HrChecklistVoceOutModel>('Hr_Onboarding', 'Hr_ChecklistVocePut', m); }
  ChecklistVocePresaVisione(m: GenericRequest<M.HrIdInModel>) { return this.post<M.HrIdInModel, M.HrChecklistVoceOutModel>('Hr_Onboarding', 'Hr_ChecklistVocePresaVisione', m); }
  ChecklistVoceDocUpload(data: M.HrVoceDocUploadModel, file: File) { return this.upload<M.HrChecklistVoceOutModel>('Hr_Onboarding', 'Hr_ChecklistVoceDocUpload', data, file); }
  ChecklistVoceDocDownload(id: number): Promise<void> { return this.download('Hr_Onboarding', 'Hr_ChecklistVoceDocDownload', 'id=' + id, 'documento'); }

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

// ---------------------------------------------------------------- date (il server manda "aaaa-mm-ggT..." senza fuso)
export function hrData(d: string | null | undefined): string {
  if (!d) return '';
  const [a, m, g] = d.substring(0, 10).split('-');
  return g && m && a ? `${g}/${m}/${a}` : '';
}

// valore per <ion-input type="date"> e per il server
export function hrPerInput(d: string | null | undefined): string | null {
  return d ? d.substring(0, 10) : null;
}

export function hrOggi(): string {
  const d = new Date();
  return `${d.getFullYear()}-${String(d.getMonth() + 1).padStart(2, '0')}-${String(d.getDate()).padStart(2, '0')}`;
}
