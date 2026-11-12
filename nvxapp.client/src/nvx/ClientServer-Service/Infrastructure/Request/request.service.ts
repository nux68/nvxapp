import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { RequestActionInModel, RequestActionOutModel, RequestCountInModel, RequestCountOutModel, RequestListInModel, RequestListOutModel } from './Models/request-model';


// Richieste da approvare comuni a tutti gli applicativi (ANALISI_INFRASTRUCTURE_RICHIESTE.md)
@Injectable({
  providedIn: 'root'
})
export class RequestService {

  constructor(private http: HttpClient) { }

  RequestCount(model: GenericRequest<RequestCountInModel>): Observable<GenericResult<RequestCountOutModel>> {
    return this.http.post<GenericResult<RequestCountOutModel>>(environment.remoteData.apiUri + 'Request/RequestCount', model);
  }

  RequestList(model: GenericRequest<RequestListInModel>): Observable<GenericResult<RequestListOutModel>> {
    return this.http.post<GenericResult<RequestListOutModel>>(environment.remoteData.apiUri + 'Request/RequestList', model);
  }

  RequestApprove(model: GenericRequest<RequestActionInModel>): Observable<GenericResult<RequestActionOutModel>> {
    return this.http.post<GenericResult<RequestActionOutModel>>(environment.remoteData.apiUri + 'Request/RequestApprove', model);
  }

  RequestReject(model: GenericRequest<RequestActionInModel>): Observable<GenericResult<RequestActionOutModel>> {
    return this.http.post<GenericResult<RequestActionOutModel>>(environment.remoteData.apiUri + 'Request/RequestReject', model);
  }

  RequestCancel(model: GenericRequest<RequestActionInModel>): Observable<GenericResult<RequestActionOutModel>> {
    return this.http.post<GenericResult<RequestActionOutModel>>(environment.remoteData.apiUri + 'Request/RequestCancel', model);
  }
}
