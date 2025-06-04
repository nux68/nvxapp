import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_Commessa_GetAll_InModel, Az_Commessa_GetAll_OutModel, Az_CommessaGetInModel, Az_CommessaGetOutModel, Az_CommessaPutInModel, Az_CommessaPutOutModel, Az_CommessaDeleteInModel, Az_CommessaDeleteOutModel } from './Models/az-commessa-model';





@Injectable({
  providedIn: 'root'
})
export class AzCommessaService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Az_Commessa_GetAll_InModel>): Observable<GenericResult<Az_Commessa_GetAll_OutModel>> {
    return this.http.post<GenericResult<Az_Commessa_GetAll_OutModel>>(environment.remoteData.apiUri + 'Az_Commessa/GetAll', model)
      .pipe(map(r => r));
  }

  Az_CommessaGet(model: GenericRequest<Az_CommessaGetInModel>): Observable<GenericResult<Az_CommessaGetOutModel>> {
    return this.http.post<GenericResult<Az_CommessaGetOutModel>>(environment.remoteData.apiUri + 'Az_Commessa/AZ_CommessaGet', model)
      .pipe(map(r => r));
  }

  Az_CommessaPut(model: GenericRequest<Az_CommessaPutInModel>): Observable<GenericResult<Az_CommessaPutOutModel>> {
    return this.http.post<GenericResult<Az_CommessaPutOutModel>>(environment.remoteData.apiUri + 'Az_Commessa/AZ_CommessaPut', model)
      .pipe(map(r => r));
  }

  Az_CommessaDelete(model: GenericRequest<Az_CommessaDeleteInModel>): Observable<GenericResult<Az_CommessaDeleteOutModel>> {
    return this.http.post<GenericResult<Az_CommessaDeleteOutModel>>(environment.remoteData.apiUri + 'Az_Commessa/Az_CommessaDelete', model)
      .pipe(map(r => r));
  }
}
