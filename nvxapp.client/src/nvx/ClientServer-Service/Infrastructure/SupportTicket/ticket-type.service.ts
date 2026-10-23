import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  TicketTypeGetInModel, TicketTypeGetOutModel, TicketTypeListInModel, TicketTypeListOutModel, TicketTypePutInModel, TicketTypePutOutModel
} from './Models/ticket-type-model';


// Tipi di richiesta dei ticket (SuperUser)
@Injectable({
  providedIn: 'root'
})
export class TicketTypeService {

  constructor(private http: HttpClient) { }

  TicketTypeList(model: GenericRequest<TicketTypeListInModel>): Observable<GenericResult<TicketTypeListOutModel>> {
    return this.http.post<GenericResult<TicketTypeListOutModel>>(environment.remoteData.apiUri + 'TicketType/TicketTypeList', model);
  }

  TicketTypeGet(model: GenericRequest<TicketTypeGetInModel>): Observable<GenericResult<TicketTypeGetOutModel>> {
    return this.http.post<GenericResult<TicketTypeGetOutModel>>(environment.remoteData.apiUri + 'TicketType/TicketTypeGet', model);
  }

  TicketTypePut(model: GenericRequest<TicketTypePutInModel>): Observable<GenericResult<TicketTypePutOutModel>> {
    return this.http.post<GenericResult<TicketTypePutOutModel>>(environment.remoteData.apiUri + 'TicketType/TicketTypePut', model);
  }
}
