import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  Az_SubCommessaAttivita_GetAll_InModel, Az_SubCommessaAttivita_GetAll_OutModel,
  Az_SubCommessaAttivita_GetAll_4FullList_InModel, Az_SubCommessaAttivita_GetAll_4FullList_OutModel,
  Az_SubCommessaAttivita_Get4SubCommessa_InModel, Az_SubCommessaAttivita_Get4SubCommessa_OutModel,
  Az_SubCommessaAttivita_Put4SubCommessa_InModel, Az_SubCommessaAttivita_Put4SubCommessa_OutModel
} from './Models/az-subcommessa-attivita-model';

@Injectable({
  providedIn: 'root'
})
export class AzSubCommessaAttivitaService {
  constructor(private http: HttpClient, private authService: AuthService) { }

  GetAll(model: GenericRequest<Az_SubCommessaAttivita_GetAll_InModel>): Observable<GenericResult<Az_SubCommessaAttivita_GetAll_OutModel>> {
    return this.http.post<GenericResult<Az_SubCommessaAttivita_GetAll_OutModel>>(environment.remoteData.apiUri + 'Az_SubCommessaAttivita/GetAll', model)
      .pipe(map(r => r));
  }

  GetAll_4FullList(model: GenericRequest<Az_SubCommessaAttivita_GetAll_4FullList_InModel>): Observable<GenericResult<Az_SubCommessaAttivita_GetAll_4FullList_OutModel>> {
    return this.http.post<GenericResult<Az_SubCommessaAttivita_GetAll_4FullList_OutModel>>(environment.remoteData.apiUri + 'Az_SubCommessaAttivita/GetAll_4FullList', model)
      .pipe(map(r => r));
  }

  Get4SubCommessa(model: GenericRequest<Az_SubCommessaAttivita_Get4SubCommessa_InModel>): Observable<GenericResult<Az_SubCommessaAttivita_Get4SubCommessa_OutModel>> {
    return this.http.post<GenericResult<Az_SubCommessaAttivita_Get4SubCommessa_OutModel>>(environment.remoteData.apiUri + 'Az_SubCommessaAttivita/Get4SubCommessa', model)
      .pipe(map(r => r));
  }

  Put4SubCommessa(model: GenericRequest<Az_SubCommessaAttivita_Put4SubCommessa_InModel>): Observable<GenericResult<Az_SubCommessaAttivita_Put4SubCommessa_OutModel>> {
    return this.http.post<GenericResult<Az_SubCommessaAttivita_Put4SubCommessa_OutModel>>(environment.remoteData.apiUri + 'Az_SubCommessaAttivita/Put4SubCommessa', model)
      .pipe(map(r => r));
  }

  //Get_Az_SubCommessaAttivita_Default(model: GenericRequest<Get_Az_SubCommessaAttivita_Default_InModel>): Observable<GenericResult<Get_Az_SubCommessaAttivita_Default_OutModel>> {
  //  return this.http.post<GenericResult<Get_Az_SubCommessaAttivita_Default_OutModel>>(environment.remoteData.apiUri + 'Az_SubCommessaAttivita/Get_Az_SubCommessaAttivita_Default', model)
  //    .pipe(map(r => r));
  //}
}
