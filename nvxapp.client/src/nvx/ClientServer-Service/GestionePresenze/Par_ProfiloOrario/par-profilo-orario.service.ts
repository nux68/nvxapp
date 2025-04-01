import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_ProfiloOrarioInModel, Par_ProfiloOrarioOutModel } from './Models/par-profilo-orario-model';

@Injectable({
  providedIn: 'root'
})
export class ParProfiloOrarioService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_ProfiloOrarioInModel>): Observable<GenericResult<Par_ProfiloOrarioOutModel>> {

    return this.http.post<GenericResult<Par_ProfiloOrarioOutModel>>(environment.remoteData.apiUri + 'Par_ProfiloOrario/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  
}
