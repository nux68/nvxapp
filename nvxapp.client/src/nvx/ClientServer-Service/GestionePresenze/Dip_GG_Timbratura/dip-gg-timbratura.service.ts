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
  Dip_GG_Timbratura_Get_4Calculation_OutModel,
  Dip_GG_TimbraturaGetInModel,
  Dip_GG_TimbraturaGetOutModel,
  Dip_GG_TimbraturaPutInModel,
  Dip_GG_TimbraturaPutOutModel,
  Dip_GG_Timbratura_DeleteInModel,
  Dip_GG_Timbratura_DeleteOutModel
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

  Dip_GG_Timbratura_Put(model: GenericRequest<Dip_GG_TimbraturaPutInModel>): Observable<GenericResult<Dip_GG_TimbraturaPutOutModel>> {

    return this.http.post<GenericResult<Dip_GG_TimbraturaPutOutModel>>(environment.remoteData.apiUri + 'Dip_GG_Timbratura/Dip_GG_TimbraturaPut', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Dip_GG_Timbratura_Get(model: GenericRequest<Dip_GG_TimbraturaGetInModel>): Observable<GenericResult<Dip_GG_TimbraturaGetOutModel>> {

    return this.http.post<GenericResult<Dip_GG_TimbraturaGetOutModel>>(environment.remoteData.apiUri + 'Dip_GG_Timbratura/Dip_GG_TimbraturaGet', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Dip_GG_Timbratura_Delete(model: GenericRequest<Dip_GG_Timbratura_DeleteInModel>): Observable<GenericResult<Dip_GG_Timbratura_DeleteOutModel>> {

    return this.http.post<GenericResult<Dip_GG_Timbratura_DeleteOutModel>>(environment.remoteData.apiUri + 'Dip_GG_Timbratura/Dip_GG_Timbratura_Delete', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

}
