import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import { Az_Cliente_GetAll_InModel, Az_Cliente_GetAll_OutModel } from './Models/az-cliente-model';

@Injectable({
  providedIn: 'root'
})
export class AzClienteService {
  constructor(private http: HttpClient, private authService: AuthService) { }

  GetAll(model: GenericRequest<Az_Cliente_GetAll_InModel>): Observable<GenericResult<Az_Cliente_GetAll_OutModel>> {
    return this.http.post<GenericResult<Az_Cliente_GetAll_OutModel>>(environment.remoteData.apiUri + 'Az_Cliente/GetAll', model)
      .pipe(map(r => r));
  }
}
