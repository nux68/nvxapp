import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  Dip_AccordoSW_CalendarioInModel, Dip_AccordoSW_CalendarioOutModel, Dip_AccordoSW_DeleteInModel, Dip_AccordoSW_DeleteOutModel,
  Dip_AccordoSW_GetAllInModel, Dip_AccordoSW_GetAllOutModel, Dip_AccordoSW_GetInModel, Dip_AccordoSW_GetOutModel,
  Dip_AccordoSW_PutInModel, Dip_AccordoSW_PutOutModel
} from './Models/smart-working-model';

// Smart working (AT-SW): accordi individuali e calendario del lavoro agile
@Injectable({
  providedIn: 'root'
})
export class SmartWorkingService {

  constructor(private http: HttpClient) { }

  private post<I, O>(azione: string, model: GenericRequest<I>): Observable<GenericResult<O>> {
    return this.http.post<GenericResult<O>>(environment.remoteData.apiUri + 'Dip_AccordoSW/' + azione, model);
  }

  GetAll(m: GenericRequest<Dip_AccordoSW_GetAllInModel>) { return this.post<Dip_AccordoSW_GetAllInModel, Dip_AccordoSW_GetAllOutModel>('GetAll', m); }
  Dip_AccordoSWGet(m: GenericRequest<Dip_AccordoSW_GetInModel>) { return this.post<Dip_AccordoSW_GetInModel, Dip_AccordoSW_GetOutModel>('Dip_AccordoSWGet', m); }
  Dip_AccordoSWPut(m: GenericRequest<Dip_AccordoSW_PutInModel>) { return this.post<Dip_AccordoSW_PutInModel, Dip_AccordoSW_PutOutModel>('Dip_AccordoSWPut', m); }
  Dip_AccordoSWDelete(m: GenericRequest<Dip_AccordoSW_DeleteInModel>) { return this.post<Dip_AccordoSW_DeleteInModel, Dip_AccordoSW_DeleteOutModel>('Dip_AccordoSWDelete', m); }
  Calendario(m: GenericRequest<Dip_AccordoSW_CalendarioInModel>) { return this.post<Dip_AccordoSW_CalendarioInModel, Dip_AccordoSW_CalendarioOutModel>('Calendario', m); }
}
