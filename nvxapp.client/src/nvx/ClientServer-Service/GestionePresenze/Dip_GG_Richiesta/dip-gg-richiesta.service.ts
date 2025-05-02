import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_GG_Richiesta_GetAll4Admin_InModel, Dip_GG_Richiesta_GetAll4Admin_OutModel, Dip_GG_Richiesta_GetAll4User_InModel,  Dip_GG_Richiesta_GetAll4User_OutModel,  Dip_GG_Richiesta_Send_InModel, Dip_GG_Richiesta_Send_OutModel, Dip_GG_Richiesta_SetState_InModel, Dip_GG_Richiesta_SetState_OutModel } from './Models/dip-gg-richiesta-model';

@Injectable({
  providedIn: 'root'
})
export class DipGGRichiestaService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll4User(model: GenericRequest<Dip_GG_Richiesta_GetAll4User_InModel>): Observable<GenericResult<Dip_GG_Richiesta_GetAll4User_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Richiesta_GetAll4User_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Richiesta/GetAll4User', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  GetAll4Admin(model: GenericRequest<Dip_GG_Richiesta_GetAll4Admin_InModel>): Observable<GenericResult<Dip_GG_Richiesta_GetAll4Admin_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Richiesta_GetAll4Admin_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Richiesta/GetAll4Admin', model)
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

  SetState(model: GenericRequest<Dip_GG_Richiesta_SetState_InModel>): Observable<GenericResult<Dip_GG_Richiesta_SetState_OutModel>> {

    return this.http.post<GenericResult<Dip_GG_Richiesta_SetState_OutModel>>(environment.remoteData.apiUri + 'Dip_GG_Richiesta/SetState', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
