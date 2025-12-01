import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_ProfiloOrario_DeleteInModel, Par_ProfiloOrario_DeleteOutModel, Par_ProfiloOrario_GetAllInModel, Par_ProfiloOrario_GetAllOutModel, Par_ProfiloOrario_GetInModel, Par_ProfiloOrario_GetOutModel, Par_ProfiloOrario_PutInModel, Par_ProfiloOrario_PutOutModel } from './Models/par-profilo-orario-model';


@Injectable({
  providedIn: 'root'
})
export class ParProfiloOrarioService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_ProfiloOrario_GetAllInModel>): Observable<GenericResult<Par_ProfiloOrario_GetAllOutModel>> {

    return this.http.post<GenericResult<Par_ProfiloOrario_GetAllOutModel>>(environment.remoteData.apiUri + 'Par_ProfiloOrario/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  Par_ProfiloOrarioGet(model: GenericRequest<Par_ProfiloOrario_GetInModel>): Observable<GenericResult<Par_ProfiloOrario_GetOutModel>> {
    return this.http.post<GenericResult<Par_ProfiloOrario_GetOutModel>>(environment.remoteData.apiUri + 'Par_ProfiloOrario/Par_ProfiloOrarioGet', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Par_ProfiloOrarioPut(model: GenericRequest<Par_ProfiloOrario_PutInModel>): Observable<GenericResult<Par_ProfiloOrario_PutOutModel>> {
    return this.http.post<GenericResult<Par_ProfiloOrario_PutOutModel>>(environment.remoteData.apiUri + 'Par_ProfiloOrario/Par_ProfiloOrarioPut', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Par_ProfiloOrarioDelete(model: GenericRequest<Par_ProfiloOrario_DeleteInModel>): Observable<GenericResult<Par_ProfiloOrario_DeleteOutModel>> {
    return this.http.post<GenericResult<Par_ProfiloOrario_DeleteOutModel>>(environment.remoteData.apiUri + 'Par_ProfiloOrario/Par_ProfiloOrarioDelete', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }
  
}
