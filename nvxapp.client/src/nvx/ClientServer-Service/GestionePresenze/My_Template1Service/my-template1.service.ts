import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { My_template1_GetAllInModel, My_template1_GetAllOutModel, My_template1_GetInModel, My_template1_GetOutModel, My_template1_PutInModel, My_template1_PutOutModel } from './Models/my-template1-model';

@Injectable({
  providedIn: 'root'
})
export class MyTemplate1Service {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<My_template1_GetAllInModel>): Observable<GenericResult<My_template1_GetAllOutModel>> {
    return this.http.post<GenericResult<My_template1_GetAllOutModel>>(environment.remoteData.apiUri + 'My_Template1/GetAll', model)
      .pipe(map(r => r));
  }

  MyTemplate1Get(model: GenericRequest<My_template1_GetInModel>): Observable<GenericResult<My_template1_GetOutModel>> {
    return this.http.post<GenericResult<My_template1_GetOutModel>>(environment.remoteData.apiUri + 'My_Template1/MyTemplate1Get', model)
      .pipe(map(r => r));
  }

  MyTemplate1Put(model: GenericRequest<My_template1_PutInModel>): Observable<GenericResult<My_template1_PutOutModel>> {
    return this.http.post<GenericResult<My_template1_PutOutModel>>(environment.remoteData.apiUri + 'My_Template1/MyTemplate1Put', model)
      .pipe(map(r => r));
  }

}
