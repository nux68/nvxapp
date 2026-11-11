import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  NotificationCountInModel, NotificationCountOutModel, NotificationDeleteInModel, NotificationDeleteOutModel, NotificationListInModel,
  NotificationListOutModel, NotificationPreferenceGetInModel, NotificationPreferenceGetOutModel, NotificationPreferencePutInModel,
  NotificationPreferencePutOutModel, NotificationReadInModel, NotificationReadOutModel
} from './Models/notification-model';


// Notifiche dell'utente corrente e preferenze per categoria (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-SVC.4)
@Injectable({
  providedIn: 'root'
})
export class NotificationService {

  constructor(private http: HttpClient) { }

  NotificationList(model: GenericRequest<NotificationListInModel>): Observable<GenericResult<NotificationListOutModel>> {
    return this.http.post<GenericResult<NotificationListOutModel>>(environment.remoteData.apiUri + 'Notification/NotificationList', model);
  }

  NotificationCount(model: GenericRequest<NotificationCountInModel>): Observable<GenericResult<NotificationCountOutModel>> {
    return this.http.post<GenericResult<NotificationCountOutModel>>(environment.remoteData.apiUri + 'Notification/NotificationCount', model);
  }

  NotificationRead(model: GenericRequest<NotificationReadInModel>): Observable<GenericResult<NotificationReadOutModel>> {
    return this.http.post<GenericResult<NotificationReadOutModel>>(environment.remoteData.apiUri + 'Notification/NotificationRead', model);
  }

  NotificationDelete(model: GenericRequest<NotificationDeleteInModel>): Observable<GenericResult<NotificationDeleteOutModel>> {
    return this.http.post<GenericResult<NotificationDeleteOutModel>>(environment.remoteData.apiUri + 'Notification/NotificationDelete', model);
  }

  NotificationPreferenceGet(model: GenericRequest<NotificationPreferenceGetInModel>): Observable<GenericResult<NotificationPreferenceGetOutModel>> {
    return this.http.post<GenericResult<NotificationPreferenceGetOutModel>>(environment.remoteData.apiUri + 'Notification/NotificationPreferenceGet', model);
  }

  NotificationPreferencePut(model: GenericRequest<NotificationPreferencePutInModel>): Observable<GenericResult<NotificationPreferencePutOutModel>> {
    return this.http.post<GenericResult<NotificationPreferencePutOutModel>>(environment.remoteData.apiUri + 'Notification/NotificationPreferencePut', model);
  }
}
