import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_GG_TimbraturaInModel, Dip_GG_TimbraturaOutModel } from './Models/dip-gg-timbratura-model';
import { AuthService } from '../../../Utility/infrastructure/auth.service';

@Injectable({
  providedIn: 'root'
})
export class DipGGTimbraturaService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_GG_TimbraturaInModel>): Observable<GenericResult<Dip_GG_TimbraturaOutModel>> {

    return this.http.post<GenericResult<Dip_GG_TimbraturaOutModel>>(environment.remoteData.apiUri + 'Dip_GG_Timbratura/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
