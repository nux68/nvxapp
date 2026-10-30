import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  DocumentDeleteInModel, DocumentDeleteOutModel, DocumentFileGetInModel, DocumentFileGetOutModel, DocumentFileTextInModel, DocumentFileTextOutModel,
  DocumentGetInModel, DocumentGetOutModel, DocumentListInModel, DocumentListOutModel, DocumentPutInModel, DocumentPutOutModel,
  DocumentScopeInModel, DocumentScopeOutModel
} from './Models/document-model';


// Documenti dei livelli (es. documentazione per le risposte AI ai ticket): livello e permessi li decide il server
@Injectable({
  providedIn: 'root'
})
export class DocumentService {

  constructor(private http: HttpClient) { }

  DocumentScope(model: GenericRequest<DocumentScopeInModel>): Observable<GenericResult<DocumentScopeOutModel>> {
    return this.http.post<GenericResult<DocumentScopeOutModel>>(environment.remoteData.apiUri + 'Document/DocumentScope', model);
  }

  DocumentList(model: GenericRequest<DocumentListInModel>): Observable<GenericResult<DocumentListOutModel>> {
    return this.http.post<GenericResult<DocumentListOutModel>>(environment.remoteData.apiUri + 'Document/DocumentList', model);
  }

  DocumentGet(model: GenericRequest<DocumentGetInModel>): Observable<GenericResult<DocumentGetOutModel>> {
    return this.http.post<GenericResult<DocumentGetOutModel>>(environment.remoteData.apiUri + 'Document/DocumentGet', model);
  }

  DocumentPut(model: GenericRequest<DocumentPutInModel>): Observable<GenericResult<DocumentPutOutModel>> {
    return this.http.post<GenericResult<DocumentPutOutModel>>(environment.remoteData.apiUri + 'Document/DocumentPut', model);
  }

  DocumentDelete(model: GenericRequest<DocumentDeleteInModel>): Observable<GenericResult<DocumentDeleteOutModel>> {
    return this.http.post<GenericResult<DocumentDeleteOutModel>>(environment.remoteData.apiUri + 'Document/DocumentDelete', model);
  }

  DocumentFileText(model: GenericRequest<DocumentFileTextInModel>): Observable<GenericResult<DocumentFileTextOutModel>> {
    return this.http.post<GenericResult<DocumentFileTextOutModel>>(environment.remoteData.apiUri + 'Document/DocumentFileText', model);
  }

  DocumentFileGet(model: GenericRequest<DocumentFileGetInModel>): Observable<GenericResult<DocumentFileGetOutModel>> {
    return this.http.post<GenericResult<DocumentFileGetOutModel>>(environment.remoteData.apiUri + 'Document/DocumentFileGet', model);
  }
}
