import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_SediInModel, Az_SediOutModel } from './Models/az-sedi-model';

@Injectable({
  providedIn: 'root'
})
export class AzSediService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Az_SediInModel>): Observable<GenericResult<Az_SediOutModel>> {

    return this.http.post<GenericResult<Az_SediOutModel>>(environment.remoteData.apiUri + 'Az_Sedi/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
