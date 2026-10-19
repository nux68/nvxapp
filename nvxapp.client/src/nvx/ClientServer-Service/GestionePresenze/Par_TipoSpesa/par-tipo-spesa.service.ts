import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { Par_TipoSpesa_DeleteInModel, Par_TipoSpesa_DeleteOutModel, Par_TipoSpesaGetInModel, Par_TipoSpesaGetOutModel, Par_TipoSpesaInModel, Par_TipoSpesaOutModel, Par_TipoSpesaPutInModel, Par_TipoSpesaPutOutModel } from './Models/par-tipo-spesa-model';

@Injectable({
  providedIn: 'root'
})
export class ParTipoSpesaService {

  constructor(private http: HttpClient) { }

  GetAll(model: GenericRequest<Par_TipoSpesaInModel>): Observable<GenericResult<Par_TipoSpesaOutModel>> {
    return this.http.post<GenericResult<Par_TipoSpesaOutModel>>(environment.remoteData.apiUri + 'Par_TipoSpesa/GetAll', model);
  }

  Par_TipoSpesaGet(model: GenericRequest<Par_TipoSpesaGetInModel>): Observable<GenericResult<Par_TipoSpesaGetOutModel>> {
    return this.http.post<GenericResult<Par_TipoSpesaGetOutModel>>(environment.remoteData.apiUri + 'Par_TipoSpesa/Par_TipoSpesaGet', model);
  }

  Par_TipoSpesaPut(model: GenericRequest<Par_TipoSpesaPutInModel>): Observable<GenericResult<Par_TipoSpesaPutOutModel>> {
    return this.http.post<GenericResult<Par_TipoSpesaPutOutModel>>(environment.remoteData.apiUri + 'Par_TipoSpesa/Par_TipoSpesaPut', model);
  }

  Par_TipoSpesaDelete(model: GenericRequest<Par_TipoSpesa_DeleteInModel>): Observable<GenericResult<Par_TipoSpesa_DeleteOutModel>> {
    return this.http.post<GenericResult<Par_TipoSpesa_DeleteOutModel>>(environment.remoteData.apiUri + 'Par_TipoSpesa/Par_TipoSpesaDelete', model);
  }
}
