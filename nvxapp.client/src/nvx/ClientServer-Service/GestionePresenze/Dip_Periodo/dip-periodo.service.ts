import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { Dip_Periodo_Get_InModel, Dip_Periodo_Get_OutModel, Dip_Periodo_GetAll_InModel, Dip_Periodo_GetAll_OutModel, Dip_Periodo_SetRiconosciute_InModel, Dip_Periodo_SetRiconosciute_OutModel, Dip_Periodo_SetStato_InModel, Dip_Periodo_SetStato_OutModel } from './Models/dip-periodo-model';

@Injectable({
  providedIn: 'root'
})
export class DipPeriodoService {

  constructor(private http: HttpClient) { }

  Dip_Periodo_Get(model: GenericRequest<Dip_Periodo_Get_InModel>): Observable<GenericResult<Dip_Periodo_Get_OutModel>> {
    return this.http.post<GenericResult<Dip_Periodo_Get_OutModel>>(environment.remoteData.apiUri + 'Dip_Periodo/Dip_Periodo_Get', model);
  }

  Dip_Periodo_GetAll(model: GenericRequest<Dip_Periodo_GetAll_InModel>): Observable<GenericResult<Dip_Periodo_GetAll_OutModel>> {
    return this.http.post<GenericResult<Dip_Periodo_GetAll_OutModel>>(environment.remoteData.apiUri + 'Dip_Periodo/Dip_Periodo_GetAll', model);
  }

  Dip_Periodo_SetStato(model: GenericRequest<Dip_Periodo_SetStato_InModel>): Observable<GenericResult<Dip_Periodo_SetStato_OutModel>> {
    return this.http.post<GenericResult<Dip_Periodo_SetStato_OutModel>>(environment.remoteData.apiUri + 'Dip_Periodo/Dip_Periodo_SetStato', model);
  }

  Dip_Periodo_SetRiconosciute(model: GenericRequest<Dip_Periodo_SetRiconosciute_InModel>): Observable<GenericResult<Dip_Periodo_SetRiconosciute_OutModel>> {
    return this.http.post<GenericResult<Dip_Periodo_SetRiconosciute_OutModel>>(environment.remoteData.apiUri + 'Dip_Periodo/Dip_Periodo_SetRiconosciute', model);
  }
}
