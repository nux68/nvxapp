import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { BehaviorSubject, map, Observable } from 'rxjs';
import { MyMokeLongJobInModel, MyMokeLongJobOutModel } from './Models/my-moke-long-job-model';

@Injectable({
  providedIn: 'root'
})
export class MyMokeLongJobService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }


  StartJob(model: GenericRequest<MyMokeLongJobInModel>): Observable<GenericResult<MyMokeLongJobOutModel>> {

    return this.http.post<GenericResult<MyMokeLongJobOutModel>>(environment.remoteData.apiUri + 'MyMokeLongJob/StartJob', model)
      .pipe(
        map(r => {
          return r;
          }
        )
      );

  }

  ExportJob(model: GenericRequest<MyMokeLongJobInModel>): Observable<GenericResult<MyMokeLongJobOutModel>> {

    return this.http.post<GenericResult<MyMokeLongJobOutModel>>(environment.remoteData.apiUri + 'MyMokeLongJob/ExportJob', model)
      .pipe(
        map(r => {
          return r;
        }
        )
      );

  }

}
