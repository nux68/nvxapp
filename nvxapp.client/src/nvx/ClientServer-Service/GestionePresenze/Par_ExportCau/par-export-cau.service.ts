import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  Par_ExportCau_GetAll_InModel,
  Par_ExportCau_GetAll_OutModel,
  Par_ExportCau_Get_InModel,
  Par_ExportCau_Get_OutModel,
  Par_ExportCau_Put_InModel,
  Par_ExportCau_Put_OutModel,
  Par_ExportCau_Delete_InModel,
  Par_ExportCau_Delete_OutModel
} from './Models/par-export-cau-model';

@Injectable({
  providedIn: 'root'
})
export class ParExportCauService {

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_ExportCau_GetAll_InModel>): Observable<GenericResult<Par_ExportCau_GetAll_OutModel>> {
    return this.http.post<GenericResult<Par_ExportCau_GetAll_OutModel>>(
      environment.remoteData.apiUri + 'Par_ExportCau/GetAll', model
    ).pipe(map(r => r));
  }

  Par_ExportCauGet(model: GenericRequest<Par_ExportCau_Get_InModel>): Observable<GenericResult<Par_ExportCau_Get_OutModel>> {
    return this.http.post<GenericResult<Par_ExportCau_Get_OutModel>>(
      environment.remoteData.apiUri + 'Par_ExportCau/Par_ExportCauGet', model
    ).pipe(map(r => r));
  }

  Par_ExportCauPut(model: GenericRequest<Par_ExportCau_Put_InModel>): Observable<GenericResult<Par_ExportCau_Put_OutModel>> {
    return this.http.post<GenericResult<Par_ExportCau_Put_OutModel>>(
      environment.remoteData.apiUri + 'Par_ExportCau/Par_ExportCauPut', model
    ).pipe(map(r => r));
  }

  Par_ExportCauDelete(model: GenericRequest<Par_ExportCau_Delete_InModel>): Observable<GenericResult<Par_ExportCau_Delete_OutModel>> {
    return this.http.post<GenericResult<Par_ExportCau_Delete_OutModel>>(
      environment.remoteData.apiUri + 'Par_ExportCau/Par_ExportCauDelete', model
    ).pipe(map(r => r));
  }
}
