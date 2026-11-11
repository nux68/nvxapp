import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { UserDashboardGetInModel, UserDashboardGetOutModel, UserDashboardPutInModel, UserDashboardPutOutModel } from './Models/user-dashboard-model';


// Widget della dashboard dell'utente corrente (ANALISI_INFRASTRUCTURE_WIDGET.md)
@Injectable({
  providedIn: 'root'
})
export class UserDashboardService {

  constructor(private http: HttpClient) { }

  UserDashboardGet(model: GenericRequest<UserDashboardGetInModel>): Observable<GenericResult<UserDashboardGetOutModel>> {
    return this.http.post<GenericResult<UserDashboardGetOutModel>>(environment.remoteData.apiUri + 'UserDashboard/UserDashboardGet', model);
  }

  UserDashboardPut(model: GenericRequest<UserDashboardPutInModel>): Observable<GenericResult<UserDashboardPutOutModel>> {
    return this.http.post<GenericResult<UserDashboardPutOutModel>>(environment.remoteData.apiUri + 'UserDashboard/UserDashboardPut', model);
  }
}
