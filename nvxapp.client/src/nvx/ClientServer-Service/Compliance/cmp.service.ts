import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { CmpCfgGetOutModel, CmpCfgPutInModel, CmpCfgPutOutModel, CmpEmptyInModel } from './Models/cmp-model';

/*
 Compliance (ANALISI_COMPLIANCE_APPLICATIVO.md): per ora la sola configurazione del modulo.
*/
@Injectable({
  providedIn: 'root'
})
export class CmpService {

  constructor(private http: HttpClient) { }

  private post<I, O>(api: string, action: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + api + '/' + action, model);
  }

  // configurazione
  CfgGet(m: GenericRequest<CmpEmptyInModel>) { return this.post<CmpEmptyInModel, CmpCfgGetOutModel>('Cmp_Config', 'Cmp_CfgGet', m); }
  CfgPut(m: GenericRequest<CmpCfgPutInModel>) { return this.post<CmpCfgPutInModel, CmpCfgPutOutModel>('Cmp_Config', 'Cmp_CfgPut', m); }
}
