import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_ProfiloOrarioGGInModel, Par_ProfiloOrarioGGOutModel } from './Models/par-profilo-orario-gg-model';

@Injectable({
  providedIn: 'root'
})
export class ParProfiloOrarioGGService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_ProfiloOrarioGGInModel>): Observable<GenericResult<Par_ProfiloOrarioGGOutModel>> {

    return this.http.post<GenericResult<Par_ProfiloOrarioGGOutModel>>(environment.remoteData.apiUri + 'Par_ProfiloOrarioGG/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  
}
