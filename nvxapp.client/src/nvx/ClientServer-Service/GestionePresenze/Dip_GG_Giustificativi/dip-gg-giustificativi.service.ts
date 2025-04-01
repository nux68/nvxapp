import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_GG_GiustificativiInModel, Dip_GG_GiustificativiOutModel } from './Models/dip-gg-giustificativi-model';

@Injectable({
  providedIn: 'root'
})
export class DipGGGiustificativiService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_GG_GiustificativiInModel>): Observable<GenericResult<Dip_GG_GiustificativiOutModel>> {

    return this.http.post<GenericResult<Dip_GG_GiustificativiOutModel>>(environment.remoteData.apiUri + 'Az_Anagrafica/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
