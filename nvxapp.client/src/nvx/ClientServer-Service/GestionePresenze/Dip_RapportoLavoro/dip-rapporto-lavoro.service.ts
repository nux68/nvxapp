import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_RapportoLavoro_GetAll_InModel, Dip_RapportoLavoro_GetAll_OutModel } from './Models/dip-rapporto-lavoro-model';

@Injectable({
  providedIn: 'root'
})
export class DipRapportoLavoroService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_RapportoLavoro_GetAll_InModel>): Observable<GenericResult<Dip_RapportoLavoro_GetAll_OutModel>> {

    return this.http.post<GenericResult<Dip_RapportoLavoro_GetAll_OutModel>>(environment.remoteData.apiUri + 'Az_Anagrafica/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
