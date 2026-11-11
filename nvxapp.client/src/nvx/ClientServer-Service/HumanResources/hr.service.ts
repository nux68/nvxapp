import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { HrCfgGetOutModel, HrCfgPutInModel, HrCfgPutOutModel, HrEmptyInModel } from './Models/hr-model';

/*
 Risorse Umane (ANALISI_HUMANRESOURCES_APPLICATIVO.md): per ora la sola configurazione del modulo.
*/
@Injectable({
  providedIn: 'root'
})
export class HrService {

  constructor(private http: HttpClient) { }

  private post<I, O>(api: string, action: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + api + '/' + action, model);
  }

  // configurazione
  CfgGet(m: GenericRequest<HrEmptyInModel>) { return this.post<HrEmptyInModel, HrCfgGetOutModel>('Hr_Config', 'Hr_CfgGet', m); }
  CfgPut(m: GenericRequest<HrCfgPutInModel>) { return this.post<HrCfgPutInModel, HrCfgPutOutModel>('Hr_Config', 'Hr_CfgPut', m); }
}
