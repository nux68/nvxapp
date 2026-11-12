import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { Widget_PresenzeOggiGetInModel, Widget_PresenzeOggiGetOutModel } from './Models/dip-widget-model';

// Dati dei widget della dashboard delle presenze (ANALISI_ATTENDANCETRACKING_WIDGET.md)
@Injectable({
  providedIn: 'root'
})
export class DipWidgetService {

  constructor(private http: HttpClient) { }

  PresenzeOggiGet(model: GenericRequest<Widget_PresenzeOggiGetInModel>): Observable<GenericResult<Widget_PresenzeOggiGetOutModel>> {
    return this.http.post<GenericResult<Widget_PresenzeOggiGetOutModel>>(environment.remoteData.apiUri + 'Dip_Widget/Widget_PresenzeOggiGet', model);
  }

}
