import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  Az_ClienteContatto_GetAll_InModel, Az_ClienteContatto_GetAll_OutModel, Az_ClienteContattoDeleteInModel, Az_ClienteContattoDeleteOutModel,
  Az_ClienteContattoPutInModel, Az_ClienteContattoPutOutModel
} from './Models/az-cliente-contatto-model';

// Contatti dei clienti (tabella comune Az_ClienteContatto)
@Injectable({
  providedIn: 'root'
})
export class AzClienteContattoService {
  constructor(private http: HttpClient) { }

  GetAll(model: GenericRequest<Az_ClienteContatto_GetAll_InModel>): Observable<GenericResult<Az_ClienteContatto_GetAll_OutModel>> {
    return this.http.post<GenericResult<Az_ClienteContatto_GetAll_OutModel>>(environment.remoteData.apiUri + 'Az_ClienteContatto/GetAll', model);
  }

  Put(model: GenericRequest<Az_ClienteContattoPutInModel>): Observable<GenericResult<Az_ClienteContattoPutOutModel>> {
    return this.http.post<GenericResult<Az_ClienteContattoPutOutModel>>(environment.remoteData.apiUri + 'Az_ClienteContatto/Az_ClienteContattoPut', model);
  }

  Delete(model: GenericRequest<Az_ClienteContattoDeleteInModel>): Observable<GenericResult<Az_ClienteContattoDeleteOutModel>> {
    return this.http.post<GenericResult<Az_ClienteContattoDeleteOutModel>>(environment.remoteData.apiUri + 'Az_ClienteContatto/Az_ClienteContattoDelete', model);
  }
}
