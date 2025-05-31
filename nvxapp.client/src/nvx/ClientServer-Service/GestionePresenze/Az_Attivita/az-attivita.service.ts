import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_AttivitaModel, Az_Attivita_GetAll_InModel, Az_Attivita_GetAll_OutModel, Az_AttivitaGetInModel, Az_AttivitaGetOutModel, Az_AttivitaPutInModel, Az_AttivitaPutOutModel } from './Models/az-attivita-model';

@Injectable({
  providedIn: 'root'
})
export class AzAttivitaService {
  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Az_Attivita_GetAll_InModel>): Observable<GenericResult<Az_Attivita_GetAll_OutModel>> {
    return this.http.post<GenericResult<Az_Attivita_GetAll_OutModel>>(environment.remoteData.apiUri + 'Az_Attivita/GetAll', model)
      .pipe(
        map(r => r)
      );
  }

  Az_AttivitaGet(model: GenericRequest<Az_AttivitaGetInModel>): Observable<GenericResult<Az_AttivitaGetOutModel>> {
    return this.http.post<GenericResult<Az_AttivitaGetOutModel>>(environment.remoteData.apiUri + 'Az_Attivita/Az_AttivitaGet', model)
      .pipe(
        map(r => r)
      );
  }

  Az_AttivitaPut(model: GenericRequest<Az_AttivitaPutInModel>): Observable<GenericResult<Az_AttivitaPutOutModel>> {
    return this.http.post<GenericResult<Az_AttivitaPutOutModel>>(environment.remoteData.apiUri + 'Az_Attivita/Az_AttivitaPut', model)
      .pipe(
        map(r => r)
      );
  }
}
