import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_GiustificativiGetInModel, Par_GiustificativiGetOutModel, Par_GiustificativiInModel, Par_GiustificativiOutModel, Par_GiustificativiPutInModel, Par_GiustificativiPutOutModel } from './Models/par-giustificativi-model';

@Injectable({
  providedIn: 'root'
})
export class ParGiustificativiService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_GiustificativiInModel>): Observable<GenericResult<Par_GiustificativiOutModel>> {

    return this.http.post<GenericResult<Par_GiustificativiOutModel>>(environment.remoteData.apiUri + 'Par_Giustificativi/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  Par_GiustificativiGet(model: GenericRequest<Par_GiustificativiGetInModel>): Observable<GenericResult<Par_GiustificativiGetOutModel>> {

    return this.http.post<GenericResult<Par_GiustificativiGetOutModel>>(environment.remoteData.apiUri + 'Par_Giustificativi/Par_GiustificativiGet', model)
      .pipe(
          map(r => {
            return r;
          }
        )
      );

  }

  Par_GiustificativiPut(model: GenericRequest<Par_GiustificativiPutInModel>): Observable<GenericResult<Par_GiustificativiPutOutModel>> {

    return this.http.post<GenericResult<Par_GiustificativiPutOutModel>>(environment.remoteData.apiUri + 'Par_Giustificativi/Par_GiustificativiPut', model)
      .pipe(
          map(r => {
              return r;
            }
          )
      );

  }

}
