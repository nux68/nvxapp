import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  TicketTypeAiSettingListInModel, TicketTypeAiSettingListOutModel, TicketTypeAiSettingPutInModel, TicketTypeAiSettingPutOutModel,
  TicketTypeAiSettingScopeInModel, TicketTypeAiSettingScopeOutModel
} from './Models/ticket-type-ai-setting-model';


// Risposta automatica AI dei tipi di richiesta per livello e nodo
@Injectable({
  providedIn: 'root'
})
export class TicketTypeAiSettingService {

  constructor(private http: HttpClient) { }

  TicketTypeAiSettingScope(model: GenericRequest<TicketTypeAiSettingScopeInModel>): Observable<GenericResult<TicketTypeAiSettingScopeOutModel>> {
    return this.http.post<GenericResult<TicketTypeAiSettingScopeOutModel>>(environment.remoteData.apiUri + 'TicketTypeAiSetting/TicketTypeAiSettingScope', model);
  }

  TicketTypeAiSettingList(model: GenericRequest<TicketTypeAiSettingListInModel>): Observable<GenericResult<TicketTypeAiSettingListOutModel>> {
    return this.http.post<GenericResult<TicketTypeAiSettingListOutModel>>(environment.remoteData.apiUri + 'TicketTypeAiSetting/TicketTypeAiSettingList', model);
  }

  TicketTypeAiSettingPut(model: GenericRequest<TicketTypeAiSettingPutInModel>): Observable<GenericResult<TicketTypeAiSettingPutOutModel>> {
    return this.http.post<GenericResult<TicketTypeAiSettingPutOutModel>>(environment.remoteData.apiUri + 'TicketTypeAiSetting/TicketTypeAiSettingPut', model);
  }
}
