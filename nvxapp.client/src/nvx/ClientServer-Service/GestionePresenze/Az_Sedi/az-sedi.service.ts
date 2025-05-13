import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_Sedi_GetAll_InModel, Az_Sedi_GetAll_OutModel } from './Models/az-sedi-model';

@Injectable({
  providedIn: 'root'
})
export class AzSediService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Az_Sedi_GetAll_InModel>): Observable<GenericResult<Az_Sedi_GetAll_OutModel>> {

    return this.http.post<GenericResult<Az_Sedi_GetAll_OutModel>>(environment.remoteData.apiUri + 'Az_Sedi/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
