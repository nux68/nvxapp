import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_Orario_GetAllInModel, Par_Orario_GetAllOutModel, Par_Orario_GetInModel, Par_Orario_GetOutModel, Par_Orario_PutInModel, Par_Orario_PutOutModel, Par_Orario_DeleteInModel, Par_Orario_DeleteOutModel } from './Models/par-orario-model';



@Injectable({
  providedIn: 'root'
})
export class ParOrarioService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_Orario_GetAllInModel>): Observable<GenericResult<Par_Orario_GetAllOutModel>> {
    return this.http.post<GenericResult<Par_Orario_GetAllOutModel>>(environment.remoteData.apiUri + 'Par_Orario/GetAll', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Par_OrarioGet(model: GenericRequest<Par_Orario_GetInModel>): Observable<GenericResult<Par_Orario_GetOutModel>> {
    return this.http.post<GenericResult<Par_Orario_GetOutModel>>(environment.remoteData.apiUri + 'Par_Orario/Par_OrarioGet', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Par_OrarioPut(model: GenericRequest<Par_Orario_PutInModel>): Observable<GenericResult<Par_Orario_PutOutModel>> {
    return this.http.post<GenericResult<Par_Orario_PutOutModel>>(environment.remoteData.apiUri + 'Par_Orario/Par_OrarioPut', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Par_OrarioDelete(model: GenericRequest<Par_Orario_DeleteInModel>): Observable<GenericResult<Par_Orario_DeleteOutModel>> {
    return this.http.post<GenericResult<Par_Orario_DeleteOutModel>>(environment.remoteData.apiUri + 'Par_Orario/Par_OrarioDelete', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }
}
