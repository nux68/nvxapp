import { Injectable } from '@angular/core';
import { HttpClient } from '@angular/common/http';
import { AuthService } from '../../../Utility/infrastructure/auth.service';
import { environment } from '../../../../environments/environment';
import { GenericRequest } from '../../ModelsBase/generic-request';
import { GenericResult } from '../../ModelsBase/generic-result';
import { map, Observable } from 'rxjs';
import {
  Az_Sedi_GetAll_InModel,
  Az_Sedi_GetAll_OutModel,
  Az_SediGetInModel,
  Az_SediGetOutModel,
  Az_SediPutInModel,
  Az_SediPutOutModel,
  Az_SediDeleteInModel,
  Az_SediDeleteOutModel
} from './Models/az-sedi-model';

@Injectable({
  providedIn: 'root'
})
export class AzSediService {

  constructor(private http: HttpClient,
    private authService: AuthService
  ) { }

  GetAll(model: GenericRequest<Az_Sedi_GetAll_InModel>): Observable<GenericResult<Az_Sedi_GetAll_OutModel>> {
    return this.http.post<GenericResult<Az_Sedi_GetAll_OutModel>>(environment.remoteData.apiUri + 'Az_Sedi/GetAll', model)
      .pipe(map(r => r));
  }

  AzSediGet(model: GenericRequest<Az_SediGetInModel>): Observable<GenericResult<Az_SediGetOutModel>> {
    return this.http.post<GenericResult<Az_SediGetOutModel>>(environment.remoteData.apiUri + 'Az_Sedi/AzSediGet', model)
      .pipe(map(r => r));
  }

  AzSediPut(model: GenericRequest<Az_SediPutInModel>): Observable<GenericResult<Az_SediPutOutModel>> {
    return this.http.post<GenericResult<Az_SediPutOutModel>>(environment.remoteData.apiUri + 'Az_Sedi/AzSediPut', model)
      .pipe(map(r => r));
  }

  AzSediDelete(model: GenericRequest<Az_SediDeleteInModel>): Observable<GenericResult<Az_SediDeleteOutModel>> {
    return this.http.post<GenericResult<Az_SediDeleteOutModel>>(environment.remoteData.apiUri + 'Az_Sedi/AzSediDelete', model)
      .pipe(map(r => r));
  }
}
