import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_ArrotondamentiInModel, Par_ArrotondamentiOutModel } from './Models/par-arrotondamenti-model';

@Injectable({
  providedIn: 'root'
})
export class ParArrotondamentiService {


  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_ArrotondamentiInModel>): Observable<GenericResult<Par_ArrotondamentiOutModel>> {

    return this.http.post<GenericResult<Par_ArrotondamentiOutModel>>(environment.remoteData.apiUri + 'Par_Arrotondamenti/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }
  
}
