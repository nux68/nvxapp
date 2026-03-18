import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_GG_Causali_Get_4Calculation_InModel, Dip_GG_Causali_Get_4Calculation_OutModel, Dip_GG_Causali_GetAll_InModel, Dip_GG_Causali_GetAll_OutModel } from './Models/dip-gg-causali-model';

@Injectable({
  providedIn: 'root'
})
export class DipGGCausaliService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_GG_Causali_GetAll_InModel>): Observable<GenericResult<Dip_GG_Causali_GetAll_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Causali_GetAll_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Causali/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  Dip_GG_Causali_Get_4Calculation_OutModel(model: GenericRequest<Dip_GG_Causali_Get_4Calculation_InModel>): Observable<GenericResult<Dip_GG_Causali_Get_4Calculation_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Causali_Get_4Calculation_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Causali/Dip_GG_Causali_Get_4Calculation', model)
      .pipe(
        map(r => {
          return r;
        })
      );

  }

}
