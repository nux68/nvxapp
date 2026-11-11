import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  CalendarFeedDeleteInModel, CalendarFeedDeleteOutModel, CalendarFeedGetInModel, CalendarFeedGetOutModel,
  CalendarFeedRegenerateInModel, CalendarFeedRegenerateOutModel
} from './Models/calendar-feed-model';


// Feed iCalendar personale (ANALISI_INFRASTRUCTURE_NUOVI_MODULI.md, INF-CAL.3): il feed .ics si scarica senza autenticazione
// con l'indirizzo restituito; qui solo la gestione (creazione, nuovo indirizzo, eliminazione)
@Injectable({
  providedIn: 'root'
})
export class CalendarFeedService {

  constructor(private http: HttpClient) { }

  CalendarFeedGet(model: GenericRequest<CalendarFeedGetInModel>): Observable<GenericResult<CalendarFeedGetOutModel>> {
    return this.http.post<GenericResult<CalendarFeedGetOutModel>>(environment.remoteData.apiUri + 'CalendarFeed/CalendarFeedGet', model);
  }

  CalendarFeedRegenerate(model: GenericRequest<CalendarFeedRegenerateInModel>): Observable<GenericResult<CalendarFeedRegenerateOutModel>> {
    return this.http.post<GenericResult<CalendarFeedRegenerateOutModel>>(environment.remoteData.apiUri + 'CalendarFeed/CalendarFeedRegenerate', model);
  }

  CalendarFeedDelete(model: GenericRequest<CalendarFeedDeleteInModel>): Observable<GenericResult<CalendarFeedDeleteOutModel>> {
    return this.http.post<GenericResult<CalendarFeedDeleteOutModel>>(environment.remoteData.apiUri + 'CalendarFeed/CalendarFeedDelete', model);
  }
}
