import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_Anagrafica_Get_InModel, Dip_Anagrafica_Get_OutModel, Dip_Anagrafica_GetAll_InModel, Dip_Anagrafica_GetAll_OutModel, Dip_Anagrafica_Put_InModel, Dip_Anagrafica_Put_OutModel } from './Models/dip-anagrafica-model';

@Injectable({
  providedIn: 'root'
})
export class DipAnagraficaService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_Anagrafica_GetAll_InModel>): Observable<GenericResult<Dip_Anagrafica_GetAll_OutModel>> {

    return this.http.post<GenericResult<Dip_Anagrafica_GetAll_OutModel>>(environment.remoteData.apiUri + 'Dip_Anagrafica/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  Dip_AnagraficaGet(model: GenericRequest<Dip_Anagrafica_Get_InModel>): Observable<GenericResult<Dip_Anagrafica_Get_OutModel>> {
    return this.http.post<GenericResult<Dip_Anagrafica_Get_OutModel>>(environment.remoteData.apiUri + 'Dip_Anagrafica/Dip_AnagraficaGet', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Dip_AnagraficaPut(model: GenericRequest<Dip_Anagrafica_Put_InModel>): Observable<GenericResult<Dip_Anagrafica_Put_OutModel>> {
    return this.http.post<GenericResult<Dip_Anagrafica_Put_OutModel>>(environment.remoteData.apiUri + 'Dip_Anagrafica/Dip_AnagraficaPut', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

}
