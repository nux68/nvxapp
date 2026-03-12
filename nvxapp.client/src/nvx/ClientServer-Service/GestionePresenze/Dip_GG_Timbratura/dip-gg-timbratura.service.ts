import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  Dip_GG_Timbratura_GetAll_InModel,
  Dip_GG_Timbratura_GetAll_OutModel,
  Dip_GG_Timbratura_Stamp_InModel,
  Dip_GG_Timbratura_Stamp_OutModel,
  Dip_GG_Timbratura_Get_4Calculation_InModel,
  Dip_GG_Timbratura_Get_4Calculation_OutModel
} from './Models/dip-gg-timbratura-model';
import { AuthService } from '../../../Utility/infrastructure/auth.service';

@Injectable({
  providedIn: 'root'
})
export class DipGGTimbraturaService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_GG_Timbratura_GetAll_InModel>): Observable<GenericResult<Dip_GG_Timbratura_GetAll_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Timbratura_GetAll_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Timbratura/GetAll', model)
      .pipe(
        map(r => {
          return r;
        })
      );

  }

  Stamp(model: GenericRequest<Dip_GG_Timbratura_Stamp_InModel>): Observable<GenericResult<Dip_GG_Timbratura_Stamp_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Timbratura_Stamp_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Timbratura/Stamp', model)
      .pipe(
        map(r => {
          return r;
        })
      );

  }

  Dip_GG_Timbratura_Get_4Calculation(model: GenericRequest<Dip_GG_Timbratura_Get_4Calculation_InModel>): Observable<GenericResult<Dip_GG_Timbratura_Get_4Calculation_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Timbratura_Get_4Calculation_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Timbratura/Dip_GG_Timbratura_Get_4Calculation', model)
      .pipe(
        map(r => {
          return r;
        })
      );

  }

}
