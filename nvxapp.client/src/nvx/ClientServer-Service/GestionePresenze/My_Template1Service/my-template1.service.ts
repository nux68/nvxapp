import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { My_template1InModel, My_template1OutModel } from './Models/my-template1-model';

@Injectable({
  providedIn: 'root'
})
export class MyTemplate1Service {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<My_template1InModel>): Observable<GenericResult<My_template1OutModel>> {

    return this.http.post<GenericResult<My_template1OutModel>>(environment.remoteData.apiUri + 'My_Template1/GetAll', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
