import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_AttivitaCompetenza_GetAll_InModel, Par_AttivitaCompetenza_GetAll_OutModel } from './Models/par-attivita-competenza-model';

@Injectable({
  providedIn: 'root'
})
export class ParAttivitaCompetenzaService {
  constructor(private http: HttpClient, private authService: AuthService) { }

  GetAll(model: GenericRequest<Par_AttivitaCompetenza_GetAll_InModel>): Observable<GenericResult<Par_AttivitaCompetenza_GetAll_OutModel>> {
    return this.http.post<GenericResult<Par_AttivitaCompetenza_GetAll_OutModel>>(environment.remoteData.apiUri + 'Par_AttivitaCompetenza/GetAll', model)
      .pipe(map(r => r));
  }
}
