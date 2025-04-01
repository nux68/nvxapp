import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Dip_ProfiloOrarioInModel, Dip_ProfiloOrarioOutModel } from './Models/dip-profilo-orario-model';

@Injectable({
  providedIn: 'root'
})
export class DipProfiloOrarioService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Dip_ProfiloOrarioInModel>): Observable<GenericResult<Dip_ProfiloOrarioOutModel>> {

    return this.http.post<GenericResult<Dip_ProfiloOrarioOutModel>>(environment.remoteData.apiUri + 'Dip_ProfiloOrario/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
