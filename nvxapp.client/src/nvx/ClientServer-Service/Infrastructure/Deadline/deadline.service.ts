import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  DeadlineCloseInModel, DeadlineCloseOutModel, DeadlineDeleteInModel, DeadlineDeleteOutModel, DeadlineGetInModel, DeadlineGetOutModel,
  DeadlineListInModel, DeadlineListOutModel, DeadlinePutInModel, DeadlinePutOutModel, DeadlineSummaryInModel, DeadlineSummaryOutModel
} from './Models/deadline-model';


// Scadenzario dell'azienda corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.1-3)
@Injectable({
  providedIn: 'root'
})
export class DeadlineService {

  constructor(private http: HttpClient) { }

  DeadlineList(model: GenericRequest<DeadlineListInModel>): Observable<GenericResult<DeadlineListOutModel>> {
    return this.http.post<GenericResult<DeadlineListOutModel>>(environment.remoteData.apiUri + 'Deadline/DeadlineList', model);
  }

  DeadlineGet(model: GenericRequest<DeadlineGetInModel>): Observable<GenericResult<DeadlineGetOutModel>> {
    return this.http.post<GenericResult<DeadlineGetOutModel>>(environment.remoteData.apiUri + 'Deadline/DeadlineGet', model);
  }

  DeadlinePut(model: GenericRequest<DeadlinePutInModel>): Observable<GenericResult<DeadlinePutOutModel>> {
    return this.http.post<GenericResult<DeadlinePutOutModel>>(environment.remoteData.apiUri + 'Deadline/DeadlinePut', model);
  }

  DeadlineClose(model: GenericRequest<DeadlineCloseInModel>): Observable<GenericResult<DeadlineCloseOutModel>> {
    return this.http.post<GenericResult<DeadlineCloseOutModel>>(environment.remoteData.apiUri + 'Deadline/DeadlineClose', model);
  }

  DeadlineDelete(model: GenericRequest<DeadlineDeleteInModel>): Observable<GenericResult<DeadlineDeleteOutModel>> {
    return this.http.post<GenericResult<DeadlineDeleteOutModel>>(environment.remoteData.apiUri + 'Deadline/DeadlineDelete', model);
  }

  DeadlineSummary(model: GenericRequest<DeadlineSummaryInModel>): Observable<GenericResult<DeadlineSummaryOutModel>> {
    return this.http.post<GenericResult<DeadlineSummaryOutModel>>(environment.remoteData.apiUri + 'Deadline/DeadlineSummary', model);
  }
}
