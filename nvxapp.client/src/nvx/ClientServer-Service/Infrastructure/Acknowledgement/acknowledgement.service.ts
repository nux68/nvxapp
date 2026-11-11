import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { AcknowledgementConfirmInModel, AcknowledgementConfirmOutModel, AcknowledgementListInModel, AcknowledgementListOutModel } from './Models/acknowledgement-model';


// Prese visione dell'utente corrente (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.6)
@Injectable({
  providedIn: 'root'
})
export class AcknowledgementService {

  constructor(private http: HttpClient) { }

  AcknowledgementList(model: GenericRequest<AcknowledgementListInModel>): Observable<GenericResult<AcknowledgementListOutModel>> {
    return this.http.post<GenericResult<AcknowledgementListOutModel>>(environment.remoteData.apiUri + 'Acknowledgement/AcknowledgementList', model);
  }

  AcknowledgementConfirm(model: GenericRequest<AcknowledgementConfirmInModel>): Observable<GenericResult<AcknowledgementConfirmOutModel>> {
    return this.http.post<GenericResult<AcknowledgementConfirmOutModel>>(environment.remoteData.apiUri + 'Acknowledgement/AcknowledgementConfirm', model);
  }
}
