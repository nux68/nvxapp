import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_GG_Causali_DeleteInModel, Dip_GG_Causali_DeleteOutModel, Dip_GG_Causali_Get_4Calculation_InModel, Dip_GG_Causali_Get_4Calculation_OutModel, Dip_GG_Causali_GetAll_InModel, Dip_GG_Causali_GetAll_OutModel, Dip_GG_CausaliGetInModel, Dip_GG_CausaliGetOutModel, Dip_GG_CausaliPutInModel, Dip_GG_CausaliPutOutModel } from './Models/dip-gg-causali-model';

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

  Dip_GG_Causali_Delete(model: GenericRequest<Dip_GG_Causali_DeleteInModel>): Observable<GenericResult<Dip_GG_Causali_DeleteOutModel>> {

    return this.http.post<GenericResult<Dip_GG_Causali_DeleteOutModel>>(environment.remoteData.apiUri + 'Dip_GG_Causali/Dip_GG_Causali_Delete', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Dip_GG_Causali_Put(model: GenericRequest<Dip_GG_CausaliPutInModel>): Observable<GenericResult<Dip_GG_CausaliPutOutModel>> {

    return this.http.post<GenericResult<Dip_GG_CausaliPutOutModel>>(environment.remoteData.apiUri + 'Dip_GG_Causali/Dip_GG_CausaliPut', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Dip_GG_Causali_Get(model: GenericRequest<Dip_GG_CausaliGetInModel>): Observable<GenericResult<Dip_GG_CausaliGetOutModel>> {

    return this.http.post<GenericResult<Dip_GG_CausaliGetOutModel>>(environment.remoteData.apiUri + 'Dip_GG_Causali/Dip_GG_CausaliGet', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }
}
