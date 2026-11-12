import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { CmpEmptyInModel } from './Models/cmp-model';
import { CmpWidgetGdprGetOutModel, CmpWidgetScadenzeGetOutModel } from './Models/cmp-widget-model';

// Dati dei widget della dashboard di Compliance (ANALISI_COMPLIANCE_WIDGET.md): controller Cmp_Widget
@Injectable({
  providedIn: 'root'
})
export class CmpWidgetService {

  constructor(private http: HttpClient) { }

  ScadenzeGet(model: GenericRequest<CmpEmptyInModel>): Observable<GenericResult<CmpWidgetScadenzeGetOutModel>> {
    return this.http.post<GenericResult<CmpWidgetScadenzeGetOutModel>>(environment.remoteData.apiUri + 'Cmp_Widget/Cmp_WidgetScadenzeGet', model);
  }

  GdprGet(model: GenericRequest<CmpEmptyInModel>): Observable<GenericResult<CmpWidgetGdprGetOutModel>> {
    return this.http.post<GenericResult<CmpWidgetGdprGetOutModel>>(environment.remoteData.apiUri + 'Cmp_Widget/Cmp_WidgetGdprGet', model);
  }
}
