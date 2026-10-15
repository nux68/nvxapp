import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { Par_Festivita_DeleteInModel, Par_Festivita_DeleteOutModel, Par_Festivita_GetAllInModel, Par_Festivita_GetAllOutModel, Par_Festivita_GetInModel, Par_Festivita_GetOutModel, Par_Festivita_PutInModel, Par_Festivita_PutOutModel } from './Models/par-festivita-model';

@Injectable({
  providedIn: 'root'
})
export class ParFestivitaService {

  constructor(private http: HttpClient) { }

  GetAll(model: GenericRequest<Par_Festivita_GetAllInModel>): Observable<GenericResult<Par_Festivita_GetAllOutModel>> {
    return this.http.post<GenericResult<Par_Festivita_GetAllOutModel>>(environment.remoteData.apiUri + 'Par_Festivita/GetAll', model);
  }

  Par_FestivitaGet(model: GenericRequest<Par_Festivita_GetInModel>): Observable<GenericResult<Par_Festivita_GetOutModel>> {
    return this.http.post<GenericResult<Par_Festivita_GetOutModel>>(environment.remoteData.apiUri + 'Par_Festivita/Par_FestivitaGet', model);
  }

  Par_FestivitaPut(model: GenericRequest<Par_Festivita_PutInModel>): Observable<GenericResult<Par_Festivita_PutOutModel>> {
    return this.http.post<GenericResult<Par_Festivita_PutOutModel>>(environment.remoteData.apiUri + 'Par_Festivita/Par_FestivitaPut', model);
  }

  Par_FestivitaDelete(model: GenericRequest<Par_Festivita_DeleteInModel>): Observable<GenericResult<Par_Festivita_DeleteOutModel>> {
    return this.http.post<GenericResult<Par_Festivita_DeleteOutModel>>(environment.remoteData.apiUri + 'Par_Festivita/Par_FestivitaDelete', model);
  }
}
