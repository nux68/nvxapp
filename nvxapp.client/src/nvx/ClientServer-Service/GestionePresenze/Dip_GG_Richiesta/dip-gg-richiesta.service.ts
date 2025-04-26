import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_GG_Richiesta_GetAll_InModel, Dip_GG_Richiesta_GetAll_OutModel, Dip_GG_Richiesta_Send_InModel, Dip_GG_Richiesta_Send_OutModel } from './Models/dip-gg-richiesta-model';

@Injectable({
  providedIn: 'root'
})
export class DipGGRichiestaService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_GG_Richiesta_GetAll_InModel>): Observable<GenericResult<Dip_GG_Richiesta_GetAll_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Richiesta_GetAll_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Richiesta/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  Send(model: GenericRequest<Dip_GG_Richiesta_Send_InModel>): Observable<GenericResult<Dip_GG_Richiesta_Send_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Richiesta_Send_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Richiesta/Send', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
