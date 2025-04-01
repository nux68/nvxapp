import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_GG_CausaliInModel, Dip_GG_CausaliOutModel } from './Models/dip-gg-causali-model';

@Injectable({
  providedIn: 'root'
})
export class DipGGCausaliService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_GG_CausaliInModel>): Observable<GenericResult<Dip_GG_CausaliOutModel>> {

    return this.http.post<GenericResult<Dip_GG_CausaliOutModel>>(environment.remoteData.apiUri + 'Dip_GG_Causali/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
