import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_AttivitaModel, Par_Attivita_GetAll_InModel, Par_Attivita_GetAll_OutModel, Par_AttivitaGetInModel, Par_AttivitaGetOutModel, Par_AttivitaPutInModel, Par_AttivitaPutOutModel, Par_AttivitaDeleteInModel, Par_AttivitaDeleteOutModel } from './Models/par-attivita-model';

@Injectable({
  providedIn: 'root'
})
export class ParAttivitaService {
  constructor(
    private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Par_Attivita_GetAll_InModel>): Observable<GenericResult<Par_Attivita_GetAll_OutModel>> {
    return this.http.post<GenericResult<Par_Attivita_GetAll_OutModel>>(environment.remoteData.apiUri + 'Par_Attivita/GetAll', model)
      .pipe(
        map(r => r)
      );
  }

  Par_AttivitaGet(model: GenericRequest<Par_AttivitaGetInModel>): Observable<GenericResult<Par_AttivitaGetOutModel>> {
    return this.http.post<GenericResult<Par_AttivitaGetOutModel>>(environment.remoteData.apiUri + 'Par_Attivita/Par_AttivitaGet', model)
      .pipe(
        map(r => r)
      );
  }

  Par_AttivitaPut(model: GenericRequest<Par_AttivitaPutInModel>): Observable<GenericResult<Par_AttivitaPutOutModel>> {
    return this.http.post<GenericResult<Par_AttivitaPutOutModel>>(environment.remoteData.apiUri + 'Par_Attivita/Par_AttivitaPut', model)
      .pipe(
        map(r => r)
      );
  }

  Par_AttivitaDelete(model: GenericRequest<Par_AttivitaDeleteInModel>): Observable<GenericResult<Par_AttivitaDeleteOutModel>> {
    return this.http.post<GenericResult<Par_AttivitaDeleteOutModel>>(environment.remoteData.apiUri + 'Par_Attivita/Par_AttivitaDelete', model)
      .pipe(
        map(r => r)
      );
  }
}
