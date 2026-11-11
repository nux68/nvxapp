import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../environments/environment';
import { GenericRequest } from '../ModelsBase/generic-request';
import { GenericResult } from '../ModelsBase/generic-result';
import { DocSpazioPerTipoGetInModel, DocSpazioPerTipoGetOutModel } from './Models/doc-widget-model';

// Dati dei widget della dashboard del documentale (ANALISI_DOCUMENTMANAGEMENT_WIDGET.md)
@Injectable({
  providedIn: 'root'
})
export class DocWidgetService {

  constructor(private http: HttpClient) { }

  SpazioPerTipoGet(model: GenericRequest<DocSpazioPerTipoGetInModel>): Observable<GenericResult<DocSpazioPerTipoGetOutModel>> {
    return this.http.post<GenericResult<DocSpazioPerTipoGetOutModel>>(environment.remoteData.apiUri + 'Doc_Widget/Doc_SpazioPerTipoGet', model);
  }
}
