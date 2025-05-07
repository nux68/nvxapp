import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_Cfg_Get_InModel, Az_Cfg_Get_OutModel, Az_Cfg_GetAll_InModel, Az_Cfg_GetAll_OutModel, Az_Cfg_Put_InModel, Az_Cfg_Put_OutModel } from './Models/az-cfg-model';

@Injectable({
  providedIn: 'root'
})
export class AzCfgService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Az_Cfg_GetAll_InModel>): Observable<GenericResult<Az_Cfg_GetAll_OutModel>> {

    return this.http.post<GenericResult<Az_Cfg_GetAll_OutModel>>(environment.remoteData.apiUri + 'Az_Cfg/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  Az_CfgGet(model: GenericRequest<Az_Cfg_Get_InModel>): Observable<GenericResult<Az_Cfg_Get_OutModel>> {

    return this.http.post<GenericResult<Az_Cfg_Get_OutModel>>(environment.remoteData.apiUri + 'Az_Cfg/Az_Cfg_Get', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  Az_CfgPut(model: GenericRequest<Az_Cfg_Put_InModel>): Observable<GenericResult<Az_Cfg_Put_OutModel>> {

    return this.http.post<GenericResult<Az_Cfg_Put_OutModel>>(environment.remoteData.apiUri + 'Az_Cfg/Az_Cfg_Put', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }


}
