import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_CausaliOutModel } from './Models/par-causali-model';

@Injectable({
  providedIn: 'root'
})
export class ParCausaliService {


  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_CausaliOutModel>): Observable<GenericResult<Par_CausaliOutModel>> {

    return this.http.post<GenericResult<Par_CausaliOutModel>>(environment.remoteData.apiUri + 'Par_Causali/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
