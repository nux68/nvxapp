import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { Observable } from 'rxjs';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import {
  Par_ExportCau_Docs_Delete_InModel, Par_ExportCau_Docs_Delete_OutModel,
  Par_ExportCau_Docs_Get_InModel, Par_ExportCau_Docs_Get_OutModel,
  Par_ExportCau_Docs_GetAll_InModel, Par_ExportCau_Docs_GetAll_OutModel,
  Par_ExportCau_Docs_Put_InModel, Par_ExportCau_Docs_Put_OutModel
} from './Models/par-export-cau-docs-model';

/** documentazione del modello di export (file per l'AI) */
@Injectable({
  providedIn: 'root'
})
export class ParExportCauDocsService {

  constructor(private http: HttpClient) { }

  Par_ExportCau_Docs_GetAll(model: GenericRequest<Par_ExportCau_Docs_GetAll_InModel>): Observable<GenericResult<Par_ExportCau_Docs_GetAll_OutModel>> {
    return this.http.post<GenericResult<Par_ExportCau_Docs_GetAll_OutModel>>(environment.remoteData.apiUri + 'Par_ExportCau_Docs/Par_ExportCau_Docs_GetAll', model);
  }

  Par_ExportCau_Docs_Get(model: GenericRequest<Par_ExportCau_Docs_Get_InModel>): Observable<GenericResult<Par_ExportCau_Docs_Get_OutModel>> {
    return this.http.post<GenericResult<Par_ExportCau_Docs_Get_OutModel>>(environment.remoteData.apiUri + 'Par_ExportCau_Docs/Par_ExportCau_Docs_Get', model);
  }

  Par_ExportCau_Docs_Put(model: GenericRequest<Par_ExportCau_Docs_Put_InModel>): Observable<GenericResult<Par_ExportCau_Docs_Put_OutModel>> {
    return this.http.post<GenericResult<Par_ExportCau_Docs_Put_OutModel>>(environment.remoteData.apiUri + 'Par_ExportCau_Docs/Par_ExportCau_Docs_Put', model);
  }

  Par_ExportCau_Docs_Delete(model: GenericRequest<Par_ExportCau_Docs_Delete_InModel>): Observable<GenericResult<Par_ExportCau_Docs_Delete_OutModel>> {
    return this.http.post<GenericResult<Par_ExportCau_Docs_Delete_OutModel>>(environment.remoteData.apiUri + 'Par_ExportCau_Docs/Par_ExportCau_Docs_Delete', model);
  }
}
