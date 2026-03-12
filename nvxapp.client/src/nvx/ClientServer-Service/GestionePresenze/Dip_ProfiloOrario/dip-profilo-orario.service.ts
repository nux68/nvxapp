import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  Dip_ProfiloOrario_Get_InModel,
  Dip_ProfiloOrario_Get_OutModel,
  Dip_ProfiloOrario_Put_InModel,
  Dip_ProfiloOrario_Put_OutModel
} from './Models/dip-profilo-orario-model';


@Injectable({
  providedIn: 'root'
})
export class DipProfiloOrarioService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  Dip_ProfiloOrarioGet(model: GenericRequest<Dip_ProfiloOrario_Get_InModel>): Observable<GenericResult<Dip_ProfiloOrario_Get_OutModel>> {
    return this.http.post<GenericResult<Dip_ProfiloOrario_Get_OutModel>>(environment.remoteData.apiUri + 'Dip_ProfiloOrario/Dip_ProfiloOrarioGet', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Dip_ProfiloOrarioPut(model: GenericRequest<Dip_ProfiloOrario_Put_InModel>): Observable<GenericResult<Dip_ProfiloOrario_Put_OutModel>> {
    return this.http.post<GenericResult<Dip_ProfiloOrario_Put_OutModel>>(environment.remoteData.apiUri + 'Dip_ProfiloOrario/Dip_ProfiloOrarioPut', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  

}
