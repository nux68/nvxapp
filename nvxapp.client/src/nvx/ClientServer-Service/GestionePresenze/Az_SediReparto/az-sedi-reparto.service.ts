import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_SediRepartoGetInModel, Az_SediRepartoGetOutModel, Az_SediReparto_GetAll_InModel, Az_SediReparto_GetAll_OutModel, Az_SediRepartoPutInModel, Az_SediRepartoPutOutModel } from './Models/az-sedi-reparto-model';

@Injectable({
  providedIn: 'root'
})
export class AzSediRepartoService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Az_SediReparto_GetAll_InModel>): Observable<GenericResult<Az_SediReparto_GetAll_OutModel>> {

    return this.http.post<GenericResult<Az_SediReparto_GetAll_OutModel>>(environment.remoteData.apiUri + 'Az_SediReparto/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }


  Az_SediRepartoGet(model: GenericRequest<Az_SediRepartoGetInModel>): Observable<GenericResult<Az_SediRepartoGetOutModel>> {

    return this.http.post<GenericResult<Az_SediRepartoGetOutModel>>(environment.remoteData.apiUri + 'Az_SediReparto/Az_SediRepartoGet', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  Az_SediRepartoPut(model: GenericRequest<Az_SediRepartoPutInModel>): Observable<GenericResult<Az_SediRepartoPutOutModel>> {

    return this.http.post<GenericResult<Az_SediRepartoPutOutModel>>(environment.remoteData.apiUri + 'Az_SediReparto/Az_SediRepartoPut', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }


}
