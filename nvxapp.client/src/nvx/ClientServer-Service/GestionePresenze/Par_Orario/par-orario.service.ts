import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_OrarioInModel, Par_OrarioOutModel } from './Models/par-orario-model';

@Injectable({
  providedIn: 'root'
})
export class ParOrarioService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_OrarioInModel>): Observable<GenericResult<Par_OrarioOutModel>> {

    return this.http.post<GenericResult<Par_OrarioOutModel>>(environment.remoteData.apiUri + 'Az_APar_Orarionagrafica/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }


}
