import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_OrarioIntervalloHHInModel, Par_OrarioIntervalloHHOutModel } from './Models/par-orario-intervallo-hh-model';

@Injectable({
  providedIn: 'root'
})
export class ParOrarioIntervalloHHService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_OrarioIntervalloHHInModel>): Observable<GenericResult<Par_OrarioIntervalloHHOutModel>> {

    return this.http.post<GenericResult<Par_OrarioIntervalloHHOutModel>>(environment.remoteData.apiUri + 'Par_OrarioIntervalloHH/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

  
}
