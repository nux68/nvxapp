import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_Competenza_GetAll_InModel, Az_Competenza_GetAll_OutModel, Az_CompetenzaPutInModel, Az_CompetenzaPutOutModel, Az_CompetenzaModel } from './Models/az-competenza-model';

@Injectable({
  providedIn: 'root'
})
export class AzCompetenzaService {
  constructor(private http: HttpClient, private authService: AuthService) { }

  GetAll(model: GenericRequest<Az_Competenza_GetAll_InModel>): Observable<GenericResult<Az_Competenza_GetAll_OutModel>> {
    return this.http.post<GenericResult<Az_Competenza_GetAll_OutModel>>(environment.remoteData.apiUri + 'Az_Competenza/GetAll', model)
      .pipe(map(r => r));
  }

  Az_CompetenzaGet(model: GenericRequest<any>): Observable<GenericResult<any>> {
    return this.http.post<GenericResult<any>>(environment.remoteData.apiUri + 'Az_Competenza/Az_CompetenzaGet', model)
      .pipe(map(r => r));
  }

  Az_CompetenzaPut(model: GenericRequest<any>): Observable<GenericResult<any>> {
    return this.http.post<GenericResult<any>>(environment.remoteData.apiUri + 'Az_Competenza/Az_CompetenzaPut', model)
      .pipe(map(r => r));
  }
}
