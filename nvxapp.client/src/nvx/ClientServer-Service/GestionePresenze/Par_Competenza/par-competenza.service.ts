import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Par_Competenza_GetAll_InModel, Par_Competenza_GetAll_OutModel, Par_CompetenzaPutInModel, Par_CompetenzaPutOutModel, Par_CompetenzaModel, Par_CompetenzaGetInModel, Par_CompetenzaGetOutModel, Par_CompetenzaDeleteInModel, Par_CompetenzaDeleteOutModel } from './Models/par-competenza-model';


@Injectable({
  providedIn: 'root'
})
export class ParCompetenzaService {
  constructor(private http: HttpClient,
              private authService: AuthService) { }

  GetAll(model: GenericRequest<Par_Competenza_GetAll_InModel>): Observable<GenericResult<Par_Competenza_GetAll_OutModel>> {
    return this.http.post<GenericResult<Par_Competenza_GetAll_OutModel>>(environment.remoteData.apiUri + 'Par_Competenza/GetAll', model)
      .pipe(map(r => r));
  }

  Par_CompetenzaGet(model: GenericRequest<Par_CompetenzaGetInModel>): Observable<GenericResult<Par_CompetenzaGetOutModel>> {
    return this.http.post<GenericResult<Par_CompetenzaGetOutModel>>(environment.remoteData.apiUri + 'Par_Competenza/Par_CompetenzaGet', model)
      .pipe(map(r => r));
  }

  Par_CompetenzaPut(model: GenericRequest<Par_CompetenzaPutInModel>): Observable<GenericResult<Par_CompetenzaPutOutModel>> {
    return this.http.post<GenericResult<Par_CompetenzaPutOutModel>>(environment.remoteData.apiUri + 'Par_Competenza/Par_CompetenzaPut', model)
      .pipe(map(r => r));
  }

  Par_CompetenzaDelete(model: GenericRequest<Par_CompetenzaDeleteInModel>): Observable<GenericResult<Par_CompetenzaDeleteOutModel>> {
    return this.http.post<GenericResult<Par_CompetenzaDeleteOutModel>>(environment.remoteData.apiUri + 'Par_Competenza/Par_CompetenzaDelete', model)
      .pipe(map(r => r));
  }


}
