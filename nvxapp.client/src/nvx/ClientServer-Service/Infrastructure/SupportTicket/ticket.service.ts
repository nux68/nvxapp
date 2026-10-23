import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  TicketActionInModel, TicketActionOutModel, TicketAttachGetInModel, TicketAttachGetOutModel, TicketCountInModel, TicketCountOutModel,
  TicketGetInModel, TicketGetOutModel, TicketListInModel, TicketListOutModel, TicketOpenInfoInModel, TicketOpenInfoOutModel,
  TicketOpenInModel, TicketOpenOutModel
} from './Models/ticket-model';


// Ticket di assistenza: elenchi, dettaglio, apertura, passaggi, allegati
@Injectable({
  providedIn: 'root'
})
export class TicketService {

  constructor(private http: HttpClient) { }

  TicketList(model: GenericRequest<TicketListInModel>): Observable<GenericResult<TicketListOutModel>> {
    return this.http.post<GenericResult<TicketListOutModel>>(environment.remoteData.apiUri + 'Ticket/TicketList', model);
  }

  TicketCount(model: GenericRequest<TicketCountInModel>): Observable<GenericResult<TicketCountOutModel>> {
    return this.http.post<GenericResult<TicketCountOutModel>>(environment.remoteData.apiUri + 'Ticket/TicketCount', model);
  }

  TicketGet(model: GenericRequest<TicketGetInModel>): Observable<GenericResult<TicketGetOutModel>> {
    return this.http.post<GenericResult<TicketGetOutModel>>(environment.remoteData.apiUri + 'Ticket/TicketGet', model);
  }

  TicketOpenInfo(model: GenericRequest<TicketOpenInfoInModel>): Observable<GenericResult<TicketOpenInfoOutModel>> {
    return this.http.post<GenericResult<TicketOpenInfoOutModel>>(environment.remoteData.apiUri + 'Ticket/TicketOpenInfo', model);
  }

  TicketOpen(model: GenericRequest<TicketOpenInModel>): Observable<GenericResult<TicketOpenOutModel>> {
    return this.http.post<GenericResult<TicketOpenOutModel>>(environment.remoteData.apiUri + 'Ticket/TicketOpen', model);
  }

  TicketExecute(model: GenericRequest<TicketActionInModel>): Observable<GenericResult<TicketActionOutModel>> {
    return this.http.post<GenericResult<TicketActionOutModel>>(environment.remoteData.apiUri + 'Ticket/TicketExecute', model);
  }

  TicketAttachGet(model: GenericRequest<TicketAttachGetInModel>): Observable<GenericResult<TicketAttachGetOutModel>> {
    return this.http.post<GenericResult<TicketAttachGetOutModel>>(environment.remoteData.apiUri + 'Ticket/TicketAttachGet', model);
  }
}
