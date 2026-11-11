import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  DigestGenerateInModel, DigestGenerateOutModel, DigestGetInModel, DigestGetOutModel, DigestListInModel, DigestListOutModel
} from './Models/digest-model';


// Riepiloghi periodici dell'utente corrente e "Genera ora" (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-AI.3)
@Injectable({
  providedIn: 'root'
})
export class DigestService {

  constructor(private http: HttpClient) { }

  DigestList(model: GenericRequest<DigestListInModel>): Observable<GenericResult<DigestListOutModel>> {
    return this.http.post<GenericResult<DigestListOutModel>>(environment.remoteData.apiUri + 'Digest/DigestList', model);
  }

  DigestGet(model: GenericRequest<DigestGetInModel>): Observable<GenericResult<DigestGetOutModel>> {
    return this.http.post<GenericResult<DigestGetOutModel>>(environment.remoteData.apiUri + 'Digest/DigestGet', model);
  }

  DigestGenerate(model: GenericRequest<DigestGenerateInModel>): Observable<GenericResult<DigestGenerateOutModel>> {
    return this.http.post<GenericResult<DigestGenerateOutModel>>(environment.remoteData.apiUri + 'Digest/DigestGenerate', model);
  }
}
