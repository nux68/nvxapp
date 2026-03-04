import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_OrarioIntervalloHH_Get_4Edit_InModel, Par_OrarioIntervalloHH_Get_4Edit_OutModel, Par_OrarioIntervalloHH_Put_4Edit_InModel, Par_OrarioIntervalloHH_Put_4Edit_OutModel, Par_OrarioIntervalloHHInModel, Par_OrarioIntervalloHHOutModel } from './Models/par-orario-intervallo-hh-model';

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


  Par_OrarioIntervalloHH_Get(model: GenericRequest<Par_OrarioIntervalloHH_Get_4Edit_InModel>): Observable<GenericResult<Par_OrarioIntervalloHH_Get_4Edit_OutModel>> {
    return this.http.post<GenericResult<Par_OrarioIntervalloHH_Get_4Edit_OutModel>>(environment.remoteData.apiUri + 'Par_OrarioIntervalloHH/Par_OrarioIntervalloHH_Get', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }

  Par_OrarioIntervalloHH_Put(model: GenericRequest<Par_OrarioIntervalloHH_Put_4Edit_InModel>): Observable<GenericResult<Par_OrarioIntervalloHH_Put_4Edit_OutModel>> {
    return this.http.post<GenericResult<Par_OrarioIntervalloHH_Put_4Edit_OutModel>>(environment.remoteData.apiUri + 'Par_OrarioIntervalloHH/Par_OrarioIntervalloHH_Put', model)
      .pipe(
        map(r => {
          return r;
        })
      );
  }
  
}
