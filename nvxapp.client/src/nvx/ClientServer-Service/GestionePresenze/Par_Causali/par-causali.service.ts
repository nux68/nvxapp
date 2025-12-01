import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_Causali_DeleteInModel, Par_Causali_DeleteOutModel, Par_CausaliGetInModel, Par_CausaliGetOutModel, Par_CausaliInModel, Par_CausaliOutModel, Par_CausaliPutInModel, Par_CausaliPutOutModel } from './Models/par-causali-model';

@Injectable({
  providedIn: 'root'
})
export class ParCausaliService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_CausaliInModel>): Observable<GenericResult<Par_CausaliOutModel>> {

    return this.http.post<GenericResult<Par_CausaliOutModel>>(environment.remoteData.apiUri + 'Par_Causali/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  Par_CausaliGet(model: GenericRequest<Par_CausaliGetInModel>): Observable<GenericResult<Par_CausaliGetOutModel>> {

    return this.http.post<GenericResult<Par_CausaliGetOutModel>>(environment.remoteData.apiUri + 'Par_Causali/Par_CausaliGet', model)
      .pipe(
          map(r => {
            return r;
          }
        )
      );

  }

  Par_CausaliPut(model: GenericRequest<Par_CausaliPutInModel>): Observable<GenericResult<Par_CausaliPutOutModel>> {

    return this.http.post<GenericResult<Par_CausaliPutOutModel>>(environment.remoteData.apiUri + 'Par_Causali/Par_CausaliPut', model)
      .pipe(
          map(r => {
              return r;
            }
          )
      );

  }

  Par_CausaliDelete(model: GenericRequest<Par_Causali_DeleteInModel>): Observable<GenericResult<Par_Causali_DeleteOutModel>> {
    return this.http.post<GenericResult<Par_Causali_DeleteOutModel>>(environment.remoteData.apiUri + 'Par_Causali/Par_CausaliDelete', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }
}
