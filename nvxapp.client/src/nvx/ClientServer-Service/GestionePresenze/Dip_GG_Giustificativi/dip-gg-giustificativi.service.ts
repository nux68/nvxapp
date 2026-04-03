import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
    Dip_GG_GiustificativiGetInModel,
    Dip_GG_GiustificativiGetOutModel,
    Dip_GG_GiustificativiPutInModel,
    Dip_GG_GiustificativiPutOutModel,
    Dip_GG_Giustificativi_DeleteInModel,
    Dip_GG_Giustificativi_DeleteOutModel,
  Dip_GG_Giustificativi_GetAll_InModel,
  Dip_GG_Giustificativi_GetAll_OutModel,
  Dip_GG_Giustificativi_Get_4Calculation_InModel,
  Dip_GG_Giustificativi_Get_4Calculation_OutModel
} from './Models/dip-gg-giustificativi-model';

@Injectable({
  providedIn: 'root'
})
export class DipGGGiustificativiService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_GG_Giustificativi_GetAll_InModel>): Observable<GenericResult<Dip_GG_Giustificativi_GetAll_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Giustificativi_GetAll_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Giustificativi/GetAll', model)
      .pipe(
        map(r => {
          return r;
        })
      );

  }

  Dip_GG_Giustificativi_Get_4Calculation(model: GenericRequest<Dip_GG_Giustificativi_Get_4Calculation_InModel>): Observable<GenericResult<Dip_GG_Giustificativi_Get_4Calculation_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Giustificativi_Get_4Calculation_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Giustificativi/Dip_GG_Giustificativi_Get_4Calculation', model)
      .pipe(
        map(r => {
          return r;
        })
      );

  }

  Dip_GG_Giustificativi_Delete(model: GenericRequest<Dip_GG_Giustificativi_DeleteInModel>): Observable<GenericResult<Dip_GG_Giustificativi_DeleteOutModel>> {

    return this.http.post<GenericResult<Dip_GG_Giustificativi_DeleteOutModel>>(environment.remoteData.apiUri + 'Dip_GG_Giustificativi/Dip_GG_Giustificativi_Delete', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Dip_GG_Giustificativi_Put(model: GenericRequest<Dip_GG_GiustificativiPutInModel>): Observable<GenericResult<Dip_GG_GiustificativiPutOutModel>> {

    return this.http.post<GenericResult<Dip_GG_GiustificativiPutOutModel>>(environment.remoteData.apiUri + 'Dip_GG_Giustificativi/Dip_GG_GiustificativiPut', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Dip_GG_Giustificativi_Get(model: GenericRequest<Dip_GG_GiustificativiGetInModel>): Observable<GenericResult<Dip_GG_GiustificativiGetOutModel>> {

    return this.http.post<GenericResult<Dip_GG_GiustificativiGetOutModel>>(environment.remoteData.apiUri + 'Dip_GG_Giustificativi/Dip_GG_GiustificativiGet', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

}
