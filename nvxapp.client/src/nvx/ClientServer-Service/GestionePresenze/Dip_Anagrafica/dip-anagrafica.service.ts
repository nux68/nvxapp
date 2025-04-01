import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_AnagraficaInModel, Dip_AnagraficaOutModel } from './Models/dip-anagrafica-model';

@Injectable({
  providedIn: 'root'
})
export class DipAnagraficaService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_AnagraficaInModel>): Observable<GenericResult<Dip_AnagraficaOutModel>> {

    return this.http.post<GenericResult<Dip_AnagraficaOutModel>>(environment.remoteData.apiUri + 'Dip_Anagrafica/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
