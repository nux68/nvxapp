import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_GiustificativiInModel, Par_GiustificativiOutModel } from './Models/par-giustificativi-model';

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

  
}
