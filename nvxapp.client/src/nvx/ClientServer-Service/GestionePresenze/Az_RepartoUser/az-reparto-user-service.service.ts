import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_RepartoUser_GetAll_InModel, Az_RepartoUser_GetAll_OutModel } from './Models/az-reparto-user-model';

@Injectable({
  providedIn: 'root'
})
export class AzRepartoUserServiceService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Az_RepartoUser_GetAll_InModel>): Observable<GenericResult<Az_RepartoUser_GetAll_OutModel>> {

    return this.http.post<GenericResult<Az_RepartoUser_GetAll_OutModel>>(environment.remoteData.apiUri + 'Az_RepartoUser/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }
}
