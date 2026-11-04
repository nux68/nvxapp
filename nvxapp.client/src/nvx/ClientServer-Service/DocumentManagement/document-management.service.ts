import { Injectable } from '@angular/core';
import { HttpClient, HttpResponse } from '@angular/common/http';
import { Observable, firstValueFrom } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { NvxHttpInterceptorService } from '../../http/http-interceptor';
import {
  DocAbilitazioneListOutModel, DocAbilitazionePutInModel, DocAbilitazionePutOutModel, DocCartellaPutInModel, DocCartellaPutOutModel,
  DocCfgPutInModel, DocCfgPutOutModel, DocClienteListOutModel, DocDeleteOutModel, DocDipendentePutInModel, DocDipendentePutOutModel,
  DocDocumentoEditModel, DocDocumentoGetOutModel, DocDocumentoListInModel, DocDocumentoListOutModel, DocDocumentoPutInModel,
  DocDocumentoPutOutModel, DocEmptyInModel, DocIdInModel, DocIdOutModel, DocLogListInModel, DocLogListOutModel, DocSetupGetOutModel,
  DocSvuotaCestinoOutModel, DocTipoPutInModel, DocTipoPutOutModel
} from './Models/doc-model';

/*
 Documentale. Caricamento multipart (dati JSON + file) e download in streaming (blob): i documenti arrivano a 50 MB
 e oltre, il base64 nel JSON li gonfierebbe. Gli errori del download (404 con il messaggio) si mostrano come gli altri.
*/
@Injectable({
  providedIn: 'root'
})
export class DocumentManagementService {

  constructor(private http: HttpClient, private nvxHttpInterceptorService: NvxHttpInterceptorService) { }

  private post<I, O>(api: string, action: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + api + '/' + action, model);
  }

  // configurazione
  SetupGet(m: GenericRequest<DocEmptyInModel>) { return this.post<DocEmptyInModel, DocSetupGetOutModel>('Doc_Config', 'Doc_SetupGet', m); }
  CfgPut(m: GenericRequest<DocCfgPutInModel>) { return this.post<DocCfgPutInModel, DocCfgPutOutModel>('Doc_Config', 'Doc_CfgPut', m); }
  TipoPut(m: GenericRequest<DocTipoPutInModel>) { return this.post<DocTipoPutInModel, DocTipoPutOutModel>('Doc_Config', 'Doc_TipoPut', m); }
  TipoDelete(m: GenericRequest<DocIdInModel>) { return this.post<DocIdInModel, DocDeleteOutModel>('Doc_Config', 'Doc_TipoDelete', m); }
  CartellaPut(m: GenericRequest<DocCartellaPutInModel>) { return this.post<DocCartellaPutInModel, DocCartellaPutOutModel>('Doc_Config', 'Doc_CartellaPut', m); }
  CartellaDelete(m: GenericRequest<DocIdInModel>) { return this.post<DocIdInModel, DocDeleteOutModel>('Doc_Config', 'Doc_CartellaDelete', m); }
  AbilitazioneList(m: GenericRequest<DocEmptyInModel>) { return this.post<DocEmptyInModel, DocAbilitazioneListOutModel>('Doc_Config', 'Doc_AbilitazioneList', m); }
  AbilitazionePut(m: GenericRequest<DocAbilitazionePutInModel>) { return this.post<DocAbilitazionePutInModel, DocAbilitazionePutOutModel>('Doc_Config', 'Doc_AbilitazionePut', m); }
  DipendentePut(m: GenericRequest<DocDipendentePutInModel>) { return this.post<DocDipendentePutInModel, DocDipendentePutOutModel>('Doc_Config', 'Doc_DipendentePut', m); }

  // clienti comuni (API di Infrastructure)
  ClienteList() { return this.post<DocEmptyInModel, DocClienteListOutModel>('Az_Cliente', 'GetAll', new GenericRequest<DocEmptyInModel>(DocEmptyInModel)); }

  // documenti
  List(m: GenericRequest<DocDocumentoListInModel>) { return this.post<DocDocumentoListInModel, DocDocumentoListOutModel>('Doc_Documento', 'Doc_DocumentoList', m); }
  Get(m: GenericRequest<DocIdInModel>) { return this.post<DocIdInModel, DocDocumentoGetOutModel>('Doc_Documento', 'Doc_DocumentoGet', m); }
  Put(m: GenericRequest<DocDocumentoPutInModel>) { return this.post<DocDocumentoPutInModel, DocDocumentoPutOutModel>('Doc_Documento', 'Doc_DocumentoPut', m); }
  Cestino(m: GenericRequest<DocIdInModel>) { return this.post<DocIdInModel, DocIdOutModel>('Doc_Documento', 'Doc_DocumentoCestino', m); }
  Ripristina(m: GenericRequest<DocIdInModel>) { return this.post<DocIdInModel, DocIdOutModel>('Doc_Documento', 'Doc_DocumentoRipristina', m); }
  Elimina(m: GenericRequest<DocIdInModel>) { return this.post<DocIdInModel, DocIdOutModel>('Doc_Documento', 'Doc_DocumentoElimina', m); }
  SvuotaCestino() { return this.post<DocEmptyInModel, DocSvuotaCestinoOutModel>('Doc_Documento', 'Doc_SvuotaCestino', new GenericRequest<DocEmptyInModel>(DocEmptyInModel)); }
  PresaVisione(m: GenericRequest<DocIdInModel>) { return this.post<DocIdInModel, DocIdOutModel>('Doc_Documento', 'Doc_DocumentoPresaVisione', m); }
  LogList(m: GenericRequest<DocLogListInModel>) { return this.post<DocLogListInModel, DocLogListOutModel>('Doc_Documento', 'Doc_LogList', m); }

  Upload(data: DocDocumentoEditModel, file: File): Observable<GenericResult<DocDocumentoPutOutModel>> {
    const form = new FormData();
    form.append('data', JSON.stringify(data));
    form.append('file', file, file.name);
    return this.http.post<GenericResult<DocDocumentoPutOutModel>>(environment.remoteData.apiUri + 'Doc_Documento/Doc_DocumentoUpload', form);
  }

  NewVersion(id: number, file: File): Observable<GenericResult<DocDocumentoPutOutModel>> {
    const form = new FormData();
    form.append('id', String(id));
    form.append('file', file, file.name);
    return this.http.post<GenericResult<DocDocumentoPutOutModel>>(environment.remoteData.apiUri + 'Doc_Documento/Doc_DocumentoNewVersion', form);
  }

  // scarica il file (versione corrente se non indicata) e lo salva con il suo nome
  async Download(id: number, versione: number | null = null): Promise<void> {
    let url = environment.remoteData.apiUri + 'Doc_Documento/Doc_DocumentoDownload?id=' + id;
    if (versione) url += '&versione=' + versione;
    try {
      const res: HttpResponse<Blob> = await firstValueFrom(this.http.get(url, { responseType: 'blob', observe: 'response' }));
      const nome = this.fileName(res.headers.get('Content-Disposition')) ?? 'documento';
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
