import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_RapportoLavoroInModel, Dip_RapportoLavoroOutModel } from './Models/dip-rapporto-lavoro-model';

@Injectable({
  providedIn: 'root'
})
export class DipRapportoLavoroService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_RapportoLavoroInModel>): Observable<GenericResult<Dip_RapportoLavoroOutModel>> {

    return this.http.post<GenericResult<Dip_RapportoLavoroOutModel>>(environment.remoteData.apiUri + 'Az_Anagrafica/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
