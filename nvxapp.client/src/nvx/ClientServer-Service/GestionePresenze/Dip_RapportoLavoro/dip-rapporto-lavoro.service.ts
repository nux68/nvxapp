import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_RapportoLavoro_Get_InModel, Dip_RapportoLavoro_Get_OutModel, Dip_RapportoLavoro_Put_InModel, Dip_RapportoLavoro_Put_OutModel } from './Models/dip-rapporto-lavoro-model';


@Injectable({
  providedIn: 'root'
})
export class DipRapportoLavoroService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  Dip_RapportoLavoroGet(model: GenericRequest<Dip_RapportoLavoro_Get_InModel>): Observable<GenericResult<Dip_RapportoLavoro_Get_OutModel>> {
    return this.http.post<GenericResult<Dip_RapportoLavoro_Get_OutModel>>(environment.remoteData.apiUri + 'Dip_RapportoLavoro/Dip_RapportoLavoroGet', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Dip_RapportoLavoroPut(model: GenericRequest<Dip_RapportoLavoro_Put_InModel>): Observable<GenericResult<Dip_RapportoLavoro_Put_OutModel>> {
    return this.http.post<GenericResult<Dip_RapportoLavoro_Put_OutModel>>(environment.remoteData.apiUri + 'Dip_RapportoLavoro/Dip_RapportoLavoroPut', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

}
