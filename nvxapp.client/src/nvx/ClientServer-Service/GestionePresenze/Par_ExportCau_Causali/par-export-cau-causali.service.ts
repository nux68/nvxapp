import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  Par_ExportCau_Causali_Get_InModel,
  Par_ExportCau_Causali_Get_OutModel,
  Par_ExportCau_Causali_Put_InModel,
  Par_ExportCau_Causali_Put_OutModel
} from './Models/par-export-cau-causali-model';

@Injectable({
  providedIn: 'root'
})
export class ParExportCauCausaliService {

  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  Par_ExportCau_Causali_Get(model: GenericRequest<Par_ExportCau_Causali_Get_InModel>): Observable<GenericResult<Par_ExportCau_Causali_Get_OutModel>> {
    return this.http.post<GenericResult<Par_ExportCau_Causali_Get_OutModel>>(
      environment.remoteData.apiUri + 'Par_ExportCau_Causali/Par_ExportCau_Causali_Get', model
    ).pipe(map(r => r));
  }

  Par_ExportCau_Causali_Put(model: GenericRequest<Par_ExportCau_Causali_Put_InModel>): Observable<GenericResult<Par_ExportCau_Causali_Put_OutModel>> {
    return this.http.post<GenericResult<Par_ExportCau_Causali_Put_OutModel>>(
      environment.remoteData.apiUri + 'Par_ExportCau_Causali/Par_ExportCau_Causali_Put', model
    ).pipe(map(r => r));
  }
}
